namespace chessBot.Test
{
  public class PerftStatus(ulong nodes = 0, ulong captures = 0UL, ulong enPassant = 0UL, ulong castles = 0UL, ulong promotions = 0UL)
  {
    public ulong nodes = nodes;
    public ulong captures = captures; 
    public ulong enPassant = enPassant;
    public ulong castles = castles;
    public ulong promotions = promotions;

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

