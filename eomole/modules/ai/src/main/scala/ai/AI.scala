package ai

import interpreter.{Command, Model}

trait AI {
  def solve(sourceModel: Model, targetModel: Model): Seq[Command]

  def name: String
}