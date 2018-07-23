d="../nbt/FR"
mkdir -p ${d}

user="cocodrips"
ai="ym_km_mix"
type_="FR"

for i in `seq 1 115`; do
	di=$(printf "%03d" $i)
	python disassem_nohalt.py < ../nbt/yz/FR/FR${di}_src.nbt > tmp.ntt
	python disassem_nohalt.py < ../nbt/FR_tgt_fission/FR${di}_tgt.nbt >> tmp.ntt
	echo "Halt" >> tmp.ntt
	python assem.py < tmp.ntt > ${d}/FR${di}.nbt
	echo "${d}/FR${di}.nbt"
    curl -X POST http://54.244.193.90:5050/add -F "nbt=@${d}/FR${di}.nbt" -F user=${user} -F ai=${ai} -F problem=$i -F type=${type_}
done
