package interpreter

import java.nio.file.{Files, Paths}

import org.scalatest.{FunSuite, Ignore}

import scala.collection.BitSet

class Interpreter2Test extends FunSuite {

  val ans1 = 335123860
  val ans2 = 165905180

  test("サンプル1のenergyが計算できる") {
    val trace = TraceDecoder.decode(Files.readAllBytes(Paths.get("../data/dfltTracesL/LA001.nbt")))
    val model = Model.decode(Files.readAllBytes(Paths.get("../data/problemsL/LA001_tgt.mdl")))
    val result = Interpreter2.execute(model.copy(bitset = BitSet()), trace.toIndexedSeq)
    assertResult(ans1)(result.energy)
    assertResult(model.bitset)(result.matrix.bitset)
  }

  test("サンプル2のenergyが計算できる") {
    val trace = TraceDecoder.decode(Files.readAllBytes(Paths.get("../data/dfltTracesL/LA002.nbt")))
    val model = Model.decode(Files.readAllBytes(Paths.get("../data/problemsL/LA002_tgt.mdl")))
    val result = Interpreter2.execute(model.copy(bitset = BitSet()), trace.toIndexedSeq)
    assertResult(ans2)(result.energy)
    assertResult(model.bitset)(result.matrix.bitset)
  }

  test("FissionとFusionが計算できる") {
    val trace = IndexedSeq(
      Command.Fission(ND(1, 0, 0), 0),
      Command.FusionP(ND(1, 0, 0)),
      Command.FusionS(ND(-1, 0, 0)),
      Command.Halt
    )
    val model = Model.decode(Files.readAllBytes(Paths.get("../data/problemsL/LA001_tgt.mdl")))
    //    Files.write(Paths.get("../data/FissionAndFusionTrace.nbt"), TraceEncoder.encode(trace).toArray)
    assertResult(expected = 72080)(Interpreter2.execute(model.copy(bitset = BitSet()), trace.toIndexedSeq).energy)
  }

  test("GFillが計算できる") {
    val trace = IndexedSeq(
      Command.Fission(ND(1, 0, 0), 0),
      Command.Fission(ND(0, 1, 0), 0),
      Command.Wait,
      Command.Wait,
      Command.SMove(LLD(Axis.Y, 2)),
      Command.SMove(LLD(Axis.Y, 2)),
      Command.GFill(ND(0, 1, 0), FD(0, 1, 0)),
      Command.Flip,
      Command.GFill(ND(0, -1, 0), FD(0, -1, 0))
    )
    val model = Model.decode(Files.readAllBytes(Paths.get("../data/problemsL/LA001_tgt.mdl")))
    //    Files.write(Paths.get("../data/GFillTrace.nbt"), TraceEncoder.encode(trace).toArray)
    assertResult(expected = 96260)(Interpreter2.execute(model.copy(bitset = BitSet()), trace.toIndexedSeq).energy)
  }

  test("サンプル1のenergyがInterpreterより速い") {
    val trace = TraceDecoder.decode(Files.readAllBytes(Paths.get("../data/dfltTracesL/LA001.nbt")))
    val model = Model.decode(Files.readAllBytes(Paths.get("../data/problemsL/LA001_tgt.mdl")))
    val i1 = time(Interpreter.execute(model.copy(bitset = BitSet()), trace.toIndexedSeq, verbose = false))
    val i2 = time(Interpreter2.execute(model.copy(bitset = BitSet()), trace.toIndexedSeq, verbose = false))
    println(i1, i2)
    assert(i1 > i2)
  }

  test("サンプル2のenergyがInterpreterより速い") {
    val trace = TraceDecoder.decode(Files.readAllBytes(Paths.get("../data/dfltTracesL/LA002.nbt")))
    val model = Model.decode(Files.readAllBytes(Paths.get("../data/problemsL/LA002_tgt.mdl")))
    val i1 = time(Interpreter.execute(model.copy(bitset = BitSet()), trace.toIndexedSeq, verbose = false))
    val i2 = time(Interpreter2.execute(model.copy(bitset = BitSet()), trace.toIndexedSeq, verbose = false))
    println(i1, i2)
    assert(i1 > i2)
  }

  test("FissionとFusionがInterpreterより速い") {
    val trace = IndexedSeq(
      Command.Fission(ND(1, 0, 0), 0),
      Command.FusionP(ND(1, 0, 0)),
      Command.FusionS(ND(-1, 0, 0)),
      Command.Halt
    )
    val model = Model.decode(Files.readAllBytes(Paths.get("../data/problemsL/LA001_tgt.mdl")))
    val i1 = time(Interpreter.execute(model.copy(bitset = BitSet()), trace.toIndexedSeq, verbose = false))
    val i2 = time(Interpreter2.execute(model.copy(bitset = BitSet()), trace.toIndexedSeq, verbose = false))
    println(i1, i2)
    assert(i1 > i2)
  }

  test("GFillがInterpreterより速い") {
    val trace = IndexedSeq(
      Command.Fission(ND(1, 0, 0), 0),
      Command.Fission(ND(0, 1, 0), 0),
      Command.Wait,
      Command.Wait,
      Command.SMove(LLD(Axis.Y, 2)),
      Command.SMove(LLD(Axis.Y, 2)),
      Command.GFill(ND(0, 1, 0), FD(0, 1, 0)),
      Command.Flip,
      Command.GFill(ND(0, -1, 0), FD(0, -1, 0))
    )
    val model = Model.decode(Files.readAllBytes(Paths.get("../data/problemsL/LA001_tgt.mdl")))
    val i1 = time(Interpreter.execute(model.copy(bitset = BitSet()), trace.toIndexedSeq, verbose = false))
    val i2 = time(Interpreter2.execute(model.copy(bitset = BitSet()), trace.toIndexedSeq, verbose = false))
    println(i1, i2)
    assert(i1 > i2)
  }

  ignore("サンプル186のenergyがInterpreterより速い") {
    val trace = TraceDecoder.decode(Files.readAllBytes(Paths.get("../data/dfltTracesL/LA186.nbt")))
    val model = Model.decode(Files.readAllBytes(Paths.get("../data/problemsL/LA186_tgt.mdl")))
    val i1 = time(Interpreter.execute(model.copy(bitset = BitSet()), trace.toIndexedSeq, verbose = false))
    val i2 = time(Interpreter2.execute(model.copy(bitset = BitSet()), trace.toIndexedSeq, verbose = false))
    println(i1, i2)
    assert(i1 > i2)
  }

  def time(run: => Unit): Long = {
    val st = System.currentTimeMillis()
    run
    System.currentTimeMillis() - st
  }
}
