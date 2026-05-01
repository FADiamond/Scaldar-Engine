using chessBot;

namespace ChessBot
{
  public class UCI
  {
    private Board? board;
    private Search? search;

    public void Run()
    {
      CancellationTokenSource source = new();
      CancellationToken token = source.Token;
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
            NewGameReset();
            break;
          case "position":
            break;
          case "go":
            if (parts[1] == "perft")
            {
              processPerft();
            }
            break;
          case "quit":
            return;
        }
      }
    }

    private void processPerft()
    {

    }

    private void NewGameReset() {
      board = new Board(FenPositions.EnPassant);
      // TODO : Clear transposition table

    }

  }
}

