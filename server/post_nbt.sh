#!/bin/bash -xe

ai="AI_NAME"
user="USER_NAME"
type_="LA/FA/FD/FR"
for i in `seq 1 186`; do
    filename=$(printf "%03d" $i)
    curl -X POST http://54.244.193.90:5050/add -F "nbt=@../data/dfltTracesL/LA${filename}.nbt" -F user=${user} -F ai=${ai} -F problem=$i -F type=${type_}
done
