package interpreter

object TraceEncoder {

  private implicit class FromBinaryString(val sc: StringContext) extends AnyVal {
    def b(args: Any*): Int = Integer.parseInt(sc.s(args: _*), 2)
  }

  private def encode(axis: Axis): Int = axis match {
    case Axis.X => 1
    case Axis.Y => 2
    case Axis.Z => 3
  }

  private def encode(nd: ND): Int = (nd.dx + 1) * 9 + (nd.dy + 1) * 3 * nd.dz + 1

  private def a(lld: LLD): Int = encode(lld.axis)

  private def i(sld: SLD): Int = sld.d + 5

  private def a(sld: SLD): Int = encode(sld.axis)

  private def i(lld: LLD): Int = lld.d + 15

  def encode(commands: Seq[Command]): Seq[Byte] = commands.flatMap {
    case Command.Halt => Seq(b"11111111")
    case Command.Wait => Seq(b"11111110")
    case Command.Flip => Seq(b"11111101")
    case Command.SMove(lld) => Seq(
      b"0100" | a(lld) << 4,
      i(lld)
    )
    case Command.LMove(sld1, sld2) => Seq(
      b"1100" | a(sld1) << 4 | a(sld2) << 6,
      i(sld1) | i(sld2) << 4
    )
    case Command.FusionP(nd) => Seq(b"111" | encode(nd) << 3)
    case Command.FusionS(nd) => Seq(b"110" | encode(nd) << 3)
    case Command.Fission(nd: ND, m: Int) => Seq(b"101" | encode(nd) << 3, m.toByte)
    case Command.Fill(nd: ND) => Seq(b"011" | encode(nd) << 3)
  }.map(_.toByte)

}
