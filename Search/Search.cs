namespace chessBot.Search
{
  public static class Search
  {
    // Iterative deepening
    public static Move? findBestMove(Board board, int maxDepth = 4) {
      Move bestMove = default;
      
      // TimeControl timeControl = new();
      Board searchBoard = board.copy();
      for (int depth = 1; depth < maxDepth; depth++) {
        bestMove = getNegamaxBestMove(searchBoard, depth);

        
      }
      return bestMove;
    }

    public static Move getNegamaxBestMove(Board board, int depth = 4)
    {
      int alpha = int.MinValue;
      int beta = int.MaxValue;

      List<Move> moves = MoveGeneration.generateMoves(board); ;
      Move bestMove = default;
      int bestScore = int.MinValue;
      foreach (Move move in moves)
      {
        Board nextBoard = board.copy();
        bool result = nextBoard.makeMove(move);
        if (!result || nextBoard.isInCheck()) continue;
        int score = negaMax(board, -beta, -alpha, depth - 1);

        if (score > bestScore) {
          bestMove = move;
          bestScore = score;
        }
        if (score > alpha) alpha = score;
      }

      return bestMove;
    }

    private static int negaMax(Board board, int alpha, int beta, int depthLeft)
    {
      if (depthLeft == 0) return Quiesce(board, alpha, beta);
      int best = int.MinValue;

      List<Move> moves = MoveGeneration.generateMoves(board); ;
      foreach (Move move in moves)
      {
        Board nextBoard = board.copy();
        bool result = nextBoard.makeMove(move);
        if (!result || nextBoard.isInCheck()) continue;
        int score = -negaMax(nextBoard, -alpha, -beta, depthLeft - 1);

        if (score > best) best = score;
        if (score > alpha) alpha = score;
        if (alpha >= beta)
          return best;
      }
      return alpha;
    }

    private static int Quiesce(Board board, int alpha, int beta)
    {
      int staticEval = Evaluation.Evaluate(board);

      if (staticEval >= beta)
        return staticEval;

      if (staticEval > alpha)
        alpha = staticEval;

      List<Move> moves = MoveGeneration.generateMoves(board); ;
      foreach (Move move in moves)
      {
        if ((move.flags & MoveFlags.Capture) == 0) continue;
        Board nextBoard = board.copy();
        bool result = nextBoard.makeMove(move);
        if (!result || nextBoard.isInCheck()) continue;
        int score = -Quiesce(nextBoard, -beta, -alpha);

        if (score >= beta)
          return score;

        if (score > alpha)
          alpha = score;
      }

      return alpha;
    }

  }
}

