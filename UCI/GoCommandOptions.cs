namespace ChessBot
{
  public sealed class GoCommandOptions
  {
    public List<string> moves = [];

    public TimeControl timeControl = new();  

    public int? movesToGo;
    public int? depth;
    public int? perftDepth;
  }
}
 
