namespace ChessBot
{
  public class UCI
  {
    private Board? board;
    private Search? search;
    private readonly Dictionary<ulong, int> repetitionCounts = [];

    public void Run()
    {
      search = new Search();
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
            PositionCommandOptions positionOptions = ParsePositionCommand(parts);
            handlePosition(positionOptions);
            break;
          case "go":
            GoCommandOptions goOptions = ParseGoCommand(parts);
            handleGoCommand(goOptions);
            break;
          case "quit":
            return;
        }
      }
    }


    private void NewGameReset()
    {
      board = new Board(FenPositions.StartPos);
      repetitionCounts.Clear();
      AddPositionToRepetitionCounts(board.zobristKey);
      search ??= new Search();
      search.ClearTT();

    }

    private void handleGoCommand(GoCommandOptions options)
    {
      board ??= new Board(FenPositions.StartPos);
      search ??= new Search();
      if (repetitionCounts.Count == 0)
        AddPositionToRepetitionCounts(board.zobristKey);

      if (options.perftDepth.HasValue)
      {
        ProcessPerft(options.perftDepth.Value);
        return;
      }

      int? timeBudgetMs = options.timeControl.getTimeBudget(board.sideToMove, options.movesToGo);

      CancellationTokenSource cts = new();
      CancellationToken timerToken = cts.Token;

      if (timeBudgetMs.HasValue)
        cts.CancelAfter((int)timeBudgetMs);

      Move move = search.findBestMove(board, options.depth, timerToken, repetitionCounts);
      Console.WriteLine("bestmove " + LongAlgebraicConverter.moveToAlgebraic(move));
    }

    private GoCommandOptions ParseGoCommand(string[] tokens)
    {
      if (tokens.Length == 0 || tokens[0] != "go")
        throw new ArgumentException("Expected a UCI go command.");

      GoCommandOptions options = new();

      int i = 1;

      while (i < tokens.Length)
      {
        switch (tokens[i])
        {
          case "perft":
            options.perftDepth = int.Parse(tokens[++i]);
            i++;
            break;
          case "searchmoves":
            i++;
            while (i < tokens.Length && !IsGoKeyword(tokens[i]))
            {
              options.moves.Add(tokens[i]);
              i++;
            }
            break;
          case "wtime":
            options.timeControl.whiteTimeMs = int.Parse(tokens[++i]);
            i++;
            break;
          case "btime":
            options.timeControl.blackTimeMs = int.Parse(tokens[++i]);
            i++;
            break;
          case "winc":
            options.timeControl.whiteIncrementMs = int.Parse(tokens[++i]);
            i++;
            break;
          case "binc":
            options.timeControl.blackIncrementMs = int.Parse(tokens[++i]);
            i++;
            break;
          case "movestogo":
            options.movesToGo = int.Parse(tokens[++i]);
            i++;
            break;
          case "depth":
            options.depth = int.Parse(tokens[++i]);
            i++;
            break;
          default:
            i++;
            break;
        }
      }

      return options;
    }

    private static bool IsGoKeyword(string token)
    {
      return token is
          "perft" or
          "searchmoves" or
          "wtime" or
          "btime" or
          "winc" or
          "binc" or
          "movestogo" or
          "depth";
    }

    private void handlePosition(PositionCommandOptions options)
    {
      string fen = options.fen ?? FenPositions.StartPos;
      board = new Board(fen);
      repetitionCounts.Clear();
      AddPositionToRepetitionCounts(board.zobristKey);
      makeAlgebraicMoves(options.moves);

    }

    private void makeAlgebraicMoves(List<string> algebraicMoves)
    {
      if (board == null)
        return;

      foreach (string alebraicMove in algebraicMoves)
      {
        Move? move = LongAlgebraicConverter.algebraicToMove(alebraicMove, board);
        if (move.HasValue)
        {
          bool moveWasMade = board.makeMove(move.Value);
          if (moveWasMade)
          {
            if (IsIrreversibleMove(move.Value))
              repetitionCounts.Clear();

            AddPositionToRepetitionCounts(board.zobristKey);
          }
        }
      }
    }

    private void AddPositionToRepetitionCounts(ulong zobristKey)
    {
      repetitionCounts.TryGetValue(zobristKey, out int count);
      repetitionCounts[zobristKey] = count + 1;
    }

    private static bool IsIrreversibleMove(Move move)
    {
      return (move.flags & MoveFlags.Capture) != 0 ||
          move.piece == Piece.WhitePawn ||
          move.piece == Piece.BlackPawn;
    }

        private PositionCommandOptions ParsePositionCommand(string[] tokens)
    {
      if (tokens.Length < 2 || tokens[0] != "position")
        throw new ArgumentException("Expected a UCI position command with at least 2 tokens.");

      PositionCommandOptions options = new();

      int i = 1;

      while (i < tokens.Length)
      {
        switch (tokens[i])
        {
          case "startpos":
            options.fen = FenPositions.StartPos;
            i++;
            break;
          case "fen":
            i++;
            int fenStart = i;
            while (i < tokens.Length && tokens[i] != "moves")
            {
              i++;
            }
            options.fen = string.Join(" ", tokens[fenStart..i]);
            break;
          case "moves":
            i++;
            while (i < tokens.Length && !IsPositionKeyword(tokens[i]))
            {
              options.moves.Add(tokens[i]);
              i++;
            }
            break;
          default:
            i++;
            break;
        }
      }

      return options;
    }

    private static bool IsPositionKeyword(string token)
    {
      return token is "startpos" or "fen" or "moves";
    }

    private void ProcessPerft(int depth)
    {
      board ??= new Board(FenPositions.StartPos);
      Perft.Run(board, depth);
    }

  }
}
