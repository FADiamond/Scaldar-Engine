namespace chessBot.Search
{
  public class Evaluation
  {
    // All values will be in centipawns
    private const int PawnValue   = 100;
    private const int KnightValue = 350;
    private const int BishopValue = 350;
    private const int RookValue   = 525;
    private const int QueenValue  = 1000;
    
    public static int Evaluate(Board board) {
      int currentStateValue = pieceCountEvaluation(board);

      if (board.sideToMove.Equals(Side.Black)) currentStateValue *= -1;
      return currentStateValue;
    }

    private static int pieceCountEvaluation(Board board) {
      Piece currentPawn = board.sideToMove.Equals(Side.White) ? Piece.WhitePawn : Piece.BlackPawn;
      Piece currentKnight = board.sideToMove.Equals(Side.White) ? Piece.WhiteKnight : Piece.BlackKnight;
      Piece currentBishop = board.sideToMove.Equals(Side.White) ? Piece.WhiteBishop : Piece.BlackBishop;
      Piece currentRook = board.sideToMove.Equals(Side.White) ? Piece.WhiteRook : Piece.BlackRook;
      Piece currentQueen = board.sideToMove.Equals(Side.White) ? Piece.WhiteQueen : Piece.BlackQueen;

      Piece opponentPawn = board.sideToMove.Equals(Side.White) ? Piece.BlackPawn : Piece.WhitePawn;
      Piece opponentKnight = board.sideToMove.Equals(Side.White) ? Piece.BlackKnight : Piece.WhiteKnight;
      Piece opponentBishop = board.sideToMove.Equals(Side.White) ? Piece.BlackBishop : Piece.WhiteBishop;
      Piece opponentRook = board.sideToMove.Equals(Side.White) ? Piece.BlackRook : Piece.WhiteRook;
      Piece opponentQueen = board.sideToMove.Equals(Side.White) ? Piece.BlackQueen : Piece.WhiteQueen;

      int pawnAmount = BitboardHelper.NumberOfSetBits(board.bitboards[(byte)currentPawn]);
      int knightAmount = BitboardHelper.NumberOfSetBits(board.bitboards[(byte)currentKnight]);
      int bishopAmount = BitboardHelper.NumberOfSetBits(board.bitboards[(byte)currentBishop]);
      int rookAmount = BitboardHelper.NumberOfSetBits(board.bitboards[(byte)currentRook]);
      int queenAmount = BitboardHelper.NumberOfSetBits(board.bitboards[(byte)currentQueen]);

      int opponentPawnAmount = BitboardHelper.NumberOfSetBits(board.bitboards[(byte)opponentPawn]);
      int opponentKnightAmount = BitboardHelper.NumberOfSetBits(board.bitboards[(byte)opponentKnight]);
      int opponentBishopAmount = BitboardHelper.NumberOfSetBits(board.bitboards[(byte)opponentBishop]);
      int opponentRookAmount = BitboardHelper.NumberOfSetBits(board.bitboards[(byte)opponentRook]);
      int opponentQueenAmount = BitboardHelper.NumberOfSetBits(board.bitboards[(byte)opponentQueen]);

      return (pawnAmount - opponentPawnAmount) * PawnValue +
             (knightAmount - opponentKnightAmount) * KnightValue +
             (bishopAmount - opponentBishopAmount) * BishopValue +
             (rookAmount - opponentRookAmount) * RookValue +
             (queenAmount - opponentQueenAmount) * QueenValue;
    }
    


  }
}
 
