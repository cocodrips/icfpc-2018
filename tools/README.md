## assem.py

こんな感じのテキストを用意する（仮に `sample.ntt` とする）。

```
# NOTE: This is an invalid trace.
Halt
Wait
Flip
SMove <12,0,0>
SMove <0,0,-4>
LMove <3,0,0> <0,-5,0>
LMove <0,-2,0> <0,0,2>
FusionP <-1,1,0>
FusionS <1,-1,0>
Fission <0,0,1> 5
Fill <0,-1,0>
```

このコマンドを実行すると binary trace のファイルが手に入る。

```
$ python3 assem.py < sample.ntt > sample.nbt
$ hexdump -C sample.nbt
00000000  ff fe fd 14 1b 34 0b 9c  08 ec 73 3f 9e 75 05 53  |.....4....s?.u.S|
00000010
```


## disassem.py

`assem.py` の反対。デフォルトトレースの解析するのに使える？

```
$ python3 disassem.py < sample.nbt
Halt
Wait
...
Fill <0,-1,0>
```
