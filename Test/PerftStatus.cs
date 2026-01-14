namespace chessBot.Test
{
  public class PerftStatus(ulong nodes = 0)
  {
    public ulong nodes = nodes;
    public ulong captures = 0UL;
    public ulong enPassant = 0UL;
    public ulong castles = 0UL;
    public ulong promotions = 0UL;

    public static PerftStatus operator +(PerftStatus status, PerftStatus other)
    {
      status.nodes += other.nodes;
      status.captures += other.captures;
      status.enPassant += other.enPassant;
      status.castles += other.castles;
      status.promotions += other.promotions;
      return status;
    }
  }
}

