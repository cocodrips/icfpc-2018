#!/bin/bash -xe

#g++ -O2 ai/unionfind_fission_weight.cpp
g++ -O2 ai/unionfind_fission.cpp

for i in `seq 101 115`; do
  filename=$(printf "%03d" $i)
  echo "Target ${filename} is processing"
  ./a.out < map/FR${filename}_tgt.mdl > tmp/testFR${filename}_tgt.nbt
  python3 tools/assem.py tmp/testFR${filename}_tgt.nbt > submission/FR${filename}_tgt.nbt
done
