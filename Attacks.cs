namespace chessBot
{
  public class Attacks
  {
    public static Dictionary<Piece, ulong[]> PawnAttacks = new()
    {
      { Piece.WhitePawn, new ulong[64] },
      { Piece.BlackPawn, new ulong[64] }
    };
    public static Dictionary<Piece, ulong[]> PawnRegularMoves = new()
    {
      { Piece.WhitePawn, new ulong[64] },
      { Piece.BlackPawn, new ulong[64] }
    };
    public static Dictionary<Piece, ulong[]> PawnDoublePush = new()
    {
      { Piece.WhitePawn, new ulong[64] },
      { Piece.BlackPawn, new ulong[64] }
    };
    public static ulong[] KnightAttacks = new ulong[64];
    public static ulong[] KingAttacks = new ulong[64];
    public static ulong[] BishopMask = new ulong[64];
    public static ulong[] RookMask = new ulong[64];
    public static ulong[,] BishopAttacks = new ulong[64, 512];
    public static ulong[,] RookAttacks = new ulong[64, 4096];

    public static void Init()
    {
      initKnightAttacks();
      initPawnAttacks();
      initKingAttacks();
    }

    private static void initKnightAttacks()
    {
      const byte NBR_POSSIBLE_ATTACKS = 8; 
      for (byte i = 0; i < KnightAttacks.Length; i++)
      {
        ulong currentPosition = BitboardHelper.getBitboardWithBitAt(i);
        ulong attacks = 0UL;
        for (byte j = 0; j < NBR_POSSIBLE_ATTACKS; j++)
        {
          if (((currentPosition >> 17) & ~Constants.FileH) > 0) attacks |= currentPosition >> 17;
          if (((currentPosition >> 15) & ~Constants.FileA) > 0) attacks |= currentPosition >> 15;
          if (((currentPosition >> 10) & ~Constants.FileGH) > 0) attacks |= currentPosition >> 10;
          if (((currentPosition >> 6) & ~Constants.FileAB) > 0) attacks |= currentPosition >> 6;
          if (((currentPosition << 17) & ~Constants.FileA) > 0) attacks |= currentPosition << 17;
          if (((currentPosition << 15) & ~Constants.FileH) > 0) attacks |= currentPosition << 15;
          if (((currentPosition << 10) & ~Constants.FileAB) > 0) attacks |= currentPosition << 10;
          if (((currentPosition << 6) & ~Constants.FileGH) > 0) attacks |= currentPosition << 6;
        }
        KnightAttacks[i] = ~currentPosition & attacks;
      }
    }

    // Generated pawn attacks without promotions
    private static void initPawnAttacks()
    {
      const int POSSIBLE_POSITIONS = 64;
      for (byte i = 0; i < POSSIBLE_POSITIONS; i++)
      {
        ulong currentPosition = BitboardHelper.getBitboardWithBitAt(i);
        PawnAttacks[Piece.WhitePawn][i] = BitboardHelper.NorthWest(currentPosition) | BitboardHelper.NorthEast(currentPosition);
        PawnAttacks[Piece.BlackPawn][i] = BitboardHelper.SouthWest(currentPosition) | BitboardHelper.SouthEast(currentPosition);
        PawnRegularMoves[Piece.WhitePawn][i] = BitboardHelper.North(currentPosition);
        PawnRegularMoves[Piece.BlackPawn][i] = BitboardHelper.South(currentPosition);
        PawnDoublePush[Piece.WhitePawn][i] = BitboardHelper.North(Constants.Rank2 & currentPosition, 2);
        PawnDoublePush[Piece.BlackPawn][i] = BitboardHelper.South(Constants.Rank7 & currentPosition, 2);
      }

    }

    private static void initKingAttacks() {
      const byte NBR_POSSIBLE_ATTACKS = 8; 
      for (byte i = 0; i < KingAttacks.Length; i++) {
        ulong currentPosition = BitboardHelper.getBitboardWithBitAt(i);
        ulong attacks = 0UL;
        for (byte j = 0; j < NBR_POSSIBLE_ATTACKS; j++) {
          attacks |= BitboardHelper.North(currentPosition);  
          attacks |= BitboardHelper.South(currentPosition);
          attacks |= BitboardHelper.East(currentPosition);
          attacks |= BitboardHelper.West(currentPosition);
          attacks |= BitboardHelper.NorthEast(currentPosition);
          attacks |= BitboardHelper.NorthWest(currentPosition);
          attacks |= BitboardHelper.SouthEast(currentPosition);
          attacks |= BitboardHelper.SouthWest(currentPosition);
        }
        KingAttacks[i] = ~currentPosition & attacks;
      }
      

    }

  }
}
