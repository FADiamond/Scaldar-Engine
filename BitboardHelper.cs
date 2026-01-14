using System.Numerics;

namespace chessBot
{
  public static class BitboardHelper
  {
    public static ulong getBitboardWithBitAt(byte square)
    {
      return 1UL << square;
    }
    public static ulong getBitboardWithBitsAt(byte[] squares)
    {
      ulong result = 0UL;
      foreach (byte square in squares) {
        result |= 1UL << square;
      }
      return result;
    }

    public static ulong coordToBitboard(byte x, byte y)
    {
      return 1UL << (8 * x) + y;
    }

    public static void setBitAtPosition(this ref ulong bitboard, byte position)
    {
      bitboard |= 1UL << position;
    }

    public static void clearBitAtPosition(this ref ulong bitboard, byte position)
    {
      bitboard &= ~(1UL << position);
    }
    public static bool HasActiveBit(ulong bitboard, int square)
    {
      return ((bitboard >> square) & 1UL) != 0;
    }

    public static bool HasActiveBit(ulong bitboard, int rank, int file)
    {
      return ((bitboard >> (rank*8 + file)) & 1UL) != 0;
    }

    public static byte popLSB(this ref ulong bitboard)
    {
      byte count = (byte)BitOperations.TrailingZeroCount(bitboard);
      bitboard &= bitboard - 1;

      return count;
    }

    public static byte NumberOfSetBits(ulong bitboard)
    {
        bitboard = bitboard - ((bitboard >> 1) & 0x5555555555555555UL);
        bitboard = (bitboard & 0x3333333333333333UL) + ((bitboard >> 2) & 0x3333333333333333UL);
        return (byte)(unchecked(((bitboard + (bitboard >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
    }

    public static ulong combineBitboards(ulong[] bitboards)
    {
      ulong fullBitboard = 0;
      for (int i = 0; i < bitboards.Length; i++)
      {
        fullBitboard |= bitboards[i];
      }
      return fullBitboard;
    }

    public static ulong North(ulong bitboard, int times = 1) => bitboard << (8 * times);

    public static ulong South(ulong bitboard, int times = 1) => bitboard >> (8 * times);

    public static ulong East(ulong bitboard) => (bitboard & ~Constants.FileH) << 1;

    public static ulong West(ulong bitboard) => (bitboard & ~Constants.FileA) >> 1;

    public static ulong NorthEast(ulong bitboard) => (bitboard & ~Constants.FileH) << 9;

    public static ulong NorthWest(ulong bitboard) => (bitboard & ~Constants.FileA) << 7;

    public static ulong SouthEast(ulong bitboard) => (bitboard & ~Constants.FileH) >> 7;

    public static ulong SouthWest(ulong bitboard) => (bitboard & ~Constants.FileA) >> 9;
  }
}

