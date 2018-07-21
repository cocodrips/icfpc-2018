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
    assertResult(ans1)(Interpreter.execute(State(
      R = model.R,
      energy = 0,
      harmonicsHigh = false,
      matrix = model.copy(bitset = BitSet()),
      bots = IndexedSeq(Nanobot(1, Pos(0, 0, 0), 2 to 20)),
      trace = trace.toIndexedSeq
    )))
  }

  test("サンプル2のenergyが計算できる") {
    val trace = TraceDecoder.decode(Files.readAllBytes(Paths.get("../data/dfltTracesL/LA002.nbt")))
    val model = Model.decode(Files.readAllBytes(Paths.get("../data/problemsL/LA002_tgt.mdl")))
    assertResult(ans2)(Interpreter.execute(State(
      R = model.R,
      energy = 0,
      harmonicsHigh = false,
      matrix = model.copy(bitset = BitSet()),
      bots = IndexedSeq(Nanobot(0, Pos(0, 0, 0), 2 to 20)),
      trace = trace.toIndexedSeq
    )))
  }

  test("FissionとFusionが計算できる") {
    val trace = IndexedSeq(Command.Fission(ND(1, 0, 0), 0), Command.FusionP(ND(1, 0, 0)), Command.FusionS(ND(-1, 0, 0)), Command.Halt)
    val model = Model.decode(Files.readAllBytes(Paths.get("../data/problemsL/LA001_tgt.mdl")))
//    Files.write(Paths.get("../data/FissionAndFusionTrace.nbt"), TraceEncoder.encode(trace).toArray)
    assertResult(expected = 72080)(Interpreter.execute(State(
      R = model.R,
      energy = 0,
      harmonicsHigh = false,
      matrix = model.copy(bitset = BitSet()),
      bots = IndexedSeq(Nanobot(0, Pos(0, 0, 0), 2 to 20)),
      trace = trace.toIndexedSeq
    )))
  }
}
