package ai

import java.nio.file.{Files, Paths}

import interpreter._
import org.scalatest.FunSuite

class AIRunRCombineTest extends FunSuite {

  for {
    i <- 1 to 100
  } {
    test("F再構築が合成できる FR%03d".format(i)) {
      if (!Files.exists(Paths.get("../data/combine")))
        Files.createDirectory(Paths.get("../data/combine"))
      val source = Model.decode(Files.readAllBytes(Paths.get("../data/problems/FR%03d_src.mdl".format(i))))
      val target = Model.decode(Files.readAllBytes(Paths.get("../data/problems/FR%03d_tgt.mdl".format(i))))
      val R = source.R
      val traceD = TraceDecoder.decode(Files.readAllBytes(Paths.get("../data/yz-FR/FR%03d_src.mbt".format(i))))
      val traceAs = Seq(
        TraceDecoder.decode(Files.readAllBytes(Paths.get("../data/FR_tgt_fission/FR%03d_tgt.nbt".format(i)))),
        TraceDecoder.decode(Files.readAllBytes(Paths.get("../data/FR_tgt_weight/FR%03d_tgt.nbt".format(i)))),
//        TraceDecoder.decode(Files.readAllBytes(Paths.get("../data/NearestEfficientAI/FR%03dA.nbt".format(i))))
      )
      val traceEnergies = traceAs.map(trace => (trace, Interpreter.execute(Model.empty(R), trace, verbose = false).energy))
      println(traceEnergies.map(_._2))
      val traceA = traceEnergies.minBy(_._2)._1
      val trace = traceD.init ++ traceA
      Files.write(Paths.get("../data/combine/FR%03d.nbt".format(i)), TraceEncoder.encode(trace).toArray)
    }
  }
}
