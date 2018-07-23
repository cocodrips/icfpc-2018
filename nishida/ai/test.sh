#!/bin/bash -xe

g++ -O2 ai/unionfind_fission_weight.cpp
#g++ -O2 ai/unionfind_fission.cpp

for i in `seq 186 186`; do
  filename=$(printf "%03d" $i)
  echo "Target ${filename} is processing"
  ./a.out < map/FA${filename}_tgt.mdl > tmp/test${filename}.nbt
  #python3 tools/assem.py tmp/test${filename}.nbt > submission/FA${filename}.nbt
done
