package interpreter

//import java.nio.file.{Files, Paths}

import scala.annotation.tailrec
import scala.collection.mutable.ArrayBuffer

object TraceDecoder {

  private implicit class FromBinaryString(val sc: StringContext) extends AnyVal {
    def b(args: Any*): Int = Integer.parseInt(sc.s(args: _*), 2)
  }

  private def decodeAxis(a: Int): Axis = a match {
    case 1 => Axis.X
    case 2 => Axis.Y
    case 3 => Axis.Z
  }

  private def decodeND(nd: Int): ND = ND(nd / 9 - 1, nd / 3 % 3 - 1, nd % 3 - 1)

  private def decodeLLD(a: Int, i: Int): LLD = LLD(decodeAxis(a), i - 15)

  private def decodeSLD(a: Int, i: Int): SLD = SLD(decodeAxis(a), i - 5)

  @tailrec
  def decode(bytes: Seq[Byte], acc: ArrayBuffer[Command] = ArrayBuffer()): Seq[Command] =
    if (bytes.isEmpty) acc.result() else {
      val head = bytes.head & 255
      (head >> 3, head & 7, bytes.tail.toStream) match {
        case (31, 7, tail) => decode(tail, acc += Command.Halt)
        case (31, 6, tail) => decode(tail, acc += Command.Wait)
        case (31, 5, tail) => decode(tail, acc += Command.Flip)
        case (d, 4, h +: t) => (d >> 1, d & 1) match {
          case (a, 0) => decode(t, acc += Command.SMove(decodeLLD(a, h & 255)))
          case (a, 1) => decode(t, acc += Command.LMove(decodeSLD(a & 3, h & 15), decodeSLD(a >> 2, (h & 255) >> 4)))
          case _ => throw new UnsupportedOperationException
        }
        case (nd, 7, tail) => decode(tail, acc += Command.FusionP(decodeND(nd)))
        case (nd, 6, tail) => decode(tail, acc += Command.FusionS(decodeND(nd)))
        case (nd, 5, h +: t) => decode(t, acc += Command.Fission(decodeND(nd), h))
        case (nd, 3, tail) => decode(tail, acc += Command.Fill(decodeND(nd)))
        case _ => throw new UnsupportedOperationException
      }
    }

//  def main(args: Array[String]): Unit = {
//    (1 to 186).foreach { i =>
//      val input = Files.readAllBytes(Paths.get("../data/dfltTracesL/LA%03d.nbt".format(i)))
//      val commands = decode(input)
//      val output = TraceEncoder.encode(commands)
//      require(input.seq == output.seq)
//      println(i)
//    }
//  }

}
