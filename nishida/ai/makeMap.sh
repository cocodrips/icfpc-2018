for i in `seq 1 3`; do
  filename=$(printf "%03d" $i)
  echo "Map ${filename} is processing"
  python3 tools/disassem_mdl.py data/FD${filename}_src.mdl > map/FD${filename}_src.mdl
done
