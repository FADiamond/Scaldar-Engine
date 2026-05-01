namespace ChessBot
{
  public class Evaluation
  {
    // All values will be in centipawns
    private const int PawnValue = 100;
    private const int KnightValue = 350;
    private const int BishopValue = 350;
    private const int RookValue = 525;
    private const int QueenValue = 1000;

    public static int Evaluate(Board board)
    {
      int currentStateValue = pieceCountEvaluation(board);

      return currentStateValue;
    }

    public static int EvaluateForSideToMove(Board board)
    {
      int whiteRelativeEval = Evaluate(board);

      return board.sideToMove == Side.White
        ? whiteRelativeEval
        : -whiteRelativeEval;

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

