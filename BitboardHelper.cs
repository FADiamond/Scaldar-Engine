namespace chessBot
{
  public class BitboardHelper
  {
    
    public static void setBitAtPosition(ref ulong bitboard, short position) {
      bitboard |= (ulong)1 << position;
    }

    public static ulong combineAllBitboards(ulong[] bitboards) {
      ulong fullBitboard = 0;
      for (int i = 0; i < bitboards.Length; i++)
      {
        fullBitboard |= bitboards[i];
      }
      return fullBitboard;
    }
  }
}
 
