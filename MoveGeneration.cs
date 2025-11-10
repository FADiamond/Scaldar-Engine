namespace chessBot
{
  public class MoveGeneration
  {
    private static readonly int[] whitePawnAttackOffset = { 7, 9 };
    private static readonly int[] blackPawnAttackOffset = { -7, -9 };
    private static readonly int[] horseAttackOffset = { 17, 15, 10, 6, -6, -10, -15, -17 };
    private static readonly int[] kingAttackOffset = { 1, -1, 8, -8, 9, -9, 7, -7 };
    }

  }
}
 
