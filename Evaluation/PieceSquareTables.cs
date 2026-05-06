namespace ChessBot
{
  public static class PieceSquareTables
  {

    public static readonly int[] Pawns =
    [
       0,   0,   0,   0,   0,   0,   0,   0,
       5,  10,  10, -20, -20,  10,  10,   5,
       5,  -5, -10,   0,   0, -10,  -5,   5,
       0,   0,   0,  20,  20,   0,   0,   0,
       5,   5,  10,  25,  25,  10,   5,   5,
      10,  10,  20,  30,  30,  20,  10,  10,
      50,  50,  50,  50,  50,  50,  50,  50,
       0,   0,   0,   0,   0,   0,   0,   0
    ];

    public static readonly int[] Knights =
    [
      -50, -40, -30, -30, -30, -30, -40, -50,
      -40, -20,   0,   5,   5,   0, -20, -40,
      -30,   5,  10,  15,  15,  10,   5, -30,
      -30,   0,  15,  20,  20,  15,   0, -30,
      -30,   5,  15,  20,  20,  15,   5, -30,
      -30,   0,  10,  15,  15,  10,   0, -30,
      -40, -20,   0,   0,   0,   0, -20, -40,
      -50, -40, -30, -30, -30, -30, -40, -50
    ];

    public static readonly int[] Bishops =
    [
      -20, -10, -10, -10, -10, -10, -10, -20,
      -10,   5,   0,   0,   0,   0,   5, -10,
      -10,  10,  10,  10,  10,  10,  10, -10,
      -10,   0,  10,  10,  10,  10,   0, -10,
      -10,   5,   5,  10,  10,   5,   5, -10,
      -10,   0,   5,  10,  10,   5,   0, -10,
      -10,   0,   0,   0,   0,   0,   0, -10,
      -20, -10, -10, -10, -10, -10, -10, -20
    ];

    public static readonly int[] Rooks =
    [
       0,   0,   0,   5,   5,   0,   0,   0,
      -5,   0,   0,   0,   0,   0,   0,  -5,
      -5,   0,   0,   0,   0,   0,   0,  -5,
      -5,   0,   0,   0,   0,   0,   0,  -5,
      -5,   0,   0,   0,   0,   0,   0,  -5,
      -5,   0,   0,   0,   0,   0,   0,  -5,
       5,  10,  10,  10,  10,  10,  10,   5,
       0,   0,   0,   0,   0,   0,   0,   0
    ];

    public static readonly int[] Queens =
    [
      -20, -10, -10,  -5,  -5, -10, -10, -20,
      -10,   0,   5,   0,   0,   0,   0, -10,
      -10,   5,   5,   5,   5,   5,   0, -10,
        0,   0,   5,   5,   5,   5,   0,  -5,
       -5,   0,   5,   5,   5,   5,   0,  -5,
      -10,   0,   5,   5,   5,   5,   0, -10,
      -10,   0,   0,   0,   0,   0,   0, -10,
      -20, -10, -10,  -5,  -5, -10, -10, -20
    ];

    public static readonly int[] KingMiddleGame =
    [
       20,  30,  10,   0,   0,  10,  30,  20,
       20,  20,   0,   0,   0,   0,  20,  20,
      -10, -20, -20, -20, -20, -20, -20, -10,
      -20, -30, -30, -40, -40, -30, -30, -20,
      -30, -40, -40, -50, -50, -40, -40, -30,
      -30, -40, -40, -50, -50, -40, -40, -30,
      -30, -40, -40, -50, -50, -40, -40, -30,
      -30, -40, -40, -50, -50, -40, -40, -30
    ];

    public static readonly int[] KingEndGame =
    [
      -50, -30, -30, -30, -30, -30, -30, -50,
      -30, -30,   0,   0,   0,   0, -30, -30,
      -30, -10,  20,  30,  30,  20, -10, -30,
      -30, -10,  30,  40,  40,  30, -10, -30,
      -30, -10,  30,  40,  40,  30, -10, -30,
      -30, -10,  20,  30,  30,  20, -10, -30,
      -30, -20, -10,   0,   0, -10, -20, -30,
      -50, -40, -30, -20, -20, -30, -40, -50
    ];

    public static byte MirrorVertical(byte square)
    {
      return (byte)(square ^ 56);
    }

    public static int GetWhiteRelativeValue(Piece piece, byte square, bool endgame = false)
    {
      int index = piece.IsWhite()
        ? square
        : MirrorVertical(square);
      
      int value = GetTable(piece, endgame)[index];

      return piece.IsWhite() ? value : -value;
    }

    private static int[] GetTable(Piece piece, bool endgame)
    {
      return piece switch
      {
        Piece.WhitePawn or Piece.BlackPawn => Pawns,
        Piece.WhiteKnight or Piece.BlackKnight => Knights,
        Piece.WhiteBishop or Piece.BlackBishop => Bishops,
        Piece.WhiteRook or Piece.BlackRook => Rooks,
        Piece.WhiteQueen or Piece.BlackQueen => Queens,
        Piece.WhiteKing or Piece.BlackKing => endgame ? KingEndGame : KingMiddleGame,
        _ => throw new ArgumentOutOfRangeException(nameof(piece), piece, null)
      };
    }

  }
}

