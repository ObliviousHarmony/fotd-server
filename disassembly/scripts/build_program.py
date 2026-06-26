# build_program.py — reconstruct ONE labeled program from sharded JSON onto a FRESH import.
#
# Headless:  analyzeHeadless <proj> <name> -import <binary> -postScript build_program.py
# (the build.* wrapper imports each binary from the game dir and runs this)
#
# Order: generate shared .gdt archives (SharedLib, RakNet) from types/ and apply them (gives the
# type<->archive linkage) -> reconstruct this binary's LOCAL types into the program -> apply the
# sharded symbols (names, namespace relocations, signatures, typed globals, comments).
import json, os, glob, re, traceback
import jpype
from ghidra.program.model.symbol import SourceType, SymbolType
from ghidra.program.model.data import (FileDataTypeManager, DataTypeConflictHandler, CategoryPath,
    StructureDataType, UnionDataType, EnumDataType, TypedefDataType, FunctionDefinitionDataType,
    ArrayDataType, Undefined4DataType, DataType, Structure, Pointer, Array,
    ParameterDefinition, ParameterDefinitionImpl, BuiltInDataTypeManager)
from ghidra.program.model.listing import ParameterImpl, ReturnParameterImpl
from ghidra.program.model.listing.Function import FunctionUpdateType
from ghidra.app.cmd.function import CreateFunctionCmd
from ghidra.program.database import DataTypeArchiveDB
from java.io import File
from java.util import ArrayList
from java.lang import Object as _JObject

EXPECTED_GHIDRA_VERSION="12.0.4"
BASE=os.path.dirname(os.path.dirname(str(getSourceFile().getAbsolutePath())))   # <repo>/ghidra-export
US=SourceType.USER_DEFINED
prog=currentProgram
def _check_version():
    from ghidra.framework import Application
    r=str(Application.getApplicationVersion())
    if r!=EXPECTED_GHIDRA_VERSION: raise RuntimeError("GHIDRA VERSION MISMATCH — pinned to %s but running %s."%(EXPECTED_GHIDRA_VERSION,r))
_check_version()
st=prog.getSymbolTable(); fm=prog.getFunctionManager(); listing=prog.getListing(); pdtm=prog.getDataTypeManager(); g=prog.getGlobalNamespace()
PROG=str(prog.getName()); UNDEF=Undefined4DataType.dataType
H=DataTypeConflictHandler.REPLACE_HANDLER
BIM=BuiltInDataTypeManager.getDataTypeManager()    # builtins (float, etc.) may not be materialized yet
ParamArr=jpype.JArray(ParameterDefinition)
TYPES=os.path.join(BASE,"types"); SYM=os.path.join(BASE,"symbols",PROG)
CONSUMER=_JObject(); SHARED_ARCHIVE="SharedLib"    # the in-project data-type archive holding shared types
def cat_of(path): return path.rsplit("/",1)[0] if "/" in path[1:] else "/"
def setdesc(dt,desc):
    if desc:
        try: dt.setDescription(desc)
        except Exception: pass

# ---- load type shards: {path: (bucket, entry)} ----
def load_types():
    # Load the shared buckets (SharedLib/RakNet/stdint.h/...) + THIS program's own local bucket;
    # skip other binaries' local buckets (named like Object.lto) so their types don't leak in.
    out={}
    for bdir in glob.glob(os.path.join(TYPES,"*")):
        if not os.path.isdir(bdir): continue
        b=os.path.basename(bdir)
        if b!=PROG and re.search(r"\.(dll|lto|exe)$",b): continue
        for fp in glob.glob(os.path.join(bdir,"**","*.json"),recursive=True):
            e=json.load(open(fp,encoding="utf-8")); out[e["path"]]=(b,e)
    return out

# ---- reconstruct a set of types into target DTM (resolve against target, then borrow from program) ----
def reconstruct(target, entries):
    entries={k:t for k,t in entries.items() if "kind" in t}   # skip non-reconstructible builtins (e.g. /undefined)
    def resolve(ref):
        if ref is None: return None
        if isinstance(ref,dict):
            if "ptr" in ref: b=resolve(ref["ptr"]); return target.getPointer(b) if b is not None else None
            if "arr" in ref: el=resolve(ref["arr"]); return ArrayDataType(el,int(ref["n"]),el.getLength()) if el is not None else None
            return None
        dt=target.getDataType(ref)
        if dt is not None: return dt
        pdt=pdtm.getDataType(ref)
        if pdt is None: pdt=BIM.getDataType(ref)            # unmaterialized builtin (e.g. /float)
        return target.resolve(pdt,H) if pdt is not None else None
    errs=[]
    for path,t in entries.items():                 # pass 1: shells
        try:
            name=path.split("/")[-1]; catp=CategoryPath(cat_of(path)); k=t["kind"]
            if k=="struct": target.addDataType(StructureDataType(catp,name,int(t["len"]),target),H)
            elif k=="union": target.addDataType(UnionDataType(catp,name,target),H)
            elif k=="enum": target.addDataType(EnumDataType(catp,name,int(t["len"]),target),H)
            elif k=="funcdef": target.addDataType(FunctionDefinitionDataType(catp,name,target),H)
        except Exception: errs.append("shell %s: %s"%(path,traceback.format_exc().splitlines()[-1]))
    # pass 1b: typedefs (fixpoint — base may be a builtin, a shell, or another typedef; create in
    # dependency order so structs that reference a typedef find it during fill)
    todo=[(p,t) for p,t in entries.items() if t["kind"]=="typedef"]
    while todo:
        rest=[]; made=False
        for path,t in todo:
            base=resolve(t["base"])
            if base is not None:
                td=TypedefDataType(CategoryPath(cat_of(path)),path.split("/")[-1],base,target)
                setdesc(td,t.get("desc")); target.addDataType(td,H); made=True
            else: rest.append((path,t))
        if not made:
            for path,t in rest: errs.append("typedef-unresolved %s"%path)
            break
        todo=rest
    for path,t in entries.items():                 # pass 2: fill (explicit offsets, then flip packed)
        try:
            name=path.split("/")[-1]; catp=CategoryPath(cat_of(path)); k=t["kind"]
            if k=="struct":
                sd=target.getDataType(path)
                for f in t["fields"]: sd.replaceAtOffset(f["offset"],resolve(f["type"]),f["len"],f["name"],f.get("comment"))
                if t.get("packed"):
                    if t.get("explicitPack") is not None: sd.setExplicitPackingValue(t["explicitPack"])
                    if t.get("explicitAlign") is not None: sd.setExplicitMinimumAlignment(t["explicitAlign"])
                    sd.setPackingEnabled(True)
                setdesc(sd,t.get("desc"))
            elif k=="union":
                ud=target.getDataType(path)
                for f in t["fields"]: ud.add(resolve(f["type"]),f["len"],f["name"],f.get("comment"))
                if t.get("packed"): ud.setPackingEnabled(True)
                setdesc(ud,t.get("desc"))
            elif k=="enum":
                ed=target.getDataType(path)
                for e in t["entries"]: ed.add(e["name"],int(e["value"]),e.get("comment"))
                setdesc(ed,t.get("desc"))
            elif k=="typedef": pass        # already created in pass 1b
            elif k=="funcdef":
                fd=target.getDataType(path); fd.setReturnType(resolve(t["return"]))
                fd.setArguments(ParamArr([ParameterDefinitionImpl(pa["name"],resolve(pa["type"]),None) for pa in t["params"]]))
                setdesc(fd,t.get("desc"))
        except Exception: errs.append("fill %s: %s"%(path,traceback.format_exc().splitlines()[-1]))
    return errs

def build_types(types):
    # Shared types live in a PROJECT data-type archive ("SharedLib") created INSIDE the project — not
    # an external .gdt file. Programs link to it by project identity (UniversalID), so the linkage
    # resolves whenever the project is open: no filesystem path to lose, no "archive file not found".
    # The first binary of a build creates+populates it (ALL shared buckets together so their cross-
    # references resolve in one archive); later binaries reuse the same one. Local types reconstruct
    # straight into the program (they reference the shared types, now present).
    shared={k:e for k,(b,e) in types.items() if b!=PROG}
    local ={k:e for k,(b,e) in types.items() if b==PROG}
    errs=[]
    root=state.getProject().getProjectData().getRootFolder()
    df=root.getFile(SHARED_ARCHIVE); created=(df is None)
    if created:                                  # first binary of the build: create + fill the archive
        archive=DataTypeArchiveDB(root, SHARED_ARCHIVE, CONSUMER)
        adtm=archive.getDataTypeManager()
        atx=archive.startTransaction("populate shared types")
        errs+=reconstruct(adtm, shared)
        archive.endTransaction(atx, True)
        archive.save("shared types", monitor)
    else:                                        # later binaries: reuse the one already in the project
        archive=df.getDomainObject(CONSUMER, False, False, monitor)
        adtm=archive.getDataTypeManager()
    print("  shared archive '%s' %s -> linking %d shared types"%(SHARED_ARCHIVE,"CREATED" if created else "REUSED",len(shared)))
    for dt in adtm.getAllDataTypes(): pdtm.addDataType(dt,H)   # link -> program copies sourced from the archive
    archive.release(CONSUMER)
    errs+=reconstruct(pdtm, local)
    return errs

# ============================ SYMBOLS ============================
def load_symbols():
    src={"namespaces":[],"functions":[],"data":[],"comments":[],"bookmarks":[]}
    npj=os.path.join(SYM,"namespaces.json")
    if os.path.exists(npj): src["namespaces"]=json.load(open(npj,encoding="utf-8"))
    for fp in glob.glob(os.path.join(SYM,"**","*.json"),recursive=True):
        rel=os.path.relpath(fp,SYM).replace("\\","/")
        if rel in ("_meta.json","namespaces.json"): continue
        obj=json.load(open(fp,encoding="utf-8"))
        if rel=="_comments.json": src["comments"]=obj
        elif rel=="_bookmarks.json": src["bookmarks"]=obj
        elif rel.startswith("_data/"): src["data"]+=obj
        elif isinstance(obj,dict): src["functions"]+=obj.get("functions",[]); src["data"]+=obj.get("data",[])
    return src

def rtype(path):
    if not path: return None
    path=str(path).strip()
    if path=="/undefined": return DataType.DEFAULT
    dt=pdtm.getDataType(path)
    if dt is not None: return dt
    bdt=BIM.getDataType(path)
    if bdt is not None: return pdtm.resolve(bdt,H)          # unmaterialized builtin (e.g. /float)
    if path.endswith(" *"):
        b=rtype(path[:-2]); return pdtm.getPointer(b) if b is not None else None
    m2=re.match(r"^(.*?)\s*\*\s*\d+$",path)        # sized pointer "Foo *32"
    if m2:
        b=rtype(m2.group(1)); return pdtm.getPointer(b) if b is not None else None
    m=re.match(r"^(.*)\[(\d+)\]$",path)            # array "Foo[N]" (recurses into "Foo *32")
    if m:
        b=rtype(m.group(1)); return ArrayDataType(b,int(m.group(2)),b.getLength()) if b is not None else None
    return None
def rns(path):
    if not path: return g
    cur=g
    for part in str(path).split("::"):
        n=st.getNamespace(part,cur)
        if n is None: n=st.createNameSpace(cur,part,US)
        cur=n
    return cur
CT={"plate":3,"pre":1,"eol":0,"post":2,"repeat":4}
def apply_symbols(src):
    r={"ns":0,"ns_moved":0,"fn":0,"sig":0,"lbl":0,"dtyped":0,"thunk":0,"cmt":0,"errs":[]}
    nsmap={}
    for s in st.getDefinedSymbols():
        if s.getSymbolType() in (SymbolType.CLASS,SymbolType.NAMESPACE): nsmap.setdefault(str(s.getName()),[]).append(s)
    for e in sorted(src["namespaces"], key=lambda x:x["path"].count("::")):
        try:
            parent=rns(e["parent"])
            if st.getNamespace(e["name"],parent) is not None: continue
            moved=False
            for s in nsmap.get(e["name"],[]):
                if s.getSymbolType() in (SymbolType.CLASS,SymbolType.NAMESPACE) and not s.isDeleted():
                    s.setNameAndNamespace(e["name"],parent,US); moved=True; break
            if moved: r["ns_moved"]+=1
            elif e["kind"]=="class": st.createClass(parent,e["name"],US); r["ns"]+=1
            else: st.createNameSpace(parent,e["name"],US); r["ns"]+=1
        except Exception as ex: r["errs"].append("ns %s: %s"%(e["path"],ex))
    for e in src["functions"]:
        try:
            addr=toAddr(int(e["addr"],16)); f=fm.getFunctionAt(addr)
            if f is None:
                CreateFunctionCmd(addr).applyTo(prog); f=fm.getFunctionAt(addr)
                if f is None: continue
            f.getSymbol().setNameAndNamespace(e["name"],rns(e["namespace"]),US); r["fn"]+=1
            ret=rtype(e["returnType"]) or UNDEF
            plist=ArrayList()
            for pe in e["params"]: plist.add(ParameterImpl(pe["name"],rtype(pe["type"]) or UNDEF,prog))
            conv=e["callingConvention"]; conv=None if conv in (None,"unknown") else conv
            # ALL_PARAMS: the captured list already includes the auto 'this' (from getParameters),
            # so tell Ghidra not to synthesize another — FORMAL_PARAMS duplicated 'this' on some thiscalls.
            f.updateFunction(conv,ReturnParameterImpl(ret,prog),plist,FunctionUpdateType.DYNAMIC_STORAGE_ALL_PARAMS,True,US); r["sig"]+=1
            # Ghidra inserts a spurious 'this:undefined4' on thiscalls whose class is an empty RTTI
            # stub. Strip any 'this'-named param past index 0, then re-align the remaining names.
            got=f.getParameters()
            if len(got)>len(e["params"]):
                for i in range(len(got)-1,0,-1):
                    if str(got[i].getName())=="this": f.removeParameter(i); break
                g=f.getParameters()
                for i,pe in enumerate(e["params"]):
                    if i<len(g) and str(g[i].getName())!=pe["name"]:
                        try: g[i].setName(pe["name"],US)
                        except Exception: pass
        except Exception as ex: r["errs"].append("fn %s: %s"%(e["addr"],ex))
    for e in src["functions"]:           # thunks: 2nd pass so the forwarded-to function already exists
        if not e.get("thunk"): continue
        try:
            f=fm.getFunctionAt(toAddr(int(e["addr"],16))); tgt=fm.getFunctionAt(toAddr(int(e["thunk"],16)))
            if f is not None and tgt is not None: f.setThunkedFunction(tgt); r["thunk"]=r.get("thunk",0)+1
        except Exception as ex: r["errs"].append("thunk %s: %s"%(e["addr"],ex))
    for d in src["data"]:
        addr=toAddr(int(d["addr"],16))
        try: st.createLabel(addr,d["name"],rns(d["namespace"]),US); r["lbl"]+=1
        except Exception as ex: r["errs"].append("lbl %s: %s"%(d["addr"],ex))
        if d.get("type"):
            t=rtype(d["type"])
            if t is not None:
                try:
                    listing.clearCodeUnits(addr,addr.add(max(0,t.getLength()-1)),False); listing.createData(addr,t); r["dtyped"]+=1
                except Exception: pass
    for c in src["comments"]:
        try: listing.setComment(toAddr(int(c["addr"],16)),CT[c["kind"]],c["text"]); r["cmt"]+=1
        except Exception: pass
    return r

# ============================ RUN ============================
types=load_types()
tx=prog.startTransaction("build"); ok=True; terrs=[]; sinfo={"errs":[]}
try:
    terrs=build_types(types)
    sym=load_symbols()
    sinfo=apply_symbols(sym)
except Exception:
    ok=False; traceback.print_exc(); sinfo={"errs":["FATAL"]}
finally:
    prog.endTransaction(tx,ok)
print("BUILD %s ok=%s"%(PROG,ok))
print("  types: %d reconstructed, %d errors"%(len(types),len(terrs)))
for e in terrs[:10]: print("      type-err:",e)
print("  symbols: ns+%d moved=%d fn=%d sig=%d data=%d typed=%d thunk=%d cmt=%d errs=%d"%(
    sinfo.get("ns",0),sinfo.get("ns_moved",0),sinfo.get("fn",0),sinfo.get("sig",0),sinfo.get("lbl",0),
    sinfo.get("dtyped",0),sinfo.get("thunk",0),sinfo.get("cmt",0),len(sinfo.get("errs",[]))))
for e in sinfo.get("errs",[])[:8]: print("     ",e)
