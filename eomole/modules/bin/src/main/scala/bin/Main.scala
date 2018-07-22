package bin

import java.nio.file.{Files, Paths}

import interpreter.{Interpreter, Model, TraceDecoder}

object Main {
  def main(args: Array[String]): Unit = {
    val src_model = Model.decode(Files.readAllBytes(Paths.get(args(0))))
    val tgt_model = Model.decode(Files.readAllBytes(Paths.get(args(1))))
    val trace = TraceDecoder.decode(Files.readAllBytes(Paths.get(args(2))))
    require(src_model.R == tgt_model.R)
    val result = Interpreter.execute(src_model, trace)
    println(result.energy)
    require(tgt_model.bitset == result.matrix.bitset, "Expected ans printed are not the same.")
  }
}
