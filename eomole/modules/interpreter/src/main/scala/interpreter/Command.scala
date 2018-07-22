package interpreter

sealed trait Axis

object Axis {

  case object X extends Axis

  case object Y extends Axis

  case object Z extends Axis

}

case class LLD(axis: Axis, d: Int) {
  require(-15 <= d && d <= 15, s"$axis $d")

  def dx: Int = if (axis == Axis.X) d else 0

  def dy: Int = if (axis == Axis.Y) d else 0

  def dz: Int = if (axis == Axis.Z) d else 0

  def mlen: Int = Math.abs(d)
}

case class SLD(axis: Axis, d: Int) {
  require(-5 <= d && d <= 5, s"$axis $d")

  def dx: Int = if (axis == Axis.X) d else 0

  def dy: Int = if (axis == Axis.Y) d else 0

  def dz: Int = if (axis == Axis.Z) d else 0

  def mlen: Int = Math.abs(d)
}

case class ND(dx: Int, dy: Int, dz: Int) {
  require(-1 <= dx && dx <= 1, s"$dx $dy $dz")
  require(-1 <= dy && dy <= 1, s"$dx $dy $dz")
  require(-1 <= dz && dz <= 1, s"$dx $dy $dz")
  require(mlen >= 1, s"$dx $dy $dz")
  require(mlen <= 2, s"$dx $dy $dz")

  def mlen: Int = Math.abs(dx) + Math.abs(dy) + Math.abs(dz)
}

sealed trait Command

object Command {

  case object Halt extends Command

  case object Wait extends Command

  case object Flip extends Command

  case class SMove(lld: LLD) extends Command

  case class LMove(sld1: SLD, sld2: SLD) extends Command

  case class FusionP(nd: ND) extends Command

  case class FusionS(nd: ND) extends Command

  case class Fission(nd: ND, m: Int) extends Command {
    require(0 <= m && m <= 255)
  }

  case class Fill(nd: ND) extends Command

}
