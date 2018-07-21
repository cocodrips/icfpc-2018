package interpreter

import java.nio.file.{Files, Paths}

import org.scalatest.FunSuite

class ModelTest extends FunSuite {

  (1 to 186).foreach { i =>
    test("公式サンプルが読める LA%03d_tgt.mdl".format(i)) {
      val input = Files.readAllBytes(Paths.get("../data/problemsL/LA%03d_tgt.mdl".format(i)))
      val model = Model.decode(input)
      require(input.tail.map(_.toInt & 255).map(Integer.bitCount).sum === model.bitset.size)
    }
  }

}
