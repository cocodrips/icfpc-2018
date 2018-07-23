import sys

class Bot(object):

    def first():
        return Bot(1, [0, 0, 0], list(range(2, 41)))

    def __init__(self, bid, pos, seeds):
        self.bid = bid
        self.pos = pos
        self.seeds = seeds

    def fission(self, dx, dy, dz, m):
        bid = self.seeds[0]
        seeds = self.seeds[1:m+1]
        self.seeds = self.seeds[m+1:]
        return Bot(bid, [self.pos[0] + dx, self.pos[1] + dy, self.pos[2] + dz], seeds)

    def fusion(self, bot):
        self.seeds.append(bot.bid)
        self.seeds.extend(bot.seeds)
        self.seeds.sort()
        bot.seeds = None


class State(object):

    def __init__(self):
        self.index = 0
        self.bots = [Bot.first()]
        self.commands = []
        self.current = []
        self.newBots = []

    def add(self, command):
        now = self.bots[self.index]
        if command is not None:
            self.current.append((now.bid, command))
        self.index += 1
        if self.index == len(self.bots):
            self.bots = [b for b in self.bots if b.seeds is not None]
            self.bots.extend(self.newBots)
            self.bots.sort(key=lambda x: x.bid)
            self.index = 0
            # TODO: Generalize this hack
            self.current.sort(key=lambda x:50 if x[0] == 1 else x[0])
            self.commands.append("")
            self.commands.extend([x[1] + " # bid" + str(x[0]) for x in self.current])
            self.current = []
            self.newBots = []

    def move(self, axis, d):
        self.bots[self.index].pos[axis] += d

    def fusionP(self, dx, dy, dz):
        now = self.bots[self.index]
        for i in range(len(self.bots)):
            bot = self.bots[i]
            if i == self.index:
                continue
            if bot.pos == [now.pos[0] + dx, now.pos[1] + dy, now.pos[2] + dz]:
                m = len(bot.seeds)
                now.fusion(bot)
                return '<{},{},{}>'.format(dx, dy, dz), m
        assert False

    def fusionS(self, dx, dy, dz):
        pass

    def fission(self, dx, dy, dz, m):
        now = self.bots[self.index]
        new = now.fission(dx, dy, dz, m)
        self.newBots.append(new)
        ndp = '<{},{},{}>'.format(-dx, -dy, -dz)
        self.current.append((new.bid, 'FusionS {}'.format(ndp)))
        return '<{},{},{}>'.format(dx, dy, dz)


def read_byte(fp):
    x = fp.read(1)
    if not x:
        return None
    return x[0]


def decode(fp, state):
    x1 = read_byte(fp)

    if x1 is None:
        # EOF
        return False

    if x1 == 0b11111111:
        # Halt
        return True

    if x1 == 0b11111110:
        state.add('Wait')
        return True

    if x1 == 0b11111101:
        state.add('Flip')
        return True

    if (x1 & 0b11001111) == 0b00000100:
        x2 = read_byte(fp)
        assert x2 is not None
        a = (x1 & 0b00110000) >> 4
        i = (x2 & 0b00011111) >> 0
        lld = decode_lld(a, i, state)
        state.add('SMove {}'.format(lld))
        return True

    if (x1 & 0b00001111) == 0b00001100:
        x2 = read_byte(fp)
        assert x2 is not None
        a1 = (x1 & 0b00110000) >> 4
        i1 = (x2 & 0b00001111) >> 0
        a2 = (x1 & 0b11000000) >> 6
        i2 = (x2 & 0b11110000) >> 4
        sld1 = decode_sld(a2, i2, state)
        sld2 = decode_sld(a1, i1, state)
        state.add('LMove {} {}'.format(sld1, sld2))
        return True

    if (x1 & 0b00000111) == 0b00000111:
        d = (x1 & 0b11111000) >> 3
        nd, m = decode_nd(d, state, 'p')
        state.add('Fission {} {}'.format(nd, m))
        return True

    if (x1 & 0b00000111) == 0b00000110:
        d = (x1 & 0b11111000) >> 3
        decode_nd(d, state, 's')
        state.add(None)
        return True

    if (x1 & 0b00000111) == 0b00000101:
        x2 = read_byte(fp)
        assert x2 is not None
        d = (x1 & 0b11111000) >> 3
        m = (x2 & 0b11111111) >> 0
        ndp = decode_nd(d, state, 'f', m)
        state.add('FusionP {}'.format(ndp))
        return True

    if (x1 & 0b00000111) == 0b00000011:
        d = (x1 & 0b11111000) >> 3
        state.add('Void {}'.format(decode_nd(d)))
        return True

    if (x1 & 0b00000111) == 0b00000010:
        d = (x1 & 0b11111000) >> 3
        state.add('Fill {}'.format(decode_nd(d)))
        return True

    if (x1 & 0b00000111) == 0b00000001:
        d = (x1 & 0b11111000) >> 3
        dx = read_byte(fp)
        assert dx is not None
        dy = read_byte(fp)
        assert dy is not None
        dz = read_byte(fp)
        assert dz is not None
        assert False, 'Not reversible'
        # 'GVoid {} {}'.format(decode_nd(d), decode_fd(dx, dy, dz))

    if (x1 & 0b00000111) == 0b00000000:
        d = (x1 & 0b11111000) >> 3
        dx = read_byte(fp)
        assert dx is not None
        dy = read_byte(fp)
        assert dy is not None
        dz = read_byte(fp)
        assert dz is not None
        assert False, 'Not reversible'
        # 'GFill {} {}'.format(decode_nd(d), decode_fd(dx, dy, dz))


def decode_lld(a, i, state):
    return decode_ld(a, i, 15, state)


def decode_sld(a, i, state):
    return decode_ld(a, i, 5, state)


def decode_ld(a, i, offset, state):
    if a == 1:
        state.move(0, i - offset);
        return '<{},{},{}>'.format(-(i - offset), 0, 0)
    if a == 2:
        state.move(1, i - offset);
        return '<{},{},{}>'.format(0, -(i - offset), 0)
    if a == 3:
        state.move(2, i - offset);
        return '<{},{},{}>'.format(0, 0, -(i - offset))
    assert False


def decode_nd(nd, state=None, mode=None, m=None):
    assert (0 <= nd <= 26)
    dx = (nd // 9) % 3 - 1
    dy = (nd // 3) % 3 - 1
    dz = (nd // 1) % 3 - 1
    if state is not None:
        if mode == 'p':
            return state.fusionP(dx, dy, dz)
        elif mode == 's':
            return state.fusionS(dx, dy, dz)
        elif mode == 'f':
            return state.fission(dx, dy, dz, m)
    return '<{},{},{}>'.format(dx, dy, dz)


def decode_fd(dx, dy, dz):
    assert (0 <= dx <= 60)
    assert (0 <= dy <= 60)
    assert (0 <= dz <= 60)
    dx -= 30; dy -= 30; dz -= 30
    return '<{},{},{}>'.format(dx, dy, dz)


def disassemble(fp):
    state = State()
    while decode(fp, state):
        pass
    for cmd in reversed(state.commands):
        print(cmd)
    print('Halt')


def main(argv):
    if len(argv) >= 2:
        with open(argv[1], mode='rb') as fp:
            disassemble(fp)
    else:
        disassemble(sys.stdin.buffer)


if __name__ == '__main__':
    main(sys.argv)
