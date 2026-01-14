using System.Diagnostics;
using chessBot.Test;

namespace chessBot
{
  public class Perft
  {

    public static PerftStatus PerftTest(Board board, int depth)
    {
      if (depth == 0) return new PerftStatus(1);

      PerftStatus perftStatus = new();

      List<Move> moves = MoveGeneration.generateMoves(board); ;
      foreach (Move move in moves)
      {
        Board nextBoard = board.copy();
        nextBoard.makeMove(move);
        if (!nextBoard.isInCheck())
        {
          perftStatus += PerftTest(nextBoard, depth - 1);
          if ((move.flags & MoveFlags.Capture) != 0)
          {
            perftStatus.captures++;
          }
          if ((move.flags & MoveFlags.EnPassant) != 0)
          {
            perftStatus.enPassant++;
          }
          if ((move.flags & (MoveFlags.CastleKingSide | MoveFlags.CastleQueenSide)) != 0)
          {
            perftStatus.castles++;
          }
          if ((move.flags & (MoveFlags.KnightPromotion | MoveFlags.BishopPromotion | MoveFlags.RookPromotion | MoveFlags.QueenPromotion)) != 0)
          {
            perftStatus.promotions++;
          }
        }
      }
      return perftStatus;
    }

    public static PerftStatus Divide(Board board, int depth)
    {
      if (depth == 0) return new PerftStatus(1);

      PerftStatus perftStatus = new();

      List<Move> moves = MoveGeneration.generateMoves(board); ;
      foreach (Move move in moves)
      {
        Board next = board.copy();
        next.makeMove(move);

        if (next.isInCheck()) continue;

        PerftStatus child = PerftTest(next, depth - 1);

        Console.WriteLine($"{(Square)move.fromSquare}{(Square)move.toSquare}: {child.nodes}");

        perftStatus += child;
      }
      return perftStatus;
    }


    public static void RunPerft(Board board, int maxDepth)
    {
      for (int depth = 1; depth <= maxDepth; depth++)
      {
        Stopwatch stopwatch = Stopwatch.StartNew();

        PerftStatus perftStatus = PerftTest(board, depth);

        stopwatch.Stop();

        double elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
        double nps = perftStatus.nodes / stopwatch.Elapsed.TotalSeconds;

        Console.WriteLine(
          $"Depth {depth}: {perftStatus.nodes:N0} nodes | " +
          $"{perftStatus.captures:N0} captures | " +
          $"{perftStatus.enPassant:N0} en passant | " +
          $"{perftStatus.castles:N0} castles | " +
          $"{perftStatus.promotions:N0} promotions | " +
          $"{elapsedMs:F2} ms | " +
          $"{nps:N0} NPS"
        );

      }

    }
  }
}
