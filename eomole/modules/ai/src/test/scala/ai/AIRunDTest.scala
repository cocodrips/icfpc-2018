package ai

import java.nio.file.{Files, Paths}

import interpreter._
import org.scalatest.FunSuite

class AIRunDTest extends FunSuite {

  val ais: Seq[AI] = Seq(NearestLowAI)

  for {
    i <- 1 to 186
    ai <- ais
  } {
    test("%sはF破壊ができる FD%03d".format(ai.name, i)) {
      val source = Model.decode(Files.readAllBytes(Paths.get("../data/problems/FD%03d_src.mdl".format(i))))
      val target = Model.empty(source.R)
      val trace = ai.solve(source, target)
      Files.write(Paths.get("../data/%s/FD%03d.nbt".format(ai.name, i)), TraceEncoder.encode(trace).toArray)
      val result = Interpreter2.execute(source, trace, verbose = false)
      println(i, result.energy)
      require(target.bitset == result.matrix.bitset, "Expected ans printed are not the same.")
    }
  }
}
