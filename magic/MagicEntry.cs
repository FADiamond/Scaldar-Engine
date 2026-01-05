namespace chessBot.magic
{
  public readonly struct MagicEntry(ulong mask, ulong magicNumber, byte blockerSquaresOnRays)
  {
    public readonly ulong mask = mask;
    public readonly ulong magicNumber = magicNumber;
    public readonly byte blockerSquaresOnRays = blockerSquaresOnRays;
  }
}

