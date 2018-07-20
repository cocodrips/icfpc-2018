import sys


def read_byte(fp):
    x = fp.read(1)
    if not x:
        return None
    return x[0]


def decode(fp):
    x1 = read_byte(fp)

    if x1 is None:
        return None

    if x1 == 0b11111111:
        return 'Halt'

    if x1 == 0b11111110:
        return 'Wait'

    if x1 == 0b11111101:
        return 'Flip'

    if (x1 & 0b11001111) == 0b00000100:
        x2 = read_byte(fp)
        assert x2 is not None
        a = (x1 & 0b00110000) >> 4
        i = (x2 & 0b00011111) >> 0
        return 'SMove {}'.format(decode_lld(a, i))

    if (x1 & 0b00001111) == 0b00001100:
        x2 = read_byte(fp)
        assert x2 is not None
        a1 = (x1 & 0b00110000) >> 4
        i1 = (x2 & 0b00001111) >> 0
        a2 = (x1 & 0b11000000) >> 6
        i2 = (x2 & 0b11110000) >> 4
        return 'LMove {} {}'.format(decode_sld(a1, i1), decode_sld(a2, i2))

    if (x1 & 0b00000111) == 0b00000111:
        d = (x1 & 0b11111000) >> 3
        return 'FusionP {}'.format(decode_nd(d))
        
    if (x1 & 0b00000111) == 0b00000110:
        d = (x1 & 0b11111000) >> 3
        return 'FusionS {}'.format(decode_nd(d))

    if (x1 & 0b00000111) == 0b00000101:
        x2 = read_byte(fp)
        assert x2 is not None
        d = (x1 & 0b11111000) >> 3
        m = (x2 & 0b11111111) >> 0
        return 'Fission {} {}'.format(decode_nd(d), m)

    if (x1 & 0b00000111) == 0b00000011:
        d = (x1 & 0b11111000) >> 3
        return 'Fill {}'.format(decode_nd(d))


def decode_lld(a, i):
    return decode_ld(a, i, 15)


def decode_sld(a, i):
    return decode_ld(a, i, 5)


def decode_ld(a, i, offset):
    if a == 1:
        return '<{},{},{}>'.format(i - offset, 0, 0)
    if a == 2:
        return '<{},{},{}>'.format(0, i - offset, 0)
    if a == 3:
        return '<{},{},{}>'.format(0, 0, i - offset)
    assert False


def decode_nd(nd):
    assert (0 <= nd <= 26)
    dx = (nd // 9) % 3 - 1
    dy = (nd // 3) % 3 - 1
    dz = (nd // 1) % 3 - 1
    return '<{},{},{}>'.format(dx, dy, dz)


def disassemble(fp):
    while True:
        command = decode(fp)
        if not command:
            break
        print(command)


def main(argv):
    if len(argv) >= 2:
        with open(argv[1], mode='rb') as fp:
            disassemble(fp)
    else:
        disassemble(sys.stdin.buffer)


if __name__ == '__main__':
    main(sys.argv)
