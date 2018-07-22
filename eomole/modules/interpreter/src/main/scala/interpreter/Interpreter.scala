package interpreter

import scala.collection.BitSet

object Interpreter {

  // TODO: 引数にinitial modelを取る
  def execute(R: Int, trace: Seq[Command]): State = {
    var s = State(
      R = R,
      energy = 0,
      harmonicsHigh = false,
      matrix = Model(R, BitSet()),
      bots = IndexedSeq(Nanobot(0, Pos(0, 0, 0), 2 to 20)),
      trace = trace.toIndexedSeq
    )
    var i = 0
    while (!halted(s)) {
//    println(i, s.copy(trace = s.trace.headOption.toIndexedSeq, matrix = s.matrix.copy(bitset = BitSet())))
      s = move(s)
      i += 1
    }
//    println(i, s.energy)
    s
  }

  def halted(state: State): Boolean = state.trace.isEmpty

  def move(state: State): State = {
    // TODO: 順不同なので単体コマンドとグループコマンドを分けて動かして合成する
    val moved = botMove(state.bots.zipWithIndex, state.copy(
      energy = state.energy +
        state.bots.size * 20 +
        state.matrix.R * state.matrix.R * state.matrix.R * (if (state.harmonicsHigh) 30 else 3)
    ))
    moved.copy(bots = moved.bots.filterNot(_.removed).sorted)
  }

  // TODO: harmonicsの値などのチェックは
  private def botMove(bots: Seq[(Nanobot, Int)], state: State): State = bots match {
    case Nil => state
    case (bot, idx) +: tail => state.trace.head match {
      case Command.Halt =>
        require(state.bots.size == 1)
        require(state.bots.forall(s => s.pos == Pos(x = 0, y = 0, z = 0)))
        require(!state.harmonicsHigh)
        state.copy(bots = IndexedSeq.empty, trace = IndexedSeq.empty)
      case Command.Wait =>
        botMove(bots.tail, state.copy(trace = state.trace.tail))
      case Command.Flip =>
        botMove(bots.tail, state.copy(trace = state.trace.tail, harmonicsHigh = !state.harmonicsHigh))
      case Command.SMove(lld) =>
        botMove(bots.tail, state.copy(
          energy = state.energy + 2 * lld.mlen,
          bots = state.bots.updated(idx, bot.copy(pos = bot.pos.move(lld.dx, lld.dy, lld.dz))),
          trace = state.trace.tail
        ))
      case Command.LMove(sld1, sld2) =>
        botMove(bots.tail, state.copy(
          energy = state.energy + 2 * (sld1.mlen + sld2.mlen + 2),
          bots = state.bots.updated(idx, bot.copy(
            pos = bot.pos.move(sld1.dx + sld2.dx, sld1.dy + sld2.dy, sld1.dz + sld2.dz)
          )),
          trace = state.trace.tail
        ))
      case Command.FusionP(nd) =>
        val sidx = state.trace.indexWhere(_.isInstanceOf[Command.FusionS])
        val (sbot, _) = bots(sidx)
        require(bot.pos.move(nd.dx, nd.dy, nd.dz) == sbot.pos)
        // TODO: reverted require
        botMove(bots.tail, state.copy(
          energy = state.energy - 24,
          bots = state.bots
            .updated(idx, bot.copy(seeds = Seq(bot.seeds, Seq(sbot.bid), sbot.seeds).flatten.sorted))
            .updated(idx + sidx, sbot.copy(removed = true)),
          trace = state.trace.updated(sidx, Command.Wait).tail
        ))
      case Command.FusionS(nd) =>
        val pidx = state.trace.indexWhere(_.isInstanceOf[Command.FusionP])
        val (pbot, _) = bots(pidx)
        require(bot.pos.move(nd.dx, nd.dy, nd.dz) == pbot.pos)
        // TODO: reverted require
        botMove(bots.tail, state.copy(
          energy = state.energy - 24,
          bots = state.bots
            .updated(idx + pidx, pbot.copy(seeds = Seq(pbot.seeds, Seq(bot.bid), bot.seeds).flatten.sorted))
            .updated(idx, bot.copy(removed = true)),
          trace = state.trace.updated(pidx, Command.Wait).tail
        ))
      case Command.Fission(nd: ND, m: Int) =>
        require(bot.seeds.size >= m + 1)
        botMove(bots.tail, state.copy(
          energy = state.energy + 24,
          trace = state.trace.tail,
          bots = state.bots.updated(idx, bot.copy(seeds = bot.seeds.drop(m + 1))) :+
            Nanobot(bot.seeds.head, bot.pos.move(nd.dx, nd.dy, nd.dz), bot.seeds.tail.take(m))
        ))
      case Command.Fill(nd: ND) =>
        val filled = bot.pos.move(nd.dx, nd.dy, nd.dz)
        botMove(bots.tail, state.copy(
          energy = state.energy + (if (state.matrix.get(filled)) 6 else 12),
          matrix = state.matrix.add(filled),
          trace = state.trace.tail
        ))
    }

  }

}
