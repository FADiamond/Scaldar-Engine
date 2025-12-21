using System.Numerics;

namespace chessBot
{
  public static class BitboardHelper
  {
    public static ulong getBitboardWithBitAt(byte position) {
      return 1UL << position;
    }

    public static void setBitAtPosition(this ref ulong bitboard, byte position)
    {
      bitboard |= 1UL << position;
    }

    public static void clearBitAtPosition(this ref ulong bitboard, byte position)
    {
      bitboard &= ~(1UL << position);
    }
    public static bool HasBit(ulong bitboard, int square)
    {
      return ((bitboard >> square) & 1UL) != 0;
    }

    public static byte popLSB(this ref ulong bitboard)
    {
      byte count = (byte)BitOperations.TrailingZeroCount(bitboard);
      bitboard &= bitboard - 1;

      return count;
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

    public static ulong North(ulong bitboard) => bitboard << 8;

    public static ulong South(ulong bitboard) => bitboard >> 8;

    public static ulong East(ulong bitboard) => (bitboard & ~Constants.FileH) << 1;

    public static ulong West(ulong bitboard) => (bitboard & ~Constants.FileA) >> 1;

    public static ulong NorthEast(ulong bitboard) => (bitboard & ~Constants.FileH) << 9;

    public static ulong NorthWest(ulong bitboard) => (bitboard & ~Constants.FileA) << 7;

    public static ulong SouthEast(ulong bitboard) => (bitboard & ~Constants.FileH) >> 7;

    public static ulong SouthWest(ulong bitboard) => (bitboard & ~Constants.FileA) >> 9;
  }
}

