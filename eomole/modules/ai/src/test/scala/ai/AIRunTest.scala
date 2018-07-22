package ai

import java.nio.file.{Files, Paths}

import interpreter.{Interpreter, Model, TraceEncoder}
import org.scalatest.FunSuite

class AIRunTest extends FunSuite {

  val ais: Seq[AI] = Seq(NearestAI)

  for {
    i <- 1 to 186
    ai <- ais
  } {
    test("%sはL構築ができる LA%03d".format(ai.name, i)) {
      val target = Model.decode(Files.readAllBytes(Paths.get("../data/problems/LA%03d_tgt.mdl".format(i))))
      val source = Model.empty(target.R)
      val trace = ai.solve(source, target)
      Files.write(Paths.get("../data/eomole/LA%03d.nbt".format(i)), TraceEncoder.encode(trace).toArray)
      val result = Interpreter.execute(source, trace, verbose = false)
      println(result.energy)
      require(target.bitset == result.matrix.bitset, "Expected ans printed are not the same.")
    }
  }

  for {
    i <- 1 to 186
    ai <- ais
  } {
    test("%sはF構築ができる FA%03d".format(ai.name, i)) {
      val target = Model.decode(Files.readAllBytes(Paths.get("../data/problems/FA%03d_tgt.mdl".format(i))))
      val source = Model.empty(target.R)
      val trace = ai.solve(source, target)
      Files.write(Paths.get("../data/eomole/FA%03d.nbt".format(i)), TraceEncoder.encode(trace).toArray)
      val result = Interpreter.execute(source, trace, verbose = false)
      println(result.energy)
      require(target.bitset == result.matrix.bitset, "Expected ans printed are not the same.")
    }
  }

  for {
    i <- 1 to 186
    ai <- ais
  } {
    test("%sはF破壊ができる FD%03d".format(ai.name, i)) {
      val source = Model.decode(Files.readAllBytes(Paths.get("../data/problems/FD%03d_src.mdl".format(i))))
      val target = Model.empty(source.R)
      val trace = ai.solve(source, target)
      Files.write(Paths.get("../data/eomole/FD%03d.nbt".format(i)), TraceEncoder.encode(trace).toArray)
      val result = Interpreter.execute(source, trace, verbose = false)
      println(result.energy)
      require(target.bitset == result.matrix.bitset, "Expected ans printed are not the same.")
    }
  }

  for {
    i <- 1 to 115
    ai <- ais
  } {
    test("%sはF再構築ができる FR%03d".format(ai.name, i)) {
      val target = Model.decode(Files.readAllBytes(Paths.get("../data/problems/FR%03d_tgt.mdl".format(i))))
      val source = Model.decode(Files.readAllBytes(Paths.get("../data/problems/FR%03d_src.mdl".format(i))))
      val trace = ai.solve(source, target)
      Files.write(Paths.get("../data/eomole/FR%03d.nbt".format(i)), TraceEncoder.encode(trace).toArray)
      val result = Interpreter.execute(source, trace, verbose = false)
      println(result.energy)
      require(target.bitset == result.matrix.bitset, "Expected ans printed are not the same.")
    }
  }
}
