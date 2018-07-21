package interpreter

import java.nio.file.{Files, Paths}

import org.scalatest.FunSuite

class TraceDecoderEncoderTest extends FunSuite {
  (1 to 186).foreach { i =>
    test("公式サンプルが読み書きできる LA%03d.nbt".format(i)) {
      val input = Files.readAllBytes(Paths.get("../data/dfltTracesL/LA%03d.nbt".format(i)))
      val commands = TraceDecoder.decode(input)
      val output = TraceEncoder.encode(commands)
      assertResult(input.seq)(output.seq)
    }
  }

  test("読み書きできる") {
    val input = Seq(Command.Fission(ND(1, 0, 0), 0), Command.FusionP(ND(1, 0, 0)), Command.FusionS(ND(-1, 0, 0)), Command.Halt)
    val output = TraceDecoder.decode(TraceEncoder.encode(input))
    assertResult(input.seq)(output.seq)
  }
}
