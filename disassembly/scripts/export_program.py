# export_program.py — export ONE labeled program to diffable, sharded JSON.
#
# Headless:  analyzeHeadless <proj> <name> -process <prog> -noanalysis -postScript export_program.py
# (the export.* wrapper runs this for every program in the project)
#
# Produces, under ghidra-export/:
#   types/<bucket>/<category>/<Name>.json   one file per /FOM + /RakNet data type
#   symbols/<program>/...                   namespace-sharded function/data/namespace labeling
# JSON is the committed source of truth; the .gdt archives are generated at build time.
import json, os, glob, shutil, re
from ghidra.program.model.symbol import SourceType, SymbolType
from ghidra.program.model.data import (Structure, Union, Enum, TypeDef, FunctionDefinition,
    Pointer, Array, BitFieldDataType, Composite, BuiltInDataType)
from ghidra.program.model.address import AddressSet
from ghidra.program.model.listing import CodeUnit

EXPECTED_GHIDRA_VERSION = "12.0.4"
BASE = os.path.dirname(os.path.dirname(str(getSourceFile().getAbsolutePath())))   # <repo>/ghidra-export
USER_TREES = ("FOM", "RakNet")
AUTO_DESCS = {"Function Signature Data Type"}      # Ghidra's auto funcdef description (not user content)
US = SourceType.USER_DEFINED

p = currentProgram
def _check_version():
    from ghidra.framework import Application
    running = str(Application.getApplicationVersion())
    if running != EXPECTED_GHIDRA_VERSION:
        raise RuntimeError("GHIDRA VERSION MISMATCH — pinned to %s but running %s." % (EXPECTED_GHIDRA_VERSION, running))
_check_version()
st=p.getSymbolTable(); fm=p.getFunctionManager(); listing=p.getListing(); pdtm=p.getDataTypeManager()
PROG=str(p.getName())

def opt(fn):
    try: return fn()
    except Exception: return None
def nz(s):
    s=str(s) if s is not None else None; return s if s else None
def desc_of(dt):
    d=nz(dt.getDescription()); return None if (d in AUTO_DESCS) else d

# ============================ TYPES ============================
def tref(d):
    if d is None: return None
    if isinstance(d,Pointer): return {"ptr":tref(d.getDataType())}
    if isinstance(d,Array):   return {"arr":tref(d.getDataType()),"n":d.getNumElements()}
    return str(d.getPathName())
def bucket_of(dt):
    sa=dt.getSourceArchive(); nm=sa.getName() if sa else None
    cp=str(dt.getCategoryPath().getPath())
    if nm=="RakNet" or cp.startswith("/RakNet"): return "RakNet"
    if nm=="SharedLib": return "SharedLib"               # /FOM shared + the /std container types
    if cp.startswith("/FOM"): return PROG                # this binary's own /FOM types
    return (cp.strip("/").split("/")[0] or "external")   # external deps: stdint.h, crtdefs.h, ...
def type_entry(dt):
    e={"path":str(dt.getPathName()),"len":dt.getLength(),"desc":desc_of(dt)}
    if isinstance(dt,(Structure,Union)):
        e["kind"]="struct" if isinstance(dt,Structure) else "union"; e["packed"]=bool(dt.isPackingEnabled())
        e["explicitPack"]=opt(lambda: dt.getExplicitPackingValue() if dt.hasExplicitPackingValue() else None)
        e["explicitAlign"]=opt(lambda: dt.getExplicitMinimumAlignment() if dt.hasExplicitMinimumAlignment() else None)
        e["fields"]=[{"offset":c.getOffset(),"name":c.getFieldName(),"type":tref(c.getDataType()),
                      "len":c.getLength(),"comment":nz(c.getComment())} for c in dt.getDefinedComponents()]
    elif isinstance(dt,Enum):
        e["kind"]="enum"; e["entries"]=[{"name":str(n),"value":dt.getValue(n),"comment":nz(dt.getComment(n))} for n in dt.getNames()]
    elif isinstance(dt,TypeDef):
        e["kind"]="typedef"; e["base"]=tref(dt.getDataType())
    elif isinstance(dt,FunctionDefinition):
        e["kind"]="funcdef"; e["return"]=tref(dt.getReturnType())
        e["params"]=[{"name":a.getName(),"type":tref(a.getDataType())} for a in dt.getArguments()]
    return e

def is_placeholder(d):           # Ghidra's empty auto-struct for a class that has no defined layout
    return isinstance(d,Structure) and d.getNumDefinedComponents()==0 and d.getLength()<=1

def export_types():
    TYPES=os.path.join(BASE,"types")
    # Seeds: this program's /FOM + /RakNet types. Then capture the full dependency CLOSURE so
    # referenced externals (the user's /std containers, the /stdint.h + /crtdefs typedefs that a
    # fresh import doesn't pull) ship too — otherwise the build can't resolve them.
    seeds=[dt for dt in pdtm.getAllDataTypes()
           if (str(dt.getCategoryPath().getPath()).startswith("/FOM") or str(dt.getCategoryPath().getPath()).startswith("/RakNet"))
           and not isinstance(dt,(Pointer,Array))]
    captured={}; seen=set()
    def visit(d):
        if d is None: return
        if isinstance(d,(Pointer,Array)): visit(d.getDataType()); return
        pth=str(d.getPathName())
        if pth in seen: return
        seen.add(pth)
        if isinstance(d,BuiltInDataType): return         # builtins always present on a fresh import
        if not isinstance(d,(Composite,Enum,TypeDef,FunctionDefinition)): return   # only reconstructible kinds (skip /undefined etc.)
        captured[pth]=d
        if isinstance(d,Composite):
            for c in d.getComponents(): visit(c.getDataType())
        elif isinstance(d,TypeDef): visit(d.getDataType())
        elif isinstance(d,FunctionDefinition):
            visit(d.getReturnType())
            for a in d.getArguments(): visit(a.getDataType())
    for s in seeds: visit(s)
    buckets=set(bucket_of(d) for d in captured.values())
    # Local bucket rewritten fresh; shared buckets (SharedLib/RakNet/stdint.h/...) ACCUMULATE across
    # binaries (a deleted shared type lingers until you remove that bucket dir and re-export all).
    ld=os.path.join(TYPES,PROG)
    if os.path.isdir(ld): shutil.rmtree(ld)
    n=0
    for pth,d in captured.items():
        b=bucket_of(d); rel=pth.lstrip("/").split("/")
        if rel and rel[0]==b: rel=rel[1:]                # drop redundant leading segment (RakNet/RakNet -> RakNet)
        fp=os.path.join(TYPES,b,*rel)+".json"
        os.makedirs(os.path.dirname(fp),exist_ok=True)
        json.dump(type_entry(d), open(fp,"w",encoding="utf-8",newline=""), indent=1, sort_keys=True); n+=1
    return n,sorted(buckets)

# ============================ SYMBOLS ============================
def in_user_tree(ns):
    fq=str(ns.getName(True)); return fq in USER_TREES or any(fq.startswith(t+"::") for t in USER_TREES)
def nspath(ns): return "" if ns.isGlobal() else str(ns.getName(True))
def dtpath(dt): return str(dt.getPathName()) if dt is not None else None
def var_entry(v):                # canonical: just name + type (storage is analysis-derived, not labeling)
    return {"name":str(v.getName()),"type":dtpath(v.getDataType())}
def dump_symbols():
    out={"program":PROG,"imageBase":str(p.getImageBase()),"namespaces":[],"functions":[],"data":[],"comments":[]}
    fns=[]; datas=[]
    ns=[s for s in st.getDefinedSymbols() if s.getSymbolType() in (SymbolType.CLASS,SymbolType.NAMESPACE)
        and (s.getSource()==US or in_user_tree(s.getParentNamespace()) or in_user_tree(s))]
    for s in sorted(ns,key=lambda x:str(x.getName(True)).count("::")):
        out["namespaces"].append({"path":str(s.getName(True)),"name":str(s.getName()),"parent":nspath(s.getParentNamespace()),
            "kind":"class" if s.getSymbolType()==SymbolType.CLASS else "namespace"})
    for f in fm.getFunctions(True):
        if not (f.getSymbol().getSource()==US or f.getSignatureSource()==US or in_user_tree(f.getParentNamespace())): continue
        tf=f.getThunkedFunction(False)
        e={"addr":str(f.getEntryPoint()),"name":str(f.getName()),"namespace":nspath(f.getParentNamespace()),
           "callingConvention":str(f.getCallingConventionName()),"returnType":dtpath(f.getReturnType()),"params":[]}
        if f.isThunk() and tf: e["thunk"]=str(tf.getEntryPoint())   # only emitted for actual thunks
        for prm in f.getParameters(): e["params"].append(var_entry(prm))
        out["functions"].append(e); fns.append(f)
    for s in st.getDefinedSymbols():
        if s.getSymbolType()==SymbolType.LABEL and not s.isExternal() and not s.isDynamic() and s.getSource()==US:
            d=listing.getDataAt(s.getAddress()); datas.append(s)
            out["data"].append({"addr":str(s.getAddress()),"name":str(s.getName()),"namespace":nspath(s.getParentNamespace()),
                "type":(dtpath(d.getDataType()) if (d is not None and d.isDefined()) else None)})
    aset=AddressSet()
    for f in fns: aset.add(f.getBody())
    for s in datas: aset.addRange(s.getAddress(),s.getAddress())
    def is_auto(c): return c.startswith("Library Function") or c.startswith("WARNING:") or c.startswith("Thunked")
    for nm,ct in (("plate",CodeUnit.PLATE_COMMENT),("pre",CodeUnit.PRE_COMMENT),("eol",CodeUnit.EOL_COMMENT),
                  ("post",CodeUnit.POST_COMMENT),("repeat",CodeUnit.REPEATABLE_COMMENT)):
        for a in listing.getCommentAddressIterator(ct,aset,True):
            c=listing.getComment(ct,a)
            if c and not is_auto(str(c)): out["comments"].append({"addr":str(a),"kind":nm,"text":str(c)})
    return out

def shard_symbols(src):
    OUT=os.path.join(BASE,"symbols",PROG)
    if os.path.isdir(OUT): shutil.rmtree(OUT)
    os.makedirs(OUT,exist_ok=True)
    def ns_to_path(ns): return ns.replace("::","/")
    def type_leaf(t):
        if not t: return None
        s=re.sub(r"(\s*\*\s*\d*|\[\d+\])+$","",str(t)).strip()
        return re.sub(r"[^A-Za-z0-9_.\-]","_", s.rsplit("/",1)[-1] or "misc")
    nsfiles={}; glob_fns=[]; glob_data=[]; data_by_type={}
    for f in src["functions"]:
        ns=f.get("namespace") or ""
        if ns: nsfiles.setdefault(ns_to_path(ns),{"functions":[],"data":[]})["functions"].append(f)
        else: glob_fns.append(f)
    for d in src["data"]:
        ns=d.get("namespace") or ""
        if ns: nsfiles.setdefault(ns_to_path(ns),{"functions":[],"data":[]})["data"].append(d)
        elif d.get("type"): data_by_type.setdefault(type_leaf(d["type"]),[]).append(d)
        else: glob_data.append(d)
    def w(rel,obj):
        fp=os.path.join(OUT,rel); os.makedirs(os.path.dirname(fp),exist_ok=True)
        json.dump(obj, open(fp,"w",encoding="utf-8",newline=""), indent=1, sort_keys=True)
    w("_meta.json", {k:src[k] for k in ("program","imageBase") if k in src})
    w("namespaces.json", src["namespaces"])
    used={}; clashes=[]
    for rel,obj in sorted(nsfiles.items()):
        cand=rel; i=1
        while (cand+".json").lower() in used:
            cand="%s__%d"%(rel,i); i+=1
        if cand!=rel: clashes.append((rel, used[(rel+".json").lower()]))
        used[(cand+".json").lower()]=rel; w(cand+".json", obj)
    if glob_fns or glob_data: w("_global.json", {"functions":glob_fns,"data":glob_data})
    for tname,lst in data_by_type.items(): w(os.path.join("_data",tname+".json"), lst)
    if src["comments"]: w("_comments.json", src["comments"])
    return len(nsfiles), clashes

# ============================ RUN ============================
ntypes,buckets=export_types()
sym=dump_symbols()
nns,clashes=shard_symbols(sym)
print("EXPORT %s"%PROG)
print("  types:   %d files -> buckets %s"%(ntypes,buckets))
print("  symbols: %d namespace files, %d functions, %d data, %d comments"%(nns,len(sym["functions"]),len(sym["data"]),len(sym["comments"])))
if clashes:
    print("  !! CASE-COLLISIONS (likely namespace typos) — disambiguated, please verify:")
    for a,b in clashes: print("       %s  <->  %s"%(a,b))
