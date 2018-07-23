package interpreter

import scala.collection.BitSet

object Interpreter2 {

  def execute(model: Model, trace: Seq[Command], verbose: Boolean = true): State = {
    var s = State(
      R = model.R,
      energy = 0,
      harmonicsHigh = false,
      matrix = model,
      bots = IndexedSeq(Nanobot(1, Pos(0, 0, 0), 2 to 40)),
      trace = trace.toIndexedSeq
    )
    var i = 0
    while (!halted(s)) {
      if (verbose)
        sys.process.stderr.println(i, s.copy(trace = s.trace.headOption.toIndexedSeq, matrix = s.matrix.copy(bitset = BitSet())))
      s = move(s)
      i += 1
    }
    if (verbose)
      sys.process.stderr.println(i, s.energy)
    s
  }

  def halted(state: State): Boolean = state.trace.isEmpty

  def move(state: State): State = {
    val n = state.bots.size
    val R = state.matrix.R
    val (taken, dropped) = state.trace.splitAt(n)
    val commands = (state.bots.zipWithIndex, taken).zipped.toSeq
    val stateChanges = Seq.newBuilder[StateChange]
    stateChanges += StateChange(energy = state.bots.size * 20 + R * R * R * (if (state.harmonicsHigh) 30 else 3))
    val fusionPs = Seq.newBuilder[(Nanobot, Int, ND)]
    val fusionSs = Seq.newBuilder[(Nanobot, Int, ND)]
    val gfills = Seq.newBuilder[Region]
    val gvoids = Seq.newBuilder[Region]
    commands.foreach {
      case ((bot, idx), command: SingletonCommand) => stateChanges += singletonCommand(idx, bot, command, state)
      case ((bot, idx), Command.FusionP(nd)) => fusionPs += ((bot, idx, nd))
      case ((bot, idx), Command.FusionS(nd)) => fusionSs += ((bot, idx, nd))
      case ((bot, idx), Command.GFill(nd, fd)) => gfills += Region.from(bot.pos, nd, fd)
      case ((bot, idx), Command.GVoid(nd, fd)) => gvoids += Region.from(bot.pos, nd, fd)
    }
    stateChanges ++= fusion(fusionPs.result(), fusionSs.result(), state)
    stateChanges ++= gfill(gfills.result(), state)
    stateChanges ++= gvoid(gvoids.result(), state)

    val changes = stateChanges.result()
    var bots = state.bots
    changes.flatMap(_.bot).foreach {
      case (idx, bot) => bots = bots.updated(idx, bot)
    }
    val removes = changes.flatMap(_.botRemove)
    bots = bots.filterNot(bot => removes.contains(bot.bid))
    state.copy(
      energy = state.energy + changes.map(_.energy).sum,
      harmonicsHigh = state.harmonicsHigh ^ (changes.count(_.harmonics) & 1) > 0,
      bots = (bots ++ changes.flatMap(_.botAdd)).sorted,
      matrix = state.matrix.add(changes.flatMap(_.fill)).del(changes.flatMap(_.void)),
      trace = dropped
    )
  }

  private def fusion(ps: Seq[(Nanobot, Int, ND)], ss: Seq[(Nanobot, Int, ND)], state: State): Seq[StateChange] = {
    require(ps.size == ss.size)
    val changes = for {
      (botP, idxP, ndP) <- ps
      (botS, idxS, ndS) <- ss
      if botP.pos.move(ndP.dx, ndP.dy, ndP.dz) == botS.pos && botS.pos.move(ndS.dx, ndS.dy, ndS.dz) == botP.pos
    } yield
      StateChange(
        energy = -24,
        bot = Some(idxP -> botP.copy(seeds = Seq(botP.seeds, Seq(botS.bid), botS.seeds).flatten.sorted)),
        botRemove = Some(botS.bid)
      )
    require(changes.size == ps.size)
    changes
  }

  case class Region(min: Pos, max: Pos) {
    def dimension: Int = (if (max.x - min.x != 0) 1 else 0) + (if (max.y - min.y != 0) 1 else 0) + (if (max.z - min.z != 0) 1 else 0)

    def dimpow: Int = Seq.iterate(2, dimension)(_ * 2).last
  }

  case object Region {
    def from(pos: Pos, nd: ND, fd: FD): Region = {
      val r1 = pos.move(nd.dx, nd.dy, nd.dz)
      val r2 = r1.move(fd.dx, fd.dy, fd.dz)
      Region(
        Pos(Math.min(r1.x, r2.x), Math.min(r1.y, r2.y), Math.min(r1.z, r2.z)),
        Pos(Math.max(r1.x, r2.x), Math.max(r1.y, r2.y), Math.max(r1.z, r2.z))
      )
    }
  }

  private def gfill(fills: Seq[Region], state: State): Seq[StateChange] =
    fills.groupBy(identity).flatMap({
      case (region, seq) =>
        require(seq.size == region.dimpow)
        for {
          x <- region.min.x to region.max.x
          y <- region.min.y to region.max.y
          z <- region.min.z to region.max.z
        } yield {
          val filled = Pos(x, y, z)
          StateChange(
            energy = if (state.matrix.get(filled)) 6 else 12,
            fill = Some(filled)
          )
        }
    }).toSeq

  private def gvoid(voids: Seq[Region], state: State): Seq[StateChange] =
    voids.groupBy(identity).flatMap({
      case (region, seq) =>
        require(seq.size == region.dimpow)
        for {
          x <- region.min.x to region.max.x
          y <- region.min.y to region.max.y
          z <- region.min.z to region.max.z
        } yield {
          val voided = Pos(x, y, z)
          StateChange(
            energy = if (state.matrix.get(voided)) -12 else 3,
            fill = Some(voided)
          )
        }
    }).toSeq

  case class StateChange(
                          energy: Int = 0,
                          harmonics: Boolean = false,
                          bot: Option[(Int, Nanobot)] = None,
                          botAdd: Option[Nanobot] = None,
                          botRemove: Option[Int] = None,
                          fill: Option[Pos] = None,
                          void: Option[Pos] = None
                        )

  private def singletonCommand(idx: Int, bot: Nanobot, command: SingletonCommand, state: State): StateChange =
    command match {
      case Command.Halt =>
        require(state.bots.size == 1)
        require(state.bots.forall(s => s.pos == Pos(x = 0, y = 0, z = 0)))
        require(!state.harmonicsHigh)
        StateChange(botRemove = Some(bot.bid))
      case Command.Wait =>
        StateChange()
      case Command.Flip =>
        StateChange(harmonics = true)
      case Command.SMove(lld) =>
        StateChange(
          energy = 2 * lld.mlen,
          bot = Some(idx -> bot.copy(pos = bot.pos.move(lld.dx, lld.dy, lld.dz)))
        )
      case Command.LMove(sld1, sld2) =>
        StateChange(
          energy = 2 * (sld1.mlen + sld2.mlen + 2),
          bot = Some(idx -> bot.copy(pos = bot.pos.move(sld1.dx + sld2.dx, sld1.dy + sld2.dy, sld1.dz + sld2.dz)))
        )
      case Command.Fission(nd: ND, m: Int) =>
        require(bot.seeds.size >= m + 1)
        StateChange(
          energy = 24,
          bot = Some(idx -> bot.copy(seeds = bot.seeds.drop(m + 1))),
          botAdd = Some(Nanobot(bot.seeds.head, bot.pos.move(nd.dx, nd.dy, nd.dz), bot.seeds.tail.take(m)))
        )
      case Command.Fill(nd: ND) =>
        val filled = bot.pos.move(nd.dx, nd.dy, nd.dz)
        StateChange(
          energy = if (state.matrix.get(filled)) 6 else 12,
          fill = Some(filled)
        )
      case Command.Void(nd: ND) =>
        val removed = bot.pos.move(nd.dx, nd.dy, nd.dz)
        StateChange(
          energy = if (state.matrix.get(removed)) -12 else 3,
          fill = Some(removed)
        )
    }

}
