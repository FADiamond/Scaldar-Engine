namespace chessBot
{
  public readonly struct Move(byte fromSquare, byte toSquare, Piece piece, MoveFlags flags)
  {
    public readonly byte fromSquare = fromSquare;
    public readonly byte toSquare = toSquare;
    public readonly Piece piece = piece;
    public readonly MoveFlags flags = flags;
  }
}

