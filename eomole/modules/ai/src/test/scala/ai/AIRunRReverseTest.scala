package ai

import java.nio.file.{Files, Paths}

import interpreter._
import org.scalatest.FunSuite

class AIRunRReverseTest extends FunSuite {

  val ais: Seq[AI] = Seq(NearestEfficientAI)

  for {
    i <- 1 to 115
    ai <- ais
  } {
    test("%sはF再構築ができる FR%03d".format(ai.name, i)) {
      val source = Model.decode(Files.readAllBytes(Paths.get("../data/problems/FR%03d_src.mdl".format(i))))
      val target = Model.decode(Files.readAllBytes(Paths.get("../data/problems/FR%03d_tgt.mdl".format(i))))
      val R = source.R
      val traceD = Reverser.reverse(ai.solve(Model.empty(R), source))
      val traceA = ai.solve(Model.empty(R), target)
      Files.write(Paths.get("../data/%s/FR%03dD.nbt".format(ai.name, i)), TraceEncoder.encode(traceD).toArray)
      Files.write(Paths.get("../data/%s/FR%03dA.nbt".format(ai.name, i)), TraceEncoder.encode(traceA).toArray)
      val trace = traceD.init ++ traceA
      Files.write(Paths.get("../data/%s/FR%03d.nbt".format(ai.name, i)), TraceEncoder.encode(trace).toArray)
//      val result = Interpreter.execute(source, trace, verbose = false)
//      println(i, result.energy)
//      require(target.bitset == result.matrix.bitset, "Expected ans printed are not the same.")
    }
  }
}
