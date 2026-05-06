namespace ChessBot
{
  public class Evaluation
  {
    // All values will be in centipawns
    private const int PawnValue = 100;
    private const int KnightValue = 320;
    private const int BishopValue = 330;
    private const int RookValue = 500;
    private const int QueenValue = 900;
    private const int KingValue = 20000;

    public static int Evaluate(Board board)
    {
      int currentStateValue = pieceCountEvaluation(board);
      int piecePositionValue = PieceSquareTableEvaluation(board);

      return currentStateValue + piecePositionValue;
    }

    public static int EvaluateForSideToMove(Board board)
    {
      int whiteRelativeEval = Evaluate(board);

      return board.sideToMove == Side.White
        ? whiteRelativeEval
        : -whiteRelativeEval;

    }
    public static int PieceSquareTableEvaluation(Board board)
    {
      int totalValue = 0;
      for (byte i = 0; i < 64; i++)
      {
        Piece? piece = board.getPieceAtSquare(i);
        if (piece.HasValue)
        {
          if (board.sideToMove.Equals(Side.White))
          {
            totalValue += PieceSquareTables.GetWhiteRelativeValue(piece.Value, i);
          }
          else
          {
            totalValue += PieceSquareTables.GetWhiteRelativeValue(piece.Value, i);
            totalValue = -totalValue;
          }
        }
      }

      return totalValue;
    }

    private static int pieceCountEvaluation(Board board)
    {
      return
        EvaluatePiece(board, Piece.WhitePawn, Piece.BlackPawn, PawnValue) +
        EvaluatePiece(board, Piece.WhiteKnight, Piece.BlackKnight, KnightValue) +
        EvaluatePiece(board, Piece.WhiteBishop, Piece.BlackBishop, BishopValue) +
        EvaluatePiece(board, Piece.WhiteRook, Piece.BlackRook, RookValue) +
        EvaluatePiece(board, Piece.WhiteQueen, Piece.BlackQueen, QueenValue);
    }

    private static int EvaluatePiece(Board board, Piece whitePiece, Piece blackPiece, int value)
    {
      int whiteCount = BitboardHelper.NumberOfSetBits(board.bitboards[(byte)whitePiece]);
      int blackCount = BitboardHelper.NumberOfSetBits(board.bitboards[(byte)blackPiece]);

      return (whiteCount - blackCount) * value;
    }


  }
}

