namespace ChessBot
{
  public class Search
  {

    private readonly int DEFAULT_DEPTH = 10;

    // Iterative deepening
    public Move findBestMove(Board board, int? maxDepth, CancellationToken token) {
      Move bestMove = default;

      maxDepth ??= DEFAULT_DEPTH;
      
      // TimeControl timeControl = new();
      Board searchBoard = board.copy();
      for (int depth = 1; depth <= maxDepth; depth++) {
        if (token.IsCancellationRequested)
          break;

        Move? move = getNegamaxBestMove(searchBoard, depth, token);

        if (!token.IsCancellationRequested && move.HasValue)
          bestMove = (Move)move;

      }
      return bestMove;
    }

    public Move? getNegamaxBestMove(Board board, int depth, CancellationToken token)
    {
      if (token.IsCancellationRequested) return null;

      int alpha = int.MinValue/2;
      int beta = int.MaxValue/2;

      List<Move> moves = MoveGeneration.generateMoves(board); ;
      Move bestMove = default;
      int bestScore = int.MinValue/2;
      foreach (Move move in moves)
      {
        Board nextBoard = board.copy();
        bool result = nextBoard.makeMove(move);
        if (!result || nextBoard.isInCheck()) continue;
        int? score = -negaMax(nextBoard, -beta, -alpha, depth - 1, token);
        if (!score.HasValue || token.IsCancellationRequested) return null;

        if (score > bestScore) {
          bestMove = move;
          bestScore = (int)score;
        }
        if (score > alpha) alpha = (int)score;
      }

      return bestMove;
    }

    private int? negaMax(Board board, int alpha, int beta, int depthLeft, CancellationToken token)
    {
      if (token.IsCancellationRequested) return null;
      if (depthLeft == 0) return Quiesce(board, alpha, beta);
      int best = int.MinValue/2;

      List<Move> moves = MoveGeneration.generateMoves(board); ;
      foreach (Move move in moves)
      {
        Board nextBoard = board.copy();
        bool result = nextBoard.makeMove(move);
        if (!result || nextBoard.isInCheck()) continue;
        int? score = -negaMax(nextBoard, -beta, -alpha, depthLeft - 1, token);
        if (!score.HasValue || token.IsCancellationRequested) return null;

        if (score > best) best = (int)score;
        if (score > alpha) alpha = (int)score;
        if (alpha >= beta)
          return best;
      }
      return alpha;
    }

    private int Quiesce(Board board, int alpha, int beta)
    {
      int staticEval = Evaluation.EvaluateForSideToMove(board);

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
