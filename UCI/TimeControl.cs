namespace ChessBot
{
  public class TimeControl
  {
    public int? whiteTimeMs;
    public int? whiteIncrementMs;
    public int? blackTimeMs;
    public int? blackIncrementMs;

    public int? getTimeBudget(Side sideToMove) {
      int? time = blackTimeMs;
      int? increment = blackIncrementMs;
      if (sideToMove.Equals(Side.White)) {
        time = whiteTimeMs;
        increment = whiteIncrementMs;
      }
      if (time.HasValue && increment.HasValue)
        return (time / 20) + (increment / 2);
      return null;
    }
    
  }
}
 
