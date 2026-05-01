namespace chessBot
{
  public static class Prng
  {
    private static ulong rngState = 0x2545F4914F6CDD1D;

    public static ulong Generate()
    {
      ulong x = rngState;
      x ^= x << 13;
      x ^= x >> 7;
      x ^= x << 17;
      rngState = x;
      return x;
    }
  }
}

