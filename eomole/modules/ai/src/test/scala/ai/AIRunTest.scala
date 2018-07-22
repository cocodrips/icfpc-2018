package ai

import java.nio.file.{Files, Paths}

import interpreter._
import org.scalatest.FunSuite

class AIRunTest extends FunSuite {

  val ais: Seq[AI] = Seq(NearestLowAI)

  for {
    i <- 1 to 186
    ai <- ais
  } {
    test("%sはF構築ができる FA%03d".format(ai.name, i)) {
      val targetL = Model.decode(Files.readAllBytes(Paths.get("../data/problems/LA%03d_tgt.mdl".format(i))))
      val targetF = Model.decode(Files.readAllBytes(Paths.get("../data/problems/FA%03d_tgt.mdl".format(i))))
      val source = Model.empty(targetF.R)
      val trace = ai.solve(source, targetF)
      Files.write(Paths.get("../data/%s/LA%03d.nbt".format(ai.name, i)), TraceEncoder.encode(trace).toArray) // 一緒なので同時に
      Files.write(Paths.get("../data/%s/FA%03d.nbt".format(ai.name, i)), TraceEncoder.encode(trace).toArray)
      val result = Interpreter.execute(source, trace, verbose = false)
      println(i, result.energy)
      require(targetL.bitset == result.matrix.bitset, "Expected ans printed are not the same.")
      require(targetF.bitset == result.matrix.bitset, "Expected ans printed are not the same.")
    }
  }

  for {
    i <- 1 to 186
    ai <- ais
  } {
    test("%sはF破壊ができる FD%03d".format(ai.name, i)) {
      val source = Model.decode(Files.readAllBytes(Paths.get("../data/problems/FD%03d_src.mdl".format(i))))
      val target = Model.empty(source.R)
      val trace = Reverser.reverse(TraceDecoder.decode(Files.readAllBytes(Paths.get("../data/dfltTraces/LA%03d.nbt".format(i)))))
      Files.write(Paths.get("../data/%s/FD%03d.nbt".format(ai.name, i)), TraceEncoder.encode(trace).toArray)
      val result = Interpreter.execute(source, trace, verbose = false)
      println(i, result.energy)
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
      Files.write(Paths.get("../data/%s/FR%03d.nbt".format(ai.name, i)), TraceEncoder.encode(trace).toArray)
      val result = Interpreter.execute(source, trace, verbose = false)
      println(i, result.energy)
      require(target.bitset == result.matrix.bitset, "Expected ans printed are not the same.")
    }
  }
}
