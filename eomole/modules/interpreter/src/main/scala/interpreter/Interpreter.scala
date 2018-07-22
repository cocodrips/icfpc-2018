package interpreter

import scala.collection.BitSet

object Interpreter {

  def execute(model: Model, trace: Seq[Command]): State = {
    var s = State(
      R = model.R,
      energy = 0,
      harmonicsHigh = false,
      matrix = model,
      bots = IndexedSeq(Nanobot(0, Pos(0, 0, 0), 2 to 20)),
      trace = trace.toIndexedSeq
    )
    var i = 0
    while (!halted(s)) {
      sys.process.stderr.println(i, s.copy(trace = s.trace.headOption.toIndexedSeq, matrix = s.matrix.copy(bitset = BitSet())))
      s = move(s)
      i += 1
    }
    sys.process.stderr.println(i, s.energy)
    s
  }

  def halted(state: State): Boolean = state.trace.isEmpty

  def move(previous: State): State = {
    val n = previous.bots.size
    val commands = (previous.bots.zipWithIndex, previous.trace.take(n)).zipped.toSeq
    val singletons: State => State = commands.collect({
      case ((bot, idx), command: SingletonCommand) => singletonCommand(idx, bot, command)
    }).reduceOption(_ andThen _).getOrElse(identity[State])
    val fusionPs = commands.filter(_._2.isInstanceOf[Command.FusionP])
    val fusionSs = commands.filter(_._2.isInstanceOf[Command.FusionS])
    val gfills = commands.filter(_._2.isInstanceOf[Command.GFill])
    val gvoids = commands.filter(_._2.isInstanceOf[Command.GVoid])
    //    val
    // TODO: fusion, gfills, gvoids
    (turnStart andThen singletons andThen fusion(fusionPs, fusionSs) andThen turnEnd(n)) (previous)
  }

  private val turnStart: State => State = state => state.copy(
    energy = state.energy +
      state.bots.size * 20 +
      state.matrix.R * state.matrix.R * state.matrix.R * (if (state.harmonicsHigh) 30 else 3)
  )

  private def turnEnd(n: Int): State => State = state => state.copy(
    bots = state.bots.filterNot(_ == null).sorted,
    trace = state.trace.drop(n)
  )

  private def fusion(ps: Seq[((Nanobot, Int), Command)], ss: Seq[((Nanobot, Int), Command)]): State => State = {
    require(ps.size == ss.size)
    val pairs = for {
      ((botP, idxP), Command.FusionP(ndP)) <- ps
      ((botS, idxS), Command.FusionS(ndS)) <- ss
      if botP.pos.move(ndP.dx, ndP.dy, ndP.dz) == botS.pos && botS.pos.move(ndS.dx, ndS.dy, ndS.dz) == botP.pos
    } yield {
      State.energy.modify(_ - 24) andThen State.bots.modify(_
        .updated(idxP, botP.copy(seeds = Seq(botP.seeds, Seq(botS.bid), botS.seeds).flatten.sorted))
        .updated(idxS, null)
      )
    }
    require(pairs.size == ps.size)
    pairs.reduceOption(_ andThen _).getOrElse(identity[State])
  }

  // TODO: harmonicsの値などのチェックは
  //  private def botMove(bots: Seq[(Nanobot, Int)], state: State): State = bots match {
  //      case Command.FusionP(nd) =>
  //        val sidx = state.trace.indexWhere(_.isInstanceOf[Command.FusionS])
  //        val (sbot, _) = bots(sidx)
  //        require(bot.pos.move(nd.dx, nd.dy, nd.dz) == sbot.pos)
  //        // TODO: reverted require
  //        botMove(bots.tail, state.copy(
  //          energy = state.energy - 24,
  //          bots = state.bots
  //            .updated(idx, bot.copy(seeds = Seq(bot.seeds, Seq(sbot.bid), sbot.seeds).flatten.sorted))
  //            .updated(idx + sidx, sbot.copy(removed = true)),
  //          trace = state.trace.updated(sidx, Command.Wait).tail
  //        ))
  //      case Command.FusionS(nd) =>
  //        val pidx = state.trace.indexWhere(_.isInstanceOf[Command.FusionP])
  //        val (pbot, _) = bots(pidx)
  //        require(bot.pos.move(nd.dx, nd.dy, nd.dz) == pbot.pos)
  //        // TODO: reverted require
  //        botMove(bots.tail, state.copy(
  //          energy = state.energy - 24,
  //          bots = state.bots
  //            .updated(idx + pidx, pbot.copy(seeds = Seq(pbot.seeds, Seq(bot.bid), bot.seeds).flatten.sorted))
  //            .updated(idx, bot.copy(removed = true)),
  //          trace = state.trace.updated(pidx, Command.Wait).tail
  //        ))
  //    }
  //
  //  }

  private def requireState(run: State => Unit): State => State = (state: State) => {
    run(state)
    state
  }

  private def singletonCommand(idx: Int, bot: Nanobot, command: SingletonCommand): State => State =
    command match {
      case Command.Halt =>
        requireState(state => {
          require(state.bots.size == 1)
          require(state.bots.forall(s => s.pos == Pos(x = 0, y = 0, z = 0)))
          require(!state.harmonicsHigh)
        }) andThen State.bots.set(IndexedSeq.empty)
      case Command.Wait =>
        identity[State]
      case Command.Flip =>
        State.harmonicsHigh.modify(!_)
      case Command.SMove(lld) =>
        State.energy.modify(_ + 2 * lld.mlen) andThen
          State.bots.modify(_.updated(idx, bot.copy(pos = bot.pos.move(lld.dx, lld.dy, lld.dz))))
      case Command.LMove(sld1, sld2) =>
        State.energy.modify(_ + 2 * (sld1.mlen + sld2.mlen + 2)) andThen
          State.bots.modify(_.updated(idx, bot.copy(
            pos = bot.pos.move(sld1.dx + sld2.dx, sld1.dy + sld2.dy, sld1.dz + sld2.dz)
          )))
      case Command.Fission(nd: ND, m: Int) =>
        requireState(state => require(bot.seeds.size >= m + 1)) andThen
          State.energy.modify(_ + 24) andThen
          State.bots.modify(_.updated(idx, bot.copy(seeds = bot.seeds.drop(m + 1))) :+
            Nanobot(bot.seeds.head, bot.pos.move(nd.dx, nd.dy, nd.dz), bot.seeds.tail.take(m))
          )
      case Command.Fill(nd: ND) =>
        state => {
          val filled = bot.pos.move(nd.dx, nd.dy, nd.dz)
          state.copy(
            energy = state.energy + (if (state.matrix.get(filled)) 6 else 12),
            matrix = state.matrix.add(filled)
          )
        }
      case Command.Void(nd: ND) =>
        state => {
          val removed = bot.pos.move(nd.dx, nd.dy, nd.dz)
          state.copy(
            energy = state.energy + (if (state.matrix.get(removed)) -12 else 3),
            matrix = state.matrix.del(removed)
          )
        }
    }

}
