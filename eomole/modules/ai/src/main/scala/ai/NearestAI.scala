package ai

import interpreter._

object NearestAI extends AI {
  override def solve(sourceModel: Model, targetModel: Model): Seq[Command] = {
    val builder = Seq.newBuilder[Command]
    val R = sourceModel.R
    builder += Command.Flip
    if (sourceModel != Model.empty(R))
      builder ++= reverse(plan(sourceModel))
    if (targetModel != Model.empty(R))
      builder ++= plan(targetModel)
    builder += Command.Flip
    builder += Command.Halt
    builder.result()
  }

  def plan(target: Model): Seq[Command] = {
    val R = target.R
    var matrix = target
    val builder = Seq.newBuilder[Command]
    var p = Pos(0, 0, 0)
    for (y <- 1 until R) {
      val dy = y - p.y
      p = p.move(0, dy, 0)
      builder += Command.SMove(LLD(Axis.Y, dy))
      var cnt = 0
      for {
        x <- 0 until R
        z <- 0 until R
        if matrix.get(Pos(x, y - 1, z))
      } cnt += 1

      while (cnt > 0) {
        // move
        var bestScore = Int.MaxValue
        var bestPlan: Seq[Command] = null
        var to: Pos = null
        for {
          x <- p.x - 5 to p.x + 5
          z <- p.z - 5 to p.z + 5
          if 0 <= x && x < R && 0 <= z && z < R
          cp = p.copy(x = x, z = z)
        } {
          var fill = 0
          for {
            dx <- -1 to 1
            dz <- -1 to 1
            pp = cp.move(dx, -1, dz)
            if matrix.get(pp) && Math.abs(dx) + Math.abs(dz) < 2
          } fill += 1
          if (fill > 0) {
            val plan = movePlan(p, cp)
            val dist = Math.abs(x - p.x) + Math.abs(z - p.z)
            val score = dist - fill
            if (bestPlan == null || plan.size < bestPlan.size || plan.size == bestPlan.size && score < bestScore) {
              bestPlan = plan
              bestScore = score
              to = cp
            }
          }
        }
        {
          var dist = 5
          while (to == null) {
            dist += 1
            for {
              dx <- 0 to dist
              dz = dist - dx
              x <- Seq(p.x + dx, p.x - dx)
              z <- Seq(p.z + dz, p.z - dz)
              if 0 <= x && x < R && 0 <= z && z < R
              cp = p.copy(x = x, z = z)
            } {
              var fill = 0
              for {
                dx <- -1 to 1
                dz <- -1 to 1
                pp = cp.move(dx, -1, dz)
                if matrix.get(pp) && Math.abs(dx) + Math.abs(dz) < 2
              } fill += 1
              if (fill > 0) {
                val plan = movePlan(p, cp)
                val score = dist - fill
                if (bestPlan == null || plan.size < bestPlan.size || plan.size == bestPlan.size && score < bestScore) {
                  bestPlan = plan
                  bestScore = score
                  to = cp
                }
              }
            }
          }
        }
        builder ++= bestPlan
        p = to

        // fill
        for {
          dx <- -1 to 1
          dz <- -1 to 1
          pp = p.move(dx, -1, dz)
          if matrix.get(pp) && Math.abs(dx) + Math.abs(dz) < 2
        } {
          matrix = matrix.del(pp)
          builder += Command.Fill(ND(dx, -1, dz))
          cnt -= 1
        }
      }

      // validation
      var cnt2 = 0
      for {
        x <- 0 until R
        z <- 0 until R
        if matrix.get(Pos(x, y - 1, z))
      } cnt2 += 1
      require(cnt2 == 0)
    }
    // return
    builder ++= movePlan(p, p.copy(x = 0, z = 0))
    p = p.copy(x = 0, z = 0)
    while (p.y > 0) {
      val d = -Math.min(15, p.y)
      builder += Command.SMove(LLD(Axis.Y, d))
      p = p.move(0, d, 0)
    }
    builder.result()
  }

  def movePlan(from: Pos, to: Pos): Seq[Command] = {
    require(from.y == to.y)
    val builder = Seq.newBuilder[Command]
    var p = from
    while (Math.abs(to.x - p.x) > 5) {
      val d = Math.min(15, Math.max(-15, to.x - p.x))
      builder += Command.SMove(LLD(Axis.X, d))
      p = p.move(d, 0, 0)
    }
    while (Math.abs(to.z - p.z) > 5) {
      val d = Math.min(15, Math.max(-15, to.z - p.z))
      builder += Command.SMove(LLD(Axis.Z, d))
      p = p.move(0, 0, d)
    }
    if (Math.abs(to.x - p.x) > 0 && Math.abs(to.z - p.z) > 0)
      builder += Command.LMove(SLD(Axis.X, to.x - p.x), SLD(Axis.Z, to.z - p.z))
    else if (Math.abs(to.x - p.x) > 0)
      builder += Command.SMove(LLD(Axis.X, to.x - p.x))
    else if (Math.abs(to.z - p.z) > 0)
      builder += Command.SMove(LLD(Axis.Z, to.z - p.z))
    builder.result()
  }

  def reverse(commands: Seq[Command]): Seq[Command] =
    commands.reverse.map {
      case Command.SMove(lld) => Command.SMove(LLD(lld.axis, -lld.d))
      case Command.LMove(sld1, sld2) => Command.LMove(SLD(sld1.axis, -sld1.d), SLD(sld2.axis, -sld2.d))
      case Command.Fill(nd) => Command.Void(nd)
      case Command.Void(nd) => Command.Fill(nd)
      case Command.GFill(nd, fd) => Command.GVoid(nd, fd)
      case Command.GVoid(nd, fd) => Command.GFill(nd, fd)
      case c => c // それ以外もあるが未対応
    }

  override def name: String = "NearestAI"
}
