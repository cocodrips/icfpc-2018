package interpreter

import monocle.macros.Lenses

import scala.collection.BitSet

@Lenses
case class Model(R: Int, bitset: BitSet) {

  def encode(pos: Pos): Option[Int] =
    if (pos.x < 0 || R <= pos.x || pos.y < 0 || R <= pos.y || pos.z < 0 || R <= pos.z)
      None
    else
      Some(pos.x * R * R + pos.y * R + pos.z)

  def get(pos: Pos): Boolean = encode(pos).exists(bitset(_))

  def add(pos: Pos): Model = encode(pos).fold(this)(p => copy(bitset = bitset + p))

  def del(pos: Pos): Model = encode(pos).fold(this)(p => copy(bitset = bitset - p))

  def add(seq: Seq[Pos]): Model = copy(bitset = bitset ++ seq.flatMap(encode))

  def del(seq: Seq[Pos]): Model = copy(bitset = bitset -- seq.flatMap(encode))
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

  def empty(R: Int): Model = Model(R, BitSet())
}
