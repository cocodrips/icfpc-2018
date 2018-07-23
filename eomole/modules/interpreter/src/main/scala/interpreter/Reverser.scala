package interpreter

object Reverser {

  def reverse(commands: Seq[Command]): Seq[Command] =
    commands.reverse match {
      case Command.Halt +: tail => tail.map(trans) :+ Command.Halt
      case seq => seq.map(trans)
    }

  private def trans(c: Command): Command = c match {
    case Command.SMove(lld) => Command.SMove(LLD(lld.axis, -lld.d))
    case Command.LMove(sld1, sld2) => Command.LMove(SLD(sld1.axis, -sld1.d), SLD(sld2.axis, -sld2.d))
    case Command.Fill(nd) => Command.Void(nd)
    case Command.Void(nd) => Command.Fill(nd)
    case Command.GFill(nd, fd) => Command.GVoid(nd, fd)
    case Command.GVoid(nd, fd) => Command.GFill(nd, fd)
    // IDが面倒でサポートできない
    case Command.FusionS(_) | Command.FusionP(_) | Command.Fission(_, _) => throw new UnsupportedOperationException
    case c => c
  }
}
