using System.Collections.Generic;
using System.Reflection.Metadata;
using chessBot.ui;

namespace chessBot
{
  public class MoveGeneration
  {

    public static Dictionary<Piece, ulong[]> PawnAttacks = new()
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

    private static readonly int[] whitePawnAttackOffset = { 7, 9 };
    private static readonly int[] blackPawnAttackOffset = { -7, -9 };
    private static readonly int[] horseAttackOffset = { 17, 15, 10, 6, -6, -10, -15, -17 };
    private static readonly int[] kingAttackOffset = { 1, -1, 8, -8, 9, -9, 7, -7 };

    private static List<Move> moves = new();

    public static void Init()
    {
      initKnightAttacks();
    }

    private static void initKnightAttacks()
    {
      for (byte i = 0; i < KnightAttacks.Length; i++)
      {
        ulong currentPosition = BitboardHelper.getBitboardWithBitAt(i);
        ulong attacks = 0UL;
        for (byte j = 0; j < horseAttackOffset.Length; j++)
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

    private static void initPawnAttacks()
    {

    }

    public void generatePseudoLegalKnightMoves(ulong knightsBitboard, ulong whitePiecesBitboard, ulong blackPiecesBitboard, SideToMove sideToMove)
    {
      while (knightsBitboard != 0)
      {
        byte pieceIndex = knightsBitboard.popLSB();
        ulong knightAttack = KnightAttacks[pieceIndex];
        ulong opposingPiecesBitboard = blackPiecesBitboard;
        ulong currentPiecesBitboard = whitePiecesBitboard;
        Piece currentSideKnight = Piece.WhiteKnight;
        if (sideToMove.Equals(SideToMove.Black))
        {
          opposingPiecesBitboard = whitePiecesBitboard;
          currentPiecesBitboard = blackPiecesBitboard;
          currentSideKnight = Piece.BlackKnight;

        }
        ulong attackedSquaresBitboard = knightAttack & opposingPiecesBitboard;
        while (attackedSquaresBitboard != 0)
        {
          byte captureIndex = attackedSquaresBitboard.popLSB();
          moves.Add(new Move(pieceIndex, captureIndex, currentSideKnight, MoveFlags.Capture));
        }
        ulong regularMove = knightAttack & ~attackedSquaresBitboard & ~currentPiecesBitboard;
        while (attackedSquaresBitboard != 0)
        {
          byte regularMoveIndex = regularMove.popLSB();
          moves.Add(new Move(pieceIndex, regularMoveIndex, currentSideKnight, MoveFlags.None));
        }
      }
    }


  }

}

