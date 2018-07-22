package ai

import interpreter.{Command, Model}

object EmptyAI extends AI {
  override def solve(sourceModel: Model, targetModel: Model): Seq[Command] =
    Seq(Command.Halt)

  override def name: String = "Empty"
}
