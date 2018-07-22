import sys

def read_byte(fp):
    x = fp.read(1)
    if not x:
        return None
    return x[0]

def disassemble(fp):
    r = read_byte(fp)
    r = int(r)
    l = (r*r*r)/8
    if (r*r*r) % 8 > 0:
        l = l + 1
    print(int(r));
    for i in range(int(l)):
        tmp = read_byte(fp)
        for j in range(8):
            if tmp & (1<<(j)):
                sys.stdout.write("1")
            else:
                sys.stdout.write("0")

def main(argv):
    if len(argv) >= 2:
        with open(argv[1], mode='rb') as fp:
            disassemble(fp)
    else:
        disassemble(sys.stdin.buffer)


if __name__ == '__main__':
    main(sys.argv)
