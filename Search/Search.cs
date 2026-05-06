namespace ChessBot
{
  public class Search
  {

    private readonly int DEFAULT_DEPTH = 10;
    private const int Infinity = 1000000;
    private const int MateScore = 100000;

    public TranspositionTable transpositionTable = new(1 << 20);

    public void ClearTT()
    {
      transpositionTable.Clear();
    }

    // Iterative deepening
    public Move findBestMove(Board board, int? maxDepth, CancellationToken token)
    {
      Move bestMove = default;

      maxDepth ??= DEFAULT_DEPTH;

      // TimeControl timeControl = new();
      Board searchBoard = board.copy();
      for (int depth = 1; depth <= maxDepth; depth++)
      {
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

      int alpha = -Infinity;
      int beta = Infinity;

      List<Move> moves = MoveGeneration.generateMoves(board); ;
      Move bestMove = default;
      int bestScore = -Infinity;
      foreach (Move move in moves)
      {
        Side movingSide = board.sideToMove;
        Board nextBoard = board.copy();
        bool result = nextBoard.makeMove(move);
        if (!result || nextBoard.isInCheck(movingSide)) continue;
        int? score = -negaMax(nextBoard, -beta, -alpha, depth - 1, token);
        if (!score.HasValue || token.IsCancellationRequested) return null;

        if (score > bestScore)
        {
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

      ulong key = Zobrist.computeKey(board);

      if (transpositionTable.TryGet(key, out TranspositionEntry entry) &&
          entry.depth >= depthLeft)
      {
        if (entry.flag == TranspositionFlag.Exact)
          return entry.score;

        if (entry.flag == TranspositionFlag.LowerBound)
          alpha = Math.Max(alpha, entry.score);
        else if (entry.flag == TranspositionFlag.UpperBound)
          beta = Math.Min(beta, entry.score);

        if (alpha >= beta)
          return entry.score;
      }

      int originalAlpha = alpha;

      bool isInCheck = board.isInCheck(board.sideToMove);

      if (depthLeft <= 0 && !isInCheck) return Quiesce(board, alpha, beta);
      Move bestMove = default;
      int bestScore = -Infinity;
      bool hasLegalMove = false;

      List<Move> moves = MoveGeneration.generateMoves(board); ;
      foreach (Move move in moves)
      {
        Side movingSide = board.sideToMove;
        Board nextBoard = board.copy();
        bool result = nextBoard.makeMove(move);
        if (!result || nextBoard.isInCheck(movingSide)) continue;
        hasLegalMove = true;
        int? score = -negaMax(nextBoard, -beta, -alpha, depthLeft - 1, token);
        if (!score.HasValue || token.IsCancellationRequested) return null;

        if (score > bestScore)
        {
          bestScore = (int)score;
          bestMove = move;
        }

        if (score >= beta)
        {
          transpositionTable.Store(
            new TranspositionEntry(
              key,
              move,
              depthLeft,
              score.Value,
              TranspositionFlag.LowerBound
            )
          );

          return score;
        }
        if (score > alpha) alpha = score.Value;
      }
      if (!hasLegalMove)
        if (isInCheck)
          bestScore = -MateScore;
        else
          bestScore = 0;

      TranspositionFlag flag = bestScore <= originalAlpha
        ? TranspositionFlag.UpperBound
        : TranspositionFlag.Exact;

      transpositionTable.Store(
        new TranspositionEntry(
          key,
          bestMove,
          depthLeft,
          bestScore,
          flag
        )
      );

      return bestScore;
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
        Side movingSide = board.sideToMove;
        Board nextBoard = board.copy();
        bool result = nextBoard.makeMove(move);
        if (!result || nextBoard.isInCheck(movingSide)) continue;
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
