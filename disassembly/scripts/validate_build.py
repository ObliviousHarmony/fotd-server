# validate_build.py — re-dump the just-built program IN MEMORY and diff vs the committed shards.
# Read-only; proves a fresh import + build_program reproduces the original. Run as a -postScript AFTER build_program.
import json, os, glob, re
from ghidra.program.model.symbol import SourceType, SymbolType
from ghidra.program.model.data import (Structure, Union, Enum, TypeDef, FunctionDefinition, Pointer, Array)
BASE=os.path.dirname(os.path.dirname(str(getSourceFile().getAbsolutePath())))   # <repo>/ghidra-export
US=SourceType.USER_DEFINED
p=currentProgram; st=p.getSymbolTable(); fm=p.getFunctionManager(); listing=p.getListing(); pdtm=p.getDataTypeManager()
PROG=str(p.getName()); USER_TREES=("FOM","RakNet"); AUTO_DESCS={"Function Signature Data Type"}
def opt(fn):
    try: return fn()
    except Exception: return None
def nz(s):
    s=str(s) if s is not None else None; return s if s else None
def desc_of(dt):
    d=nz(dt.getDescription()); return None if d in AUTO_DESCS else d
def tref(d):
    if d is None: return None
    if isinstance(d,Pointer): return {"ptr":tref(d.getDataType())}
    if isinstance(d,Array): return {"arr":tref(d.getDataType()),"n":d.getNumElements()}
    return str(d.getPathName())
def type_entry(dt):
    e={"path":str(dt.getPathName()),"len":dt.getLength(),"desc":desc_of(dt)}
    if isinstance(dt,(Structure,Union)):
        e["kind"]="struct" if isinstance(dt,Structure) else "union"; e["packed"]=bool(dt.isPackingEnabled())
        e["fields"]=[[c.getOffset(),c.getFieldName(),tref(c.getDataType()),c.getLength(),nz(c.getComment())] for c in dt.getDefinedComponents()]
    elif isinstance(dt,Enum): e["kind"]="enum"; e["entries"]=sorted([[str(n),dt.getValue(n),nz(dt.getComment(n))] for n in dt.getNames()])
    elif isinstance(dt,TypeDef): e["kind"]="typedef"; e["base"]=tref(dt.getDataType())
    elif isinstance(dt,FunctionDefinition):
        e["kind"]="funcdef"; e["return"]=tref(dt.getReturnType()); e["params"]=[[a.getName(),tref(a.getDataType())] for a in dt.getArguments()]
    return e

# ---- TYPES ----
want={}
TYPES_DIR=os.path.join(BASE,"types")
for fp in glob.glob(os.path.join(TYPES_DIR,"**","*.json"),recursive=True):
    bucket=os.path.relpath(fp,TYPES_DIR).replace("\\","/").split("/")[0]
    if bucket!=PROG and re.search(r"\.(dll|lto|exe)$",bucket): continue   # other binary's local types
    e=json.load(open(fp,encoding="utf-8"))
    w={"path":e["path"],"len":e["len"],"desc":e.get("desc"),"kind":e["kind"]}
    if e["kind"] in ("struct","union"):
        w["packed"]=e.get("packed"); w["fields"]=[[f["offset"],f["name"],f["type"],f["len"],f.get("comment")] for f in e["fields"]]
    elif e["kind"]=="enum": w["entries"]=sorted([[x["name"],x["value"],x.get("comment")] for x in e["entries"]])
    elif e["kind"]=="typedef": w["base"]=e["base"]
    elif e["kind"]=="funcdef": w["return"]=e["return"]; w["params"]=[[x["name"],x["type"]] for x in e["params"]]
    want[e["path"]]=w
built={}
for dt in pdtm.getAllDataTypes():                      # any type in want (incl externals) + /FOM /RakNet extras
    if isinstance(dt,(Pointer,Array)): continue
    pth=str(dt.getPathName()); cp=str(dt.getCategoryPath().getPath())
    if pth in want or cp.startswith("/FOM") or cp.startswith("/RakNet"):
        built[pth]=type_entry(dt)
def teq(a,b):
    for k in ("kind","len","desc"):
        if a.get(k)!=b.get(k): return False
    if a["kind"] in ("struct","union"): return a.get("packed")==b.get("packed") and a.get("fields")==b.get("fields")
    if a["kind"]=="enum": return a.get("entries")==b.get("entries")
    if a["kind"]=="typedef": return a.get("base")==b.get("base")
    if a["kind"]=="funcdef": return a.get("return")==b.get("return") and a.get("params")==b.get("params")
    return True
t_missing=[k for k in want if k not in built]
t_extra=[k for k in built if k not in want]
t_bad=[k for k in want if k in built and not teq(want[k],built[k])]
empties=[k for k in t_extra if built[k].get("kind")=="struct" and built[k].get("len")==1 and not built[k].get("fields")]

# ---- SYMBOLS (key fields) ----
def in_user_tree(ns):
    fq=str(ns.getName(True)); return fq in USER_TREES or any(fq.startswith(t+"::") for t in USER_TREES)
def nsp(ns): return "" if ns.isGlobal() else str(ns.getName(True))
def dtp(dt): return str(dt.getPathName()) if dt is not None else None
bfn={}; bdata={}; bns=set()
for s in st.getDefinedSymbols():
    if s.getSymbolType() in (SymbolType.CLASS,SymbolType.NAMESPACE) and (s.getSource()==US or in_user_tree(s.getParentNamespace()) or in_user_tree(s)):
        bns.add((str(s.getName(True)),nsp(s.getParentNamespace()),"class" if s.getSymbolType()==SymbolType.CLASS else "namespace"))
for f in fm.getFunctions(True):
    if not (f.getSymbol().getSource()==US or f.getSignatureSource()==US or in_user_tree(f.getParentNamespace())): continue
    bfn[str(f.getEntryPoint())]={"name":str(f.getName()),"ns":nsp(f.getParentNamespace()),"ret":dtp(f.getReturnType()),
        "conv":str(f.getCallingConventionName()),"params":[[str(x.getName()),dtp(x.getDataType())] for x in f.getParameters()]}
for s in st.getDefinedSymbols():
    if s.getSymbolType()==SymbolType.LABEL and not s.isExternal() and not s.isDynamic() and s.getSource()==US:
        d=listing.getDataAt(s.getAddress())
        bdata[str(s.getAddress())]={"name":str(s.getName()),"ns":nsp(s.getParentNamespace()),"type":(dtp(d.getDataType()) if (d and d.isDefined()) else None)}
# load shards
SYM=os.path.join(BASE,"symbols",PROG); wfn={}; wdata={}; wns=set()
for e in json.load(open(os.path.join(SYM,"namespaces.json"),encoding="utf-8")): wns.add((e["path"],e["parent"],e["kind"]))
for fp in glob.glob(os.path.join(SYM,"**","*.json"),recursive=True):
    rel=os.path.relpath(fp,SYM).replace("\\","/")
    if rel in ("_meta.json","namespaces.json","_comments.json","_bookmarks.json"): continue
    obj=json.load(open(fp,encoding="utf-8"))
    flist=obj if rel.startswith("_data/") else (obj.get("functions",[]) if isinstance(obj,dict) else [])
    dlist=obj if rel.startswith("_data/") else (obj.get("data",[]) if isinstance(obj,dict) else [])
    if not rel.startswith("_data/"):
        for f in obj.get("functions",[]):
            wfn[f["addr"]]={"name":f["name"],"ns":f.get("namespace") or "","ret":f.get("returnType"),"conv":f.get("callingConvention"),
                "params":[[x["name"],x["type"]] for x in f["params"]]}
    for d in dlist:
        wdata[d["addr"]]={"name":d["name"],"ns":d.get("namespace") or "","type":d.get("type")}
fn_bad=[a for a in wfn if a not in bfn or wfn[a]!=bfn[a]]
data_bad=[a for a in wdata if a not in bdata or wdata[a]!=bdata[a]]
ns_bad=wns ^ bns

print("=== BUILD VALIDATION: %s ==="%PROG)
print("TYPES  want=%d built=%d  missing=%d  bad=%d  extra=%d (of which %d empty RTTI stubs)"%(
    len(want),len(built),len(t_missing),len(t_bad),len(t_extra),len(empties)))
for k in (t_missing+t_bad)[:8]: print("    TYPE issue:",k)
for k in [x for x in t_extra if x not in empties][:8]: print("    TYPE extra(non-stub):",k)
print("FUNCS  want=%d  mismatched=%d"%(len(wfn),len(fn_bad)))
for a in fn_bad[:8]: print("    fn @%s want=%s built=%s"%(a,wfn[a],bfn.get(a)))
print("DATA   want=%d  mismatched=%d"%(len(wdata),len(data_bad)))
for a in data_bad[:8]: print("    data @%s want=%s built=%s"%(a,wdata[a],bdata.get(a)))
print("NAMESPACES symmetric-diff=%d"%len(ns_bad))
for x in list(ns_bad)[:8]: print("    ns:",x)
# linkage: which archive are the shipped types sourced from? (shared -> SharedLib.gdt = linked)
from collections import Counter
srcs=Counter()
for dt in pdtm.getAllDataTypes():
    pth=str(dt.getPathName())
    if pth in want and not isinstance(dt,(Pointer,Array)):
        sa=dt.getSourceArchive(); srcs[(sa.getName() if sa else None, str(sa.getArchiveType()) if sa else None)]+=1
print("LINKAGE (source archive of the %d shipped types):"%len(want))
for k,n in srcs.most_common(): print("    %5d  %s"%(n,k))
ok = (not t_missing and not t_bad and not fn_bad and not data_bad and not ns_bad)
print("\nEND-TO-END: %s (type-extras are fresh-import RTTI stubs, expected)"%("PASS" if ok else "REVIEW"))
