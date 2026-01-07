namespace chessBot
{
  public class UCI
  {
    public static void Run()
    {
      while (true)
      {
        string? line = Console.ReadLine();
        if (line == null)
        {
          break;
        }

        string trimmed = line.Trim();
        if (trimmed.Length == 0) continue;

        string[] parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string command = parts[0];

        switch (command)
        {
          case "uci":
            Console.WriteLine("id name ScaldarEngine");
            Console.WriteLine("id author fadiamond");
            Console.WriteLine("uciok");
            break;
          case "isready":
            Console.WriteLine("readyok");
            break;
          case "ucinewgame":
            break;
          case "position":
            break;
          case "go":
            break;
          case "quit":
            return;
        }
      }
    }

  }
}
 
