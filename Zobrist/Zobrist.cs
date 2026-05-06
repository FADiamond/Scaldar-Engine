namespace ChessBot
{
  public static class Zobrist
  {
    public static readonly Prng prng;

    public static readonly ulong[,] PieceSquare = new ulong[12, 64];

    public static readonly ulong[] CastlingRights = new ulong[4];

    public static readonly ulong[] EnPassantSquare = new ulong[16];

    public static readonly ulong BlackToMove;

    static Zobrist() {
      prng = new Prng();

      for (int piece = 0; piece < 12; piece++) {
        for (int square = 0; square < 64; square++) {
          PieceSquare[piece, square] = prng.Generate();
        }
      }

      for (int i = 0; i < 4; i++) {
        CastlingRights[i] = prng.Generate();
      }

      for (int file = 0; file < 16; file++) {
        EnPassantSquare[file] = prng.Generate();
      }

      BlackToMove = prng.Generate();

    }

    public static ulong computeKey(Board board) {
      ulong key = 0UL;
      for (byte square = 0; square < 64; square++) {
        Piece? piece = board.getPieceAtSquare(square);
        if (piece.HasValue)
          key ^= PieceSquare[(byte)piece, square];
      }

      if (board.whiteCanCastleKingSide)
        key ^= CastlingRights[0];
      if (board.whiteCanCastleQueenSide)
        key ^= CastlingRights[1];
      if (board.blackCanCastleKingSide)
        key ^= CastlingRights[2];
      if (board.blackCanCastleQueenSide)
        key ^= CastlingRights[3];

      if (board.enPassantSquare.HasValue) {
        int enPassantIndex = ((byte)board.enPassantSquare.Value) % 16;
        key ^= EnPassantSquare[enPassantIndex];
      }

      if (board.sideToMove == Side.Black)
        key ^= BlackToMove;

      return key;
    }

  }
}
 
