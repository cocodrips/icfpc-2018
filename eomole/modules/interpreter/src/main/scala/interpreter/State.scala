package interpreter

case class State(R: Int, energy: Long, harmonicsHigh: Boolean, matrix: Model, bots: IndexedSeq[Nanobot], trace: IndexedSeq[Command])

case class Nanobot(bid: Int, pos: Pos, seeds: Seq[Int], removed: Boolean = false)

object Nanobot {
  implicit val ordering: Ordering[Nanobot] = Ordering.by(_.bid)
}

case class Pos(x: Int, y: Int, z: Int) {
  def move(dx: Int, dy: Int, dz: Int): Pos = Pos(x + dx, y + dy, z + dz)
}
