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

case class FD(dx: Int, dy: Int, dz: Int) {
  require(-30 <= dx && dx <= 30, s"$dx $dy $dz")
  require(-30 <= dy && dy <= 30, s"$dx $dy $dz")
  require(-30 <= dz && dz <= 30, s"$dx $dy $dz")
  require(mlen >= 1, s"$dx $dy $dz")

  def mlen: Int = Math.abs(dx) + Math.abs(dy) + Math.abs(dz)
}

sealed trait Command

sealed trait SingletonCommand extends Command

sealed trait FusionCommand extends Command

sealed trait GFillCommand extends Command

sealed trait GVoidCommand extends Command

object Command {

  case object Halt extends SingletonCommand

  case object Wait extends SingletonCommand

  case object Flip extends SingletonCommand

  case class SMove(lld: LLD) extends SingletonCommand

  case class LMove(sld1: SLD, sld2: SLD) extends SingletonCommand

  case class FusionP(nd: ND) extends FusionCommand

  case class FusionS(nd: ND) extends FusionCommand

  case class Fission(nd: ND, m: Int) extends SingletonCommand {
    require(0 <= m && m <= 255)
  }

  case class Fill(nd: ND) extends SingletonCommand

  case class Void(nd: ND) extends SingletonCommand

  case class GFill(nd: ND, fd: FD) extends GFillCommand

  case class GVoid(nd: ND, fd: FD) extends GVoidCommand

}
