package interpreter

import java.nio.file.{Files, Paths}

import org.scalatest.FunSuite

import scala.collection.BitSet

class InterpreterTest extends FunSuite {

  val ans1 = 335123860
  val ans2 = 165905180

  test("サンプル1のenergyが計算できる") {
    val trace = TraceDecoder.decode(Files.readAllBytes(Paths.get("../data/dfltTracesL/LA001.nbt")))
    val model = Model.decode(Files.readAllBytes(Paths.get("../data/problemsL/LA001_tgt.mdl")))
    val result = Interpreter.execute(model.copy(bitset = BitSet()), trace.toIndexedSeq)
    assertResult(ans1)(result.energy)
    assertResult(model.bitset)(result.matrix.bitset)
  }

  test("サンプル2のenergyが計算できる") {
    val trace = TraceDecoder.decode(Files.readAllBytes(Paths.get("../data/dfltTracesL/LA002.nbt")))
    val model = Model.decode(Files.readAllBytes(Paths.get("../data/problemsL/LA002_tgt.mdl")))
    val result = Interpreter.execute(model.copy(bitset = BitSet()), trace.toIndexedSeq)
    assertResult(ans2)(result.energy)
    assertResult(model.bitset)(result.matrix.bitset)
  }

  test("FissionとFusionが計算できる") {
    val trace = IndexedSeq(Command.Fission(ND(1, 0, 0), 0), Command.FusionP(ND(1, 0, 0)), Command.FusionS(ND(-1, 0, 0)), Command.Halt)
    val model = Model.decode(Files.readAllBytes(Paths.get("../data/problemsL/LA001_tgt.mdl")))
//    Files.write(Paths.get("../data/FissionAndFusionTrace.nbt"), TraceEncoder.encode(trace).toArray)
    assertResult(expected = 72080)(Interpreter.execute(model.copy(bitset = BitSet()), trace.toIndexedSeq).energy)
  }
}
