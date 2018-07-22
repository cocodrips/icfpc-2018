#!/bin/bash -xe

g++ -o solve ./tools/ai.cpp
python3 ./tools/make_map.py
python3 ./tools/solve.py
