for i in `seq 1 115`; do
  filename=$(printf "%03d" $i)
  echo "Map ${filename} is processing"
  python3 tools/disassem_mdl.py data/FR${filename}_src.mdl > map/FR${filename}_src.mdl
  python3 tools/disassem_mdl.py data/FR${filename}_tgt.mdl > map/FR${filename}_tgt.mdl
done
