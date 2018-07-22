package bin

import java.nio.file.{Files, Paths}

import interpreter.{Interpreter, Model, TraceDecoder}

object Main {
  def main(args: Array[String]): Unit = {
    val model = Model.decode(Files.readAllBytes(Paths.get(args(0))))
    val trace = TraceDecoder.decode(Files.readAllBytes(Paths.get(args(1))))
    val result = Interpreter.execute(model.R, trace)
    println(result.energy)
    require(model.bitset == result.matrix.bitset, "Expected ans printed are not the same.")
  }
}
