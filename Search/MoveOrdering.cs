namespace ChessBot
{
  public sealed class MoveOrdering
  {

    public void orderMoves(Board board, List<Move> moves, Move? ttMove)
    {
      List<Move> ttMoves = [];
      List<Move> promotions = [];
      List<Move> captures = [];
      List<Move> quiets = [];

      foreach (Move move in moves)
      {
        if (ttMove.HasValue && move.Equals(ttMove.Value))
          ttMoves.Add(move);
        else if (IsPromotion(move))
          promotions.Add(move);
        else if ((move.flags & MoveFlags.Capture) != 0)
          captures.Add(move);
        else
          quiets.Add(move);
      }

      moves.Clear();
      moves.AddRange(ttMoves);
      moves.AddRange(promotions);
      moves.AddRange(captures);
      moves.AddRange(quiets);
    }

    private static bool IsPromotion(Move move)
    {
      return (move.flags & (
        MoveFlags.QueenPromotion |
        MoveFlags.RookPromotion |
        MoveFlags.BishopPromotion |
        MoveFlags.KnightPromotion
      )) != 0;
    }
  }

}
 
