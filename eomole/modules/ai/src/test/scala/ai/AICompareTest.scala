package ai

import java.nio.file.{Files, Paths}

import interpreter.{Interpreter, Model, TraceDecoder}
import org.scalatest.FunSuite

class AICompareTest extends FunSuite {

  val baseline: AI = NearestEfficientAI
  val ais: Seq[AI] = Seq(NearestEfficientAI)

  for {
    i <- 1 to 20 //186
    ai <- ais
  } {
    test("%sはF構築が%sと同じかより優れている FA%03d".format(ai.name, baseline.name, i)) {
      val target = Model.decode(Files.readAllBytes(Paths.get("../data/problems/FA%03d_tgt.mdl".format(i))))
      val source = Model.empty(target.R)
      val trace = ai.solve(source, target)
      val result = Interpreter.execute(source, trace, verbose = false)
      val default = TraceDecoder.decode(Files.readAllBytes(Paths.get("../data/%s/LA%03d.nbt".format(baseline.name, i))))
      val defaultResult = Interpreter.execute(source, default, verbose = false)
      println(i, result.energy, defaultResult.energy, 100 * result.energy / defaultResult.energy + "%")
      require(target.bitset == result.matrix.bitset, "Expected ans printed are not the same.")
      require(result.energy <= defaultResult.energy, "Lose!")
    }
  }
}
