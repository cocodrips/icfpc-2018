package interpreter

import java.nio.file.{Files, Paths}

import org.scalatest.FunSuite

class TraceDecoderEncoderTest extends FunSuite {
  (1 to 186).foreach { i =>
    test("公式サンプルが読み書きできる LA%03d.nbt".format(i)) {
      val input = Files.readAllBytes(Paths.get("../data/dfltTracesL/LA%03d.nbt".format(i)))
      val commands = TraceDecoder.decode(input)
      val output = TraceEncoder.encode(commands)
      require(input.seq == output.seq)
    }
  }
}
