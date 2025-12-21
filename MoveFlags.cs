namespace chessBot
{
  // Flags added with : MoveFlags flags = MoveFlags.Capture | MoveFlags.QueenPromotion | ...
  // Check if flag is present with : flags & MoveFlags.Capture != 0;
  [Flags]
  public enum MoveFlags : ushort
  {
    None            = 0,        // 0000 0000 0000 0000
    Capture         = 1 << 0,   // 0000 0000 0000 0001
    EnPassant       = 1 << 1,   // 0000 0000 0000 0010
    DoublePush      = 1 << 2,   // 0000 0000 0000 0100
    CastleKingSide  = 1 << 3,   // 0000 0000 0000 1000
    CastleQueenSide = 1 << 4,   // 0000 0000 0001 0000
    KnightPromotion = 1 << 5,   // 0000 0000 0010 0000
    BishopPromotion = 1 << 6,   // 0000 0000 0100 0000
    RookPromotion   = 1 << 7,   // 0000 0000 1000 0000
    QueenPromotion  = 1 << 8    // 0000 0001 0000 0000
  }
}
 
