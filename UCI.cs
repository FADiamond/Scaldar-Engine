namespace chessBot
{
  public class UCI
  {

    public static readonly string startPos = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    private Board board = null;

    public void Run()
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
            board = new Board(startPos);
            break;
          case "position":
            break;
          case "go":
            if (parts[1] == "perft") {
              processPerft();
            }
            break;
          case "quit":
            return;
        }
      }
    }

    private void processPerft() {

    }

  }
}
 
