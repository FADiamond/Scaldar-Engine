namespace ChessBot
{
  public static class LongAlgebraicConverter
  {
    public static string moveToAlgebraic(Move move)
    {
      string moveText = $"{(Square)move.fromSquare}{(Square)move.toSquare}";
      if ((move.flags & MoveFlags.QueenPromotion) != 0)
        moveText += "q";
      else if ((move.flags & MoveFlags.RookPromotion) != 0)
        moveText += "r";
      else if ((move.flags & MoveFlags.KnightPromotion) != 0)
        moveText += "n";
      else if ((move.flags & MoveFlags.BishopPromotion) != 0)
        moveText += "b";
      return moveText;
    }

    public static Move? algebraicToMove(string algebraicMove, Board board)
    {
      Move? move = null;
      if (algebraicMove.Length < 4)
        return null;

      if (Enum.TryParse(algebraicMove[..2], true, out Square fromSquare) &&
          Enum.TryParse(algebraicMove[2..4], true, out Square toSquare))
      {
        byte from = (byte)fromSquare;
        byte to = (byte)toSquare;
        Piece? piece = board.getPieceAtSquare(from);

        if (!piece.HasValue)
        {
          return null;
        }

        string? promotion = algebraicMove.Length > 4 ? algebraicMove[4..].ToLower() : null;

        MoveFlags flags = getMoveFlagsForMove(from, to, (Piece)piece, promotion, board);

        move = new Move(from, to, (Piece)piece, flags);
      }
      return move;
    }

    private static MoveFlags getMoveFlagsForMove(byte fromSquare, byte toSquare, Piece fromPiece,
        string? promotion, Board board)
    {
      MoveFlags move = MoveFlags.None;
      if (promotion != null)
      {
        if (promotion == "q")
          move |= MoveFlags.QueenPromotion;
        else if (promotion == "r")
          move |= MoveFlags.RookPromotion;
        else if (promotion == "n")
          move |= MoveFlags.KnightPromotion;
        else if (promotion == "b")
          move |= MoveFlags.BishopPromotion;
      }

      if (board.getPieceAtSquare(toSquare).HasValue)
      {
        move |= MoveFlags.Capture;
      }

      if (fromPiece == Piece.WhitePawn || fromPiece == Piece.BlackPawn)
      {

        if (board.enPassantSquare.HasValue && toSquare == (byte)board.enPassantSquare.Value)
          move |= MoveFlags.EnPassant | MoveFlags.Capture;
        else if (Math.Abs(fromSquare - toSquare) == 16)
          move |= MoveFlags.DoublePush;
      }
      
      if (fromPiece == Piece.WhiteKing) {
        if (fromSquare == (byte)Square.e1 && toSquare == (byte)Square.g1)
          move |= MoveFlags.CastleKingSide;
        else if (fromSquare == (byte)Square.e1 && toSquare == (byte)Square.c1)
          move |= MoveFlags.CastleQueenSide;
      }
      else if (fromPiece == Piece.BlackKing) {
        if (fromSquare == (byte)Square.e8 && toSquare == (byte)Square.g8)
          move |= MoveFlags.CastleKingSide;
        else if (fromSquare == (byte)Square.e8 && toSquare == (byte)Square.c8)
          move |= MoveFlags.CastleQueenSide;
      }

      return move;
    }

  }
}
