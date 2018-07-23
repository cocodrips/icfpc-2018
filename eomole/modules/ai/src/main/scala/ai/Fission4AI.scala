package ai

import interpreter._

object Fission4AI extends AI {

  // Seq((0,0,0),(1,0,0),(1,0,1),(0,0,1))
  val fissions = Seq(
    Command.Fission(ND(1, 0, 0), 19),
    Command.Fission(ND(0, 0, 1), 9),
    Command.Fission(ND(0, 0, 1), 9)
  )

  // Seq((0,0,0),(1,0,0),(1,0,1),(0,0,1))
  val fusions = Seq(
    Command.FusionP(ND(0, 0, 1)),
    Command.FusionP(ND(0, 0, 1)),
    Command.FusionS(ND(0, 0, -1)),
    Command.FusionS(ND(0, 0, -1)),
    Command.FusionP(ND(1, 0, 0)),
    Command.FusionS(ND(-1, 0, 0))
  )

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

  def serialize(commands: Seq[Seq[Command]]): Seq[Command] = {
    val l = commands.map(_.size).max
    commands.map(_.padTo(l, Command.Wait)).transpose.flatten
  }

  override def solve(sourceModel: Model, targetModel: Model): Seq[Command] = {
    val R = targetModel.R
    if (sourceModel != Model.empty(R))
      throw new UnsupportedOperationException

    val builder = Seq.newBuilder[Command]
    builder ++= fissions
    builder ++= serialize(Seq(
      Seq.empty,
      movePlan(Pos(1, 0, 0), Pos(R - 1, 0, 0)),
      movePlan(Pos(1, 0, 1), Pos(R - 1, 0, R - 1)),
      movePlan(Pos(0, 0, 1), Pos(0, 0, R - 1))
    ))

    builder ++= serialize(Seq(
      Seq.empty,
      movePlan(Pos(R - 1, 0, 0), Pos(1, 0, 0)),
      movePlan(Pos(R - 1, 0, R - 1), Pos(1, 0, 1)),
      movePlan(Pos(0, 0, R - 1), Pos(0, 0, 1))
    ))
    builder ++= fusions
    builder += Command.Halt
    builder.result()
  }

  override def name: String = "FissionAI"
}
