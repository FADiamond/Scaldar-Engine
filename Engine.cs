namespace ChessBot
{
  public class Engine
  {
    public static void Main(string[] args)
    {
      Attacks.Init();
      Zobrist.Init();

      UCI uci = new();
      uci.Run();
    }
  }
}
