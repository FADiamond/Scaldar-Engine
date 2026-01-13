namespace chessBot.Pieces
{
  public static class PieceExtensions
  {
    public static bool IsBlack(this Piece piece)
    {
      return piece.Equals(Piece.BlackPawn) || 
             piece.Equals(Piece.BlackKnight) || 
             piece.Equals(Piece.BlackBishop) || 
             piece.Equals(Piece.BlackRook) || 
             piece.Equals(Piece.BlackQueen) || 
             piece.Equals(Piece.BlackKing);
    }

    public static bool IsWhite(this Piece piece)
    {
      return piece.Equals(Piece.WhitePawn) || 
             piece.Equals(Piece.WhiteKnight) || 
             piece.Equals(Piece.WhiteBishop) || 
             piece.Equals(Piece.WhiteRook) || 
             piece.Equals(Piece.WhiteQueen) || 
             piece.Equals(Piece.WhiteKing);
    }
  }
}
 
