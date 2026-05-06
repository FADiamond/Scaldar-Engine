namespace ChessBot
{
  public class TimeControl
  {
    public int? whiteTimeMs;
    public int? whiteIncrementMs;
    public int? blackTimeMs;
    public int? blackIncrementMs;

    public int? getTimeBudget(Side sideToMove, int? movesToGo = null) {
      int? time = blackTimeMs;
      int? increment = blackIncrementMs;
      if (sideToMove.Equals(Side.White)) {
        time = whiteTimeMs;
        increment = whiteIncrementMs;
      }
      if (time.HasValue)
      {
        int divisor = movesToGo ?? 20;
        return (time.Value / divisor) + ((increment ?? 0) / 2);
      }
      return null;
    }
    
  }
}
 
