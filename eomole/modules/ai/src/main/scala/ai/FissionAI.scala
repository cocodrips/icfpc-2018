package ai

import interpreter.{Command, Model}

object FissionAI extends AI {
  override def solve(sourceModel: Model, targetModel: Model): Seq[Command] = {
    val builder = Seq.newBuilder[Command]


    // ばらまく
    // かためる
    builder += Command.Flip
    builder += Command.Halt
    builder.result()
  }

  override def name: String = "Fission"
}
