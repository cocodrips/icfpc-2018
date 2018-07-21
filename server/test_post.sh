#!/bin/bash -xe

for i in `seq 181 186`; do
    filename=$(printf "%03d" $i)
    curl -X POST http://54.244.193.90:5050/add -F "nbt=@../data/dfltTracesL/LA$filename.nbt" -F user=default -F ai=ai -F problem=$i
done
