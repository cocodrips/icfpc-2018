package ai

import java.nio.file.{Files, Paths}

import interpreter.{Interpreter, Model, TraceDecoder}
import org.scalatest.FunSuite

class AICompareTest extends FunSuite {

  val ais: Seq[AI] = Seq(NearestAI)

  for {
    i <- 1 to 186
    ai <- ais
  } {
    test("%sはL構築がデフォルトより優れている LA%03d".format(ai.name, i)) {
      val target = Model.decode(Files.readAllBytes(Paths.get("../data/problems/LA%03d_tgt.mdl".format(i))))
      val source = Model.empty(target.R)
      val trace = ai.solve(source, target)
      val result = Interpreter.execute(source, trace, verbose = false)
      val default = TraceDecoder.decode(Files.readAllBytes(Paths.get("../data/dfltTraces/LA%03d.nbt".format(i))))
      val defaultResult = Interpreter.execute(source, default, verbose = false)
      println(i, result.energy, defaultResult.energy, 100 * result.energy / defaultResult.energy + "%")
      require(result.energy < defaultResult.energy, "Lose!")
    }
  }

  for {
    i <- 1 to 186
    ai <- ais
  } {
    test("%sはF構築がデフォルトより優れている FA%03d".format(ai.name, i)) {
      val target = Model.decode(Files.readAllBytes(Paths.get("../data/problems/FA%03d_tgt.mdl".format(i))))
      val source = Model.empty(target.R)
      val trace = ai.solve(source, target)
      val result = Interpreter.execute(source, trace, verbose = false)
      val default = TraceDecoder.decode(Files.readAllBytes(Paths.get("../data/dfltTraces/FA%03d.nbt".format(i))))
      val defaultResult = Interpreter.execute(source, default, verbose = false)
      println(i, result.energy, defaultResult.energy, 100 * result.energy / defaultResult.energy + "%")
      require(result.energy < defaultResult.energy, "Lose!")
    }
  }

  for {
    i <- 1 to 186
    ai <- ais
  } {
    test("%sはF破壊がデフォルトより優れている FD%03d".format(ai.name, i)) {
      val source = Model.decode(Files.readAllBytes(Paths.get("../data/problems/FD%03d_src.mdl".format(i))))
      val target = Model.empty(source.R)
      val trace = ai.solve(source, target)
      val result = Interpreter.execute(source, trace, verbose = false)
      val default = TraceDecoder.decode(Files.readAllBytes(Paths.get("../data/dfltTraces/FD%03d.nbt".format(i))))
      val defaultResult = Interpreter.execute(source, default, verbose = false)
      println(i, result.energy, defaultResult.energy, 100 * result.energy / defaultResult.energy + "%")
      require(result.energy < defaultResult.energy, "Lose!")
    }
  }

  for {
    i <- 1 to 115
    ai <- ais
  } {
    test("%sはF再構築がデフォルトより優れている FR%03d".format(ai.name, i)) {
      val target = Model.decode(Files.readAllBytes(Paths.get("../data/problems/FR%03d_tgt.mdl".format(i))))
      val source = Model.decode(Files.readAllBytes(Paths.get("../data/problems/FR%03d_src.mdl".format(i))))
      val trace = ai.solve(source, target)
      val result = Interpreter.execute(source, trace, verbose = false)
      val default = TraceDecoder.decode(Files.readAllBytes(Paths.get("../data/dfltTraces/FR%03d.nbt".format(i))))
      val defaultResult = Interpreter.execute(source, default, verbose = false)
      println(i, result.energy, defaultResult.energy, 100 * result.energy / defaultResult.energy + "%")
      require(result.energy < defaultResult.energy, "Lose!")
    }
  }
}
