package interpreter

import scala.collection.BitSet

case class Model(R: Int, bitset: BitSet) {
  def get(pos: Pos): Boolean = bitset(pos.x * R * R + pos.y * R + pos.z)

  def add(pos: Pos): Model =
    copy(bitset = bitset + (pos.x * R * R + pos.y * R + pos.z))
}

object Model {
  def decode(bytes: Seq[Byte]): Model = bytes.toStream.map(_.toInt & 255) match {
    case head +: tail =>
      val bitset = BitSet.newBuilder
      tail.zipWithIndex.foreach {
        case (byte, idx1) => (0 until 8).foreach(idx2 => if ((1 << idx2 & byte) > 0) bitset += (idx1 * 8 + idx2))
      }
      new Model(head & 255, bitset.result())
  }
}