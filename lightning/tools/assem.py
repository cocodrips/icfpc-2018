import re
import sys


TUPLE = re.compile(r'<(-?\d+),(-?\d+),(-?\d+)>')
TOKEN = re.compile(r'[A-Za-z]+|\d+|<[^<>]+>')

COMMAND = re.compile(r'(?:{})+'.format(TOKEN.pattern))
IGNORED = re.compile(r'\s+|#.*$')


def parse_tuple(token):
    match = TUPLE.fullmatch(token)
    assert match
    return map(int, match.groups())


def encode(command):
    command = re.sub(IGNORED, '', command)
    if not command:
        return b''
    assert COMMAND.fullmatch(command)
    opcode, *operands = TOKEN.findall(command)

    if opcode.lower() == 'halt':
        assert len(operands) == 0
        return bytes([0b11111111])

    if opcode.lower() == 'wait':
        assert len(operands) == 0
        return bytes([0b11111110])

    if opcode.lower() == 'flip':
        assert len(operands) == 0
        return bytes([0b11111101])

    if opcode.lower() == 'smove':
        assert len(operands) == 1
        a, i = encode_lld(operands[0])
        return bytes([0b00000100 | (a << 4), 0b00000000 | (i << 0)])

    if opcode.lower() == 'lmove':
        assert len(operands) == 2
        a1, i1 = encode_sld(operands[0])
        a2, i2 = encode_sld(operands[1])
        return bytes([0b00001100 | (a1 << 4) | (a2 << 6),
                      0b00000000 | (i1 << 0) | (i2 << 4)])

    if opcode.lower() == 'fusionp':
        assert len(operands) == 1
        d = encode_nd(operands[0])
        return bytes([0b00000111 | (d << 3)])

    if opcode.lower() == 'fusions':
        assert len(operands) == 1
        d = encode_nd(operands[0])
        return bytes([0b00000110 | (d << 3)])

    if opcode.lower() == 'fission':
        assert len(operands) == 2
        d = encode_nd(operands[0])
        m = int(operands[1])
        assert (0 <= m <= 255)
        return bytes([0b00000101 | (d << 3), 0b00000000 | (m << 0)])

    if opcode.lower() == 'fill':
        assert len(operands) == 1
        d = encode_nd(operands[0])
        return bytes([0b00000011 | (d << 3)])

    if opcode.lower() == 'void':
        assert len(operands) == 1
        d = encode_nd(operands[0])
        return bytes([0b00000010 | (d << 3)])

    if opcode.lower() == 'gfill':
        assert len(operands) == 2
        nd = encode_nd(operands[0])
        fd = encode_fd(operands[1])
        return bytes([0b00000001 | (nd << 3)] + list(fd))

    if opcode.lower() == 'gvoid':
        assert len(operands) == 2
        nd = encode_nd(operands[0])
        fd = encode_fd(operands[1])
        return bytes([0b00000000 | (nd << 3)] + list(fd))


def encode_lld(lld):
    return encode_ld(lld, 15)


def encode_sld(sld):
    return encode_ld(sld, 5)


def encode_ld(ld, max):
    dx, dy, dz = parse_tuple(ld)
    if (dx != 0) and (dy == 0) and (dz == 0):
        assert (-max <= dx <= +max)
        return (1, dx + max)
    if (dx == 0) and (dy != 0) and (dz == 0):
        assert (-max <= dy <= +max)
        return (2, dy + max)
    if (dx == 0) and (dy == 0) and (dz != 0):
        assert (-max <= dz <= +max)
        return (3, dz + max)
    assert False


def encode_nd(nd):
    dx, dy, dz = parse_tuple(nd)
    assert (-1 <= dx <= +1)
    assert (-1 <= dy <= +1)
    assert (-1 <= dz <= +1)
    return (dx + 1) * 9 + (dy + 1) * 3 + (dz + 1)
    

def encode_fd(fd):
    dx, dy, dz = parse_tuple(fd)
    assert (-30 <= dx <= +30)
    assert (-30 <= dy <= +30)
    assert (-30 <= dz <= +30)
    return (dx + 30, dy + 30, dz + 30)


def assemble(fp):
    for line in fp:
        sys.stdout.buffer.write(encode(line))


def main(argv):
    if len(argv) >= 2:
        with open(argv[1], mode='r') as fp:
            assemble(fp)
    else:
        assemble(sys.stdin)


if __name__ == '__main__':
    main(sys.argv)
