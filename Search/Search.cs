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
    public Move? findBestMove(Board board, int? maxDepth, CancellationToken token,
        IReadOnlyDictionary<ulong, int>? repetitionCounts = null)
    {
      maxDepth ??= DEFAULT_DEPTH;

      Board searchBoard = board.copy();
      Move? bestMove = FindFirstLegalMove(searchBoard);
      Dictionary<ulong, int> searchRepetitionCounts = repetitionCounts != null
        ? new Dictionary<ulong, int>(repetitionCounts)
        : [];
      EnsurePositionIsCounted(searchRepetitionCounts, board.zobristKey);

      for (int depth = 1; depth <= maxDepth; depth++)
      {
        if (token.IsCancellationRequested)
          break;

        Move? move = getNegamaxBestMove(searchBoard, depth, token, searchRepetitionCounts);

        if (!token.IsCancellationRequested && move.HasValue)
          bestMove = (Move)move;

      }
      return bestMove;
    }

    public Move? getNegamaxBestMove(Board board, int depth, CancellationToken token,
        Dictionary<ulong, int>? repetitionCounts = null)
    {
      if (token.IsCancellationRequested) return null;

      int alpha = -Infinity;
      int beta = Infinity;
      Dictionary<ulong, int> searchRepetitionCounts = repetitionCounts ?? [];
      EnsurePositionIsCounted(searchRepetitionCounts, board.zobristKey);

      List<Move> moves = MoveGeneration.generateMoves(board); ;
      Move bestMove = default;
      int bestScore = -Infinity;
      foreach (Move move in moves)
      {
        Side movingSide = board.sideToMove;
        Board nextBoard = board.copy();
        bool result = nextBoard.makeMove(move);
        if (!result || nextBoard.isInCheck(movingSide)) continue;
        Dictionary<ulong, int> childRepetitionCounts = EnterPosition(searchRepetitionCounts, nextBoard.zobristKey, move);
        int? score = -negaMax(nextBoard, -beta, -alpha, depth - 1, 0, token, childRepetitionCounts);
        LeavePosition(searchRepetitionCounts, nextBoard.zobristKey, move, childRepetitionCounts);
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

    private static Move? FindFirstLegalMove(Board board)
    {
      List<Move> moves = MoveGeneration.generateMoves(board);

      foreach (Move move in moves)
      {
        Side movingSide = board.sideToMove;
        Board nextBoard = board.copy();
        bool result = nextBoard.makeMove(move);
        if (result && !nextBoard.isInCheck(movingSide))
          return move;
      }

      return null;
    }

    private int? negaMax(Board board, int alpha, int beta, int depthLeft, int ply, CancellationToken token,
        Dictionary<ulong, int> repetitionCounts)
    {
      if (token.IsCancellationRequested) return null;

      ulong key = board.zobristKey;

      if (IsThreefoldRepetition(key, repetitionCounts))
        return 0;

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

      if (depthLeft <= 0 && !isInCheck) return Quiesce(board, alpha, beta, token);
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
        Dictionary<ulong, int> childRepetitionCounts = EnterPosition(repetitionCounts, nextBoard.zobristKey, move);
        int? score = -negaMax(nextBoard, -beta, -alpha, depthLeft - 1, ply + 1, token, childRepetitionCounts);
        LeavePosition(repetitionCounts, nextBoard.zobristKey, move, childRepetitionCounts);
        if (!score.HasValue || token.IsCancellationRequested) return null;

        if (score > bestScore)
        {
          bestScore = score.Value;
          bestMove = move;
        }
        if (score > alpha)
          alpha = score.Value;

        if (alpha >= beta)
        {
          transpositionTable.Store(
            new TranspositionEntry(
              key,
              bestMove,
              depthLeft,
              bestScore,
              TranspositionFlag.LowerBound
            )
          );

          return bestScore;
        }
      }
      if (!hasLegalMove)
        if (isInCheck)
          bestScore = -MateScore + ply;
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

    private static bool IsThreefoldRepetition(ulong zobristKey, Dictionary<ulong, int> repetitionCounts)
    {
      return repetitionCounts.TryGetValue(zobristKey, out int repetitions) && repetitions >= 3;
    }

    private static Dictionary<ulong, int> EnterPosition(Dictionary<ulong, int> repetitionCounts, ulong zobristKey,
        Move move)
    {
      if (IsIrreversibleMove(move))
      {
        Dictionary<ulong, int> resetCounts = [];
        AddPositionToRepetitionCounts(resetCounts, zobristKey);
        return resetCounts;
      }

      AddPositionToRepetitionCounts(repetitionCounts, zobristKey);
      return repetitionCounts;
    }

    private static void LeavePosition(Dictionary<ulong, int> parentCounts, ulong zobristKey, Move move,
        Dictionary<ulong, int> childCounts)
    {
      if (!IsIrreversibleMove(move) && ReferenceEquals(parentCounts, childCounts))
        RemovePositionFromRepetitionCounts(parentCounts, zobristKey);
    }

    private static void EnsurePositionIsCounted(Dictionary<ulong, int> repetitionCounts, ulong zobristKey)
    {
      if (!repetitionCounts.ContainsKey(zobristKey))
        AddPositionToRepetitionCounts(repetitionCounts, zobristKey);
    }

    private static void AddPositionToRepetitionCounts(Dictionary<ulong, int> repetitionCounts, ulong zobristKey)
    {
      repetitionCounts.TryGetValue(zobristKey, out int count);
      repetitionCounts[zobristKey] = count + 1;
    }

    private static void RemovePositionFromRepetitionCounts(Dictionary<ulong, int> repetitionCounts, ulong zobristKey)
    {
      if (!repetitionCounts.TryGetValue(zobristKey, out int count))
        return;

      if (count <= 1)
        repetitionCounts.Remove(zobristKey);
      else
        repetitionCounts[zobristKey] = count - 1;
    }

    private static bool IsIrreversibleMove(Move move)
    {
      return (move.flags & MoveFlags.Capture) != 0 ||
          move.piece == Piece.WhitePawn ||
          move.piece == Piece.BlackPawn;
    }

    private int? Quiesce(Board board, int alpha, int beta, CancellationToken token)
    {
      if (token.IsCancellationRequested) return null;

      int staticEval = Evaluation.EvaluateForSideToMove(board);

      if (staticEval >= beta)
        return staticEval;

      if (staticEval > alpha)
        alpha = staticEval;

      List<Move> moves = MoveGeneration.generateMoves(board); ;
      foreach (Move move in moves)
      {
        if (token.IsCancellationRequested) return null;

        if ((move.flags & MoveFlags.Capture) == 0) continue;
        Side movingSide = board.sideToMove;
        Board nextBoard = board.copy();
        bool result = nextBoard.makeMove(move);
        if (!result || nextBoard.isInCheck(movingSide)) continue;
        int? childScore = Quiesce(nextBoard, -beta, -alpha, token);
        if (!childScore.HasValue || token.IsCancellationRequested) return null;

        int score = -childScore.Value;

        if (score >= beta)
          return score;

        if (score > alpha)
          alpha = score;
      }

      return alpha;
    }


  }
}
