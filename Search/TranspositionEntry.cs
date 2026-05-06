namespace ChessBot
{
  public readonly struct TranspositionEntry
  {
    public readonly ulong key;
    public readonly bool isValid;
    public readonly Move bestMove;
    public readonly int depth;
    public readonly int score;
    public readonly TranspositionFlag flag;

    public TranspositionEntry(ulong key, Move bestMove, int depthLeft,
        int bestScore, TranspositionFlag flag)
    {
      this.key = key;
      this.bestMove = bestMove;
      this.depth = depthLeft;
      this.score = bestScore;
      this.flag = flag;
      this.isValid = true;
    }
  }
}

