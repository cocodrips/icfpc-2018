package ai

import java.nio.file.{Files, Paths}

import interpreter._
import org.scalatest.FunSuite

class AIRunRTest extends FunSuite {

  val ais: Seq[AI] = Seq(NearestLowAI)

  for {
    i <- 1 to 115
    ai <- ais
  } {
    test("%sはF再構築ができる FR%03d".format(ai.name, i)) {
      val target = Model.decode(Files.readAllBytes(Paths.get("../data/problems/FR%03d_tgt.mdl".format(i))))
      val source = Model.decode(Files.readAllBytes(Paths.get("../data/problems/FR%03d_src.mdl".format(i))))
      val trace = ai.solve(source, target)
      Files.write(Paths.get("../data/%s/FR%03d.nbt".format(ai.name, i)), TraceEncoder.encode(trace).toArray)
      val result = Interpreter2.execute(source, trace, verbose = false)
      println(i, result.energy)
      require(target.bitset == result.matrix.bitset, "Expected ans printed are not the same.")
    }
  }
}
