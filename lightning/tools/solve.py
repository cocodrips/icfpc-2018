import os
import sys
from pathlib import Path

src = './map/'
dest = './tmp/'
final = './submission/'
spath = Path(src)
dpath = Path(dest)

cc = 1
for name in list(spath.glob("*")):
    #if cc > 2:
    #    break
    print(name.name+" is processing")
    os.system('./solve < ' + src + name.name + ' > ' + dest + 'LA' + str(cc).zfill(3) + '.nbt')
    os.system('python3 ./tools/assem.py ' + dest + 'LA' + str(cc).zfill(3) + '.nbt' + ' > ' + final + 'LA' + str(cc).zfill(3) + '.nbt')
    cc = cc + 1
