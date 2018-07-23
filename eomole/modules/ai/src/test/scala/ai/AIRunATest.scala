package ai

import java.nio.file.{Files, Paths}

import interpreter._
import org.scalatest.FunSuite

class AIRunATest extends FunSuite {

  val ais: Seq[AI] = Seq(NearestEfficientAI, Fission4AI)

  for {
    i <- 1 to 186
    ai <- ais
  } {
    test("%sはF構築ができる FA%03d".format(ai.name, i)) {
      if (!Files.exists(Paths.get("../data/%s".format(ai.name))))
        Files.createDirectory(Paths.get("../data/%s".format(ai.name)))
      val targetL = Model.decode(Files.readAllBytes(Paths.get("../data/problems/LA%03d_tgt.mdl".format(i))))
      val targetF = Model.decode(Files.readAllBytes(Paths.get("../data/problems/FA%03d_tgt.mdl".format(i))))
      val source = Model.empty(targetF.R)
      val trace = ai.solve(source, targetF)
      Files.write(Paths.get("../data/%s/LA%03d.nbt".format(ai.name, i)), TraceEncoder.encode(trace).toArray) // 一緒なので同時に
      Files.write(Paths.get("../data/%s/FA%03d.nbt".format(ai.name, i)), TraceEncoder.encode(trace).toArray)
      val result = Interpreter2.execute(source, trace, verbose = false)
      println(i, result.energy)
      require(targetL.bitset == result.matrix.bitset, "Expected ans printed are not the same.")
      require(targetF.bitset == result.matrix.bitset, "Expected ans printed are not the same.")
    }
  }
}
