using chessBot.ui;

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
      initRookAttacks();
      initBishopAttacks();

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

    private static void initKingAttacks()
    {
      const byte NBR_POSSIBLE_ATTACKS = 8;
      for (byte i = 0; i < KingAttacks.Length; i++)
      {
        ulong currentPosition = BitboardHelper.getBitboardWithBitAt(i);
        ulong attacks = 0UL;
        for (byte j = 0; j < NBR_POSSIBLE_ATTACKS; j++)
        {
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

    public static void initRookMask()
    {
      for (byte squareNbr = 0; squareNbr < 64; squareNbr++)
      {
        ulong attacks = 0UL;
        int rankNbr = squareNbr / 8;
        int fileNbr = squareNbr % 8;

        for (int rank = rankNbr + 1; rank <= 6; rank++)
          attacks |= BitboardHelper.coordToBitboard((byte)rank, (byte)fileNbr);

        for (int rank = rankNbr - 1; rank >= 1; rank--)
          attacks |= BitboardHelper.coordToBitboard((byte)rank, (byte)fileNbr);

        for (int file = fileNbr + 1; file <= 6; file++)
          attacks |= BitboardHelper.coordToBitboard((byte)rankNbr, (byte)file);

        for (int file = fileNbr - 1; file >= 1; file--)
          attacks |= BitboardHelper.coordToBitboard((byte)rankNbr, (byte)file);

        RookMask[squareNbr] = attacks;
      }
    }

    public static void initBishopMask()
    {
      for (byte squareNbr = 0; squareNbr < 64; squareNbr++)
      {
        ulong attacks = 0UL;
        int rankNbr = squareNbr / 8;
        int fileNbr = squareNbr % 8;

        for (int rank = rankNbr + 1, file = fileNbr + 1; rank <= 6 && file <= 6; rank++, file++)
          attacks |= BitboardHelper.coordToBitboard((byte)rank, (byte)file);

        for (int rank = rankNbr - 1, file = fileNbr + 1; rank >= 1 && file <= 6; rank--, file++)
          attacks |= BitboardHelper.coordToBitboard((byte)rank, (byte)file);

        for (int rank = rankNbr - 1, file = fileNbr - 1; rank >= 1 && file >= 1; rank--, file--)
          attacks |= BitboardHelper.coordToBitboard((byte)rank, (byte)file);

        for (int rank = rankNbr + 1, file = fileNbr - 1; rank <= 6 && file >= 1; rank++, file--)
          attacks |= BitboardHelper.coordToBitboard((byte)rank, (byte)file);

        BishopMask[squareNbr] = attacks;
      }
    }

    public static void initRookAttacks()
    {
      initRookMask();
      for (byte squareNbr = 0; squareNbr < 64; squareNbr++)
      {
        ulong mask = RookMask[squareNbr];
        int bitsInMask = Constants.RookRelevantBits[squareNbr];
        int occupanciesVariationAmounts = 1 << Constants.RookRelevantBits[squareNbr];

        for (int variationIndex = 0; variationIndex < occupanciesVariationAmounts; variationIndex++)
        {
          ulong occupancy = getMaskOccupancyBitboard(variationIndex, bitsInMask, mask);

          int magicIndex = (int)((occupancy * Constants.RookMagicNumbers[squareNbr]) >> (64 - Constants.RookRelevantBits[squareNbr]));

          RookAttacks[squareNbr, magicIndex] = generateRookPossibleMoves(squareNbr, occupancy);
        }
      }
    }

    public static void initBishopAttacks()
    {
      initBishopMask();
      for (byte squareNbr = 0; squareNbr < 64; squareNbr++)
      {
        ulong mask = BishopMask[squareNbr];
        int bitsInMask = Constants.BishopRelevantBits[squareNbr];
        int occupanciesVariationAmounts = 1 << Constants.BishopRelevantBits[squareNbr];

        for (int variationIndex = 0; variationIndex < occupanciesVariationAmounts; variationIndex++)
        {
          ulong occupancy = getMaskOccupancyBitboard(variationIndex, bitsInMask, mask);

          int magicIndex = (int)((occupancy * Constants.BishopMagicNumbers[squareNbr]) >> (64 - Constants.BishopRelevantBits[squareNbr]));

          BishopAttacks[squareNbr, magicIndex] = generateBishopPossibleMoves(squareNbr, occupancy);
        }

      }
    }

    public static ulong generateRookPossibleMoves(byte squareNbr, ulong occupancyMask)
    {
      ulong attacks = 0UL;
      int rankNbr = squareNbr / 8;
      int fileNbr = squareNbr % 8;

      for (int rank = rankNbr + 1; rank <= 7; rank++)
      {
        attacks |= BitboardHelper.coordToBitboard((byte)rank, (byte)fileNbr);
        if (BitboardHelper.HasActiveBit(occupancyMask, rank, fileNbr))
        {
          break;
        }
      }

      for (int rank = rankNbr - 1; rank >= 0; rank--)
      {
        attacks |= BitboardHelper.coordToBitboard((byte)rank, (byte)fileNbr);
        if (BitboardHelper.HasActiveBit(occupancyMask, rank, fileNbr))
        {
          break;
        }
      }

      for (int file = fileNbr + 1; file <= 7; file++)
      {
        attacks |= BitboardHelper.coordToBitboard((byte)rankNbr, (byte)file);
        if (BitboardHelper.HasActiveBit(occupancyMask, rankNbr, file))
        {
          break;
        }
      }

      for (int file = fileNbr - 1; file >= 0; file--)
      {
        attacks |= BitboardHelper.coordToBitboard((byte)rankNbr, (byte)file);
        if (BitboardHelper.HasActiveBit(occupancyMask, rankNbr, file))
        {
          break;
        }
      }

      return attacks;
    }

    public static ulong generateBishopPossibleMoves(byte squareNbr, ulong occupancyMask)
    {
      ulong attacks = 0UL;
      int rankNbr = squareNbr / 8;
      int fileNbr = squareNbr % 8;

      for (int rank = rankNbr + 1, file = fileNbr + 1; rank <= 7 && file <= 7; rank++, file++)
      {
        attacks |= BitboardHelper.coordToBitboard((byte)rank, (byte)file);

        if (BitboardHelper.HasActiveBit(occupancyMask, rank, file))
        {
          break;
        }
      }

      for (int rank = rankNbr - 1, file = fileNbr + 1; rank >= 0 && file <= 7; rank--, file++)
      {
        attacks |= BitboardHelper.coordToBitboard((byte)rank, (byte)file);
        if (BitboardHelper.HasActiveBit(occupancyMask, rank, file))
        {
          break;
        }
      }

      for (int rank = rankNbr - 1, file = fileNbr - 1; rank >= 0 && file >= 0; rank--, file--)
      {
        attacks |= BitboardHelper.coordToBitboard((byte)rank, (byte)file);
        if (BitboardHelper.HasActiveBit(occupancyMask, rank, file))
        {
          break;
        }
      }

      for (int rank = rankNbr + 1, file = fileNbr - 1; rank <= 7 && file >= 0; rank++, file--)
      {
        attacks |= BitboardHelper.coordToBitboard((byte)rank, (byte)file);
        if (BitboardHelper.HasActiveBit(occupancyMask, rank, file))
        {
          break;
        }
      }

      return attacks;

    }

    public static ulong getMaskOccupancyBitboard(int index, int maskBitsAmount, ulong mask)
    {
      ulong occupancies = 0UL;

      for (int count = 0; count < maskBitsAmount; count++)
      {
        byte squareIndex = mask.popLSB();

        if ((index & (1 << count)) > 0)
        {
          occupancies.setBitAtPosition(squareIndex);
        }
      }

      return occupancies;

    }



  }
}
