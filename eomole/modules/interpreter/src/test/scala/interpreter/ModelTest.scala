package interpreter

import java.nio.file.{Files, Paths}

import org.scalatest.FunSuite

class ModelTest extends FunSuite {

  def bb(model: Model): (Int, Int, Int) = {
    val R = model.R
    var minX = R
    var minY = R
    var minZ = R
    var maxX = 0
    var maxY = 0
    var maxZ = 0
    for {
      x <- 0 until R
      y <- 0 until R
      z <- 0 until R
      if model.get(Pos(x, y, z))
    } {
      minX = Math.min(minX, x)
      minY = Math.min(minY, y)
      minZ = Math.min(minZ, z)
      maxX = Math.max(maxX, x)
      maxY = Math.max(maxY, y)
      maxZ = Math.max(maxZ, z)
    }
    (maxX - minX + 1, maxY - minY + 1, maxZ - minZ + 1)
  }

  (1 to 186).foreach { i =>
    test("公式サンプルが読める FA%03d_tgt.mdl".format(i)) {
      val input = Files.readAllBytes(Paths.get("../data/problems/FA%03d_tgt.mdl".format(i)))
      val model = Model.decode(input)
      println(i, model.R, bb(model))
      assertResult(input.tail.map(_.toInt & 255).map(Integer.bitCount).sum)(model.bitset.size)
    }
  }

  (1 to 115).foreach { i =>
    test("公式サンプルが読める FR%03d_src.mdl".format(i)) {
      val input = Model.decode(Files.readAllBytes(Paths.get("../data/problems/FR%03d_src.mdl".format(i))))
      val output = Model.decode(Files.readAllBytes(Paths.get("../data/problems/FR%03d_tgt.mdl".format(i))))
      println(i, input.R, "src" -> bb(input), "tgt" -> bb(output))
    }
  }

}
