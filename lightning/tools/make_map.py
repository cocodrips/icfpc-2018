import os
import sys
from pathlib import Path

src = './data/'
dest = './map/'
spath = Path(src)
dpath = Path(dest)

cc = 0
for name in list(spath.glob("*")):
    #if cc > 2:
    #    break
    print(name.name+" is processing")
    os.system('python3 ./tools/disassem_mdl.py ' + src + name.name + ' > ' + dest + name.name)
    cc = cc + 1
