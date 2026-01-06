using System.Collections.Generic;
using System.Reflection.Metadata;
using chessBot.ui;

namespace chessBot
{
  public class MoveGeneration
  {
    public static List<Move> moves = new();


    public static void generateMoves(Board board)
    {
      ulong whitePiecesBitboard = board.getWhitePiecesBitboard();
      ulong blackPiecesBitboard = board.getBlackPiecesBitboard();
      SideToMove side = board.sideToMove;

      Piece pawn = side.Equals(SideToMove.White) ? Piece.WhitePawn : Piece.BlackPawn;
      Piece knight = side.Equals(SideToMove.White) ? Piece.WhiteKnight : Piece.BlackKnight;
      Piece bishop = side.Equals(SideToMove.White) ? Piece.WhiteBishop : Piece.BlackBishop;
      Piece rook = side.Equals(SideToMove.White) ? Piece.WhiteRook : Piece.BlackRook;
      Piece queen = side.Equals(SideToMove.White) ? Piece.WhiteQueen : Piece.BlackQueen;
      Piece king = side.Equals(SideToMove.White) ? Piece.WhiteKing : Piece.BlackKing;

      generatePseudoLegalKnightMoves(board.bitboards[(byte)knight], whitePiecesBitboard, blackPiecesBitboard, side);
      generatePseudoLegalPawnMoves(board.bitboards[(byte)pawn], whitePiecesBitboard, blackPiecesBitboard, side, board.enPassantSquare);
      generatePseudoLegalKingMoves(board.bitboards[(byte)king], whitePiecesBitboard, blackPiecesBitboard, side);
      generatePseudoLegalRookMoves(board.bitboards[(byte)rook], whitePiecesBitboard, blackPiecesBitboard, side);
      generatePseudoLegalBishopMoves(board.bitboards[(byte)bishop], whitePiecesBitboard, blackPiecesBitboard, side);
      generatePseudoLegalQueenMoves(board.bitboards[(byte)queen], whitePiecesBitboard, blackPiecesBitboard, side);
    }

    public static void generatePseudoLegalKnightMoves(ulong knightsBitboard, ulong whitePiecesBitboard, ulong blackPiecesBitboard, SideToMove sideToMove)
    {
      ulong opposingPiecesBitboard = blackPiecesBitboard;
      ulong currentPiecesBitboard = whitePiecesBitboard;
      Piece currentSideKnight = Piece.WhiteKnight;
      if (sideToMove.Equals(SideToMove.Black))
      {
        opposingPiecesBitboard = whitePiecesBitboard;
        currentPiecesBitboard = blackPiecesBitboard;
        currentSideKnight = Piece.BlackKnight;
      }

      while (knightsBitboard != 0)
      {
        byte pieceIndex = knightsBitboard.popLSB();
        ulong knightAttack = Attacks.KnightAttacks[pieceIndex];
        
        ulong attackedSquaresBitboard = knightAttack & opposingPiecesBitboard;
        while (attackedSquaresBitboard != 0)
        {
          byte captureIndex = attackedSquaresBitboard.popLSB();
          moves.Add(new Move(pieceIndex, captureIndex, currentSideKnight, MoveFlags.Capture));
        }
        ulong regularMove = knightAttack & ~attackedSquaresBitboard & ~currentPiecesBitboard;
        while (regularMove != 0)
        {
          byte regularMoveIndex = regularMove.popLSB();
          moves.Add(new Move(pieceIndex, regularMoveIndex, currentSideKnight, MoveFlags.None));
        }
      }
    }

    public static void generatePseudoLegalPawnMoves(ulong pawnBitboard, ulong whitePiecesBitboard, ulong blackPiecesBitboard, SideToMove sideToMove, EnPassantSquare? enPassantSquare)
    {
      ulong opposingPiecesBitboard = blackPiecesBitboard;
      ulong currentPiecesBitboard = whitePiecesBitboard;
      ulong potentialPromotingPieces = Constants.Rank7 & pawnBitboard;
      ulong promotionRank = Constants.Rank8;
      Piece currentSidePawn = Piece.WhitePawn;
      if (sideToMove.Equals(SideToMove.Black))
      {
        opposingPiecesBitboard = whitePiecesBitboard;
        currentPiecesBitboard = blackPiecesBitboard;
        currentSidePawn = Piece.BlackPawn;
        potentialPromotingPieces = Constants.Rank2 & pawnBitboard;
        promotionRank = Constants.Rank1;
      }
      currentPiecesBitboard ^= potentialPromotingPieces;
      generatePseudoLegalPawnPromotions(potentialPromotingPieces, currentPiecesBitboard, opposingPiecesBitboard, currentSidePawn);


      ulong occupancyBitboard = whitePiecesBitboard | blackPiecesBitboard;
      while (pawnBitboard != 0)
      {
        byte currentPieceIndex = pawnBitboard.popLSB();
        ulong currentAttacks = Attacks.PawnAttacks[currentSidePawn][currentPieceIndex];

        if (enPassantSquare != null)
        {
          generatePseudoLegalPawnEnPassantCapture(currentPieceIndex, currentAttacks, currentSidePawn, enPassantSquare);
        }

        ulong attackedSquaresBitboard = currentAttacks & opposingPiecesBitboard;
        while (attackedSquaresBitboard != 0)
        {
          byte attackedSquareIndex = attackedSquaresBitboard.popLSB();
          moves.Add(new Move(currentPieceIndex, attackedSquareIndex, currentSidePawn, MoveFlags.Capture));
        }

        ulong regularMove = Attacks.PawnRegularMoves[currentSidePawn][currentPieceIndex] & ~occupancyBitboard;
        while (regularMove != 0)
        {
          byte movedToSquareIndex = regularMove.popLSB();
          moves.Add(new Move(currentPieceIndex, movedToSquareIndex, currentSidePawn, MoveFlags.None));
        }

        // Still have to filter out doublepush
        ulong doublePushMoves = Attacks.PawnDoublePush[currentSidePawn][currentPieceIndex] & ~occupancyBitboard;
        while (doublePushMoves != 0)
        {
          byte doublePushSquareIndex = doublePushMoves.popLSB();
          ulong doublePushSquareBitboard = BitboardHelper.getBitboardWithBitAt(doublePushSquareIndex);
          ulong shiftedSquareBitboard = 0UL;
          if (sideToMove.Equals(SideToMove.White))
          {
            shiftedSquareBitboard = BitboardHelper.South(doublePushSquareBitboard);
          }
          else if (sideToMove.Equals(SideToMove.Black))
          {
            shiftedSquareBitboard = BitboardHelper.North(doublePushSquareBitboard);
          }

          if ((shiftedSquareBitboard & occupancyBitboard) == 0)
          {
            moves.Add(new Move(currentPieceIndex, doublePushSquareIndex, currentSidePawn, MoveFlags.DoublePush));
          }
        }

      }

    }

    private static void generatePseudoLegalPawnEnPassantCapture(byte pieceIndex, ulong currentAttacksBitboard, Piece currentSidePawn, EnPassantSquare? enPassantSquare)
    {
      byte enPassantIndex = (byte)enPassantSquare.Value;
      ulong enPassantSquareBitboard = BitboardHelper.getBitboardWithBitAt(enPassantIndex);
      ulong reachedEnPassantSquareBitboard = enPassantSquareBitboard & currentAttacksBitboard;
      if (reachedEnPassantSquareBitboard != 0)
      {
        moves.Add(new Move(pieceIndex, enPassantIndex, currentSidePawn, MoveFlags.Capture | MoveFlags.EnPassant));
      }
    }

    private static void generatePseudoLegalPawnPromotions(ulong possiblePromotingPawnsBitboard, ulong currentPiecesBitboard, ulong opposingPiecesBitboard, Piece currentSidePawn)
    {
      while (possiblePromotingPawnsBitboard != 0)
      {
        byte pieceIndex = possiblePromotingPawnsBitboard.popLSB();

        ulong regularMovesPromotions = Attacks.PawnRegularMoves[currentSidePawn][pieceIndex] & ~(opposingPiecesBitboard | currentPiecesBitboard);
        while (regularMovesPromotions != 0)
        {
          byte targetPromotionIndex = regularMovesPromotions.popLSB();
          addPromotionMoves(pieceIndex, targetPromotionIndex, currentSidePawn);
        }

        ulong captureMovesPromotions = Attacks.PawnAttacks[currentSidePawn][pieceIndex] & opposingPiecesBitboard;
        while (captureMovesPromotions != 0)
        {
          byte targetPromotionIndex = captureMovesPromotions.popLSB();
          addPromotionMoves(pieceIndex, targetPromotionIndex, currentSidePawn, MoveFlags.Capture);
        }

      }
    }

    private static void addPromotionMoves(byte fromSquare, byte toSquare, Piece pawn, MoveFlags extraFlags = MoveFlags.None)
    {
      moves.Add(new Move(fromSquare, toSquare, pawn, extraFlags | MoveFlags.QueenPromotion));
      moves.Add(new Move(fromSquare, toSquare, pawn, extraFlags | MoveFlags.RookPromotion));
      moves.Add(new Move(fromSquare, toSquare, pawn, extraFlags | MoveFlags.KnightPromotion));
      moves.Add(new Move(fromSquare, toSquare, pawn, extraFlags | MoveFlags.BishopPromotion));
    }

    // TODO : Add castleing
    public static void generatePseudoLegalKingMoves(ulong kingBitboard, ulong whitePiecesBitboard, ulong blackPiecesBitboard, SideToMove sideToMove)
    {
      ulong opposingPiecesBitboard = blackPiecesBitboard;
      ulong currentPiecesBitboard = whitePiecesBitboard;
      Piece currentSideKing = Piece.WhiteKing;
      if (sideToMove.Equals(SideToMove.Black))
      {
        opposingPiecesBitboard = whitePiecesBitboard;
        currentPiecesBitboard = blackPiecesBitboard;
        currentSideKing = Piece.BlackKing;
      }

      byte pieceIndex = kingBitboard.popLSB();
      ulong kingAttacks = Attacks.KingAttacks[pieceIndex];

      kingAttacks = (kingAttacks & currentPiecesBitboard) ^ kingAttacks; // Filter out our pieces

      ulong kingCaptures = kingAttacks & opposingPiecesBitboard;

      // Captures
      while (kingCaptures != 0)
      {
        byte targetIndex = kingAttacks.popLSB();
        moves.Add(new Move(pieceIndex, targetIndex, currentSideKing, MoveFlags.Capture));
      }

      kingAttacks ^= kingCaptures;  // Fitler out Attacks

      // Regular Moves
      while (kingAttacks != 0)
      {
        byte targetIndex = kingAttacks.popLSB();
        moves.Add(new Move(pieceIndex, targetIndex, currentSideKing, MoveFlags.None));
      }

    }

    private static ulong getRookAttacks(ulong currentPiecesBitboard, ulong opposingPiecesBitboard, byte pieceIndex) {
        ulong occupancy = (opposingPiecesBitboard | currentPiecesBitboard) & Attacks.RookMask[pieceIndex];
        int magicIndex = (int)((occupancy * Constants.RookMagicNumbers[pieceIndex]) >> (64 - Constants.RookRelevantBits[pieceIndex]));

        return Attacks.RookAttacks[pieceIndex, magicIndex];
    }

    public static void generatePseudoLegalRookMoves(ulong rooksBitboard, ulong whitePiecesBitboard, ulong blackPiecesBitboard, SideToMove sideToMove)
    {
      ulong opposingPiecesBitboard = blackPiecesBitboard;
      ulong currentPiecesBitboard = whitePiecesBitboard;
      Piece currentSideRook = Piece.WhiteRook;
      if (sideToMove.Equals(SideToMove.Black))
      {
        opposingPiecesBitboard = whitePiecesBitboard;
        currentPiecesBitboard = blackPiecesBitboard;
        currentSideRook = Piece.BlackRook;
      }
      while (rooksBitboard != 0)
      {
        byte pieceIndex = rooksBitboard.popLSB();

        ulong attacks = getRookAttacks(currentPiecesBitboard, opposingPiecesBitboard, pieceIndex);

        addPseudoLegalMovesFromAttackTable(attacks, currentPiecesBitboard, opposingPiecesBitboard, pieceIndex, currentSideRook);
      }
    }

    private static ulong getBishopAttacks(ulong currentPiecesBitboard, ulong opposingPiecesBitboard, byte pieceIndex) {
      ulong occupancy = (opposingPiecesBitboard | currentPiecesBitboard) & Attacks.BishopMask[pieceIndex];
      int magicIndex = (int)((occupancy * Constants.BishopMagicNumbers[pieceIndex]) >> (64 - Constants.BishopRelevantBits[pieceIndex]));

      return Attacks.BishopAttacks[pieceIndex, magicIndex];
    }

    public static void generatePseudoLegalBishopMoves(ulong bishopsBitboard, ulong whitePiecesBitboard, ulong blackPiecesBitboard, SideToMove sideToMove)
    {
      ulong opposingPiecesBitboard = blackPiecesBitboard;
      ulong currentPiecesBitboard = whitePiecesBitboard;
      Piece currentSideBishop = Piece.WhiteBishop;
      if (sideToMove.Equals(SideToMove.Black))
      {
        opposingPiecesBitboard = whitePiecesBitboard;
        currentPiecesBitboard = blackPiecesBitboard;
        currentSideBishop = Piece.BlackBishop;
      }
      while (bishopsBitboard != 0)
      {
        byte pieceIndex = bishopsBitboard.popLSB();

        ulong attacks = getBishopAttacks(currentPiecesBitboard, opposingPiecesBitboard, pieceIndex);

        addPseudoLegalMovesFromAttackTable(attacks, currentPiecesBitboard, opposingPiecesBitboard, pieceIndex, currentSideBishop);
      }
    }

    public static void generatePseudoLegalQueenMoves(ulong queensBitboard, ulong whitePiecesBitboard, ulong blackPiecesBitboard, SideToMove sideToMove)
    {
      ulong opposingPiecesBitboard = blackPiecesBitboard;
      ulong currentPiecesBitboard = whitePiecesBitboard;
      Piece currentSidequeen = Piece.WhiteQueen;
      if (sideToMove.Equals(SideToMove.Black))
      {
        opposingPiecesBitboard = whitePiecesBitboard;
        currentPiecesBitboard = blackPiecesBitboard;
        currentSidequeen = Piece.BlackQueen;
      }
      while (queensBitboard != 0)
      {
        byte pieceIndex = queensBitboard.popLSB();

        ulong attacks = getBishopAttacks(currentPiecesBitboard, opposingPiecesBitboard, pieceIndex) | getRookAttacks(currentPiecesBitboard, opposingPiecesBitboard, pieceIndex);

        addPseudoLegalMovesFromAttackTable(attacks, currentPiecesBitboard, opposingPiecesBitboard, pieceIndex, currentSidequeen);
      }
    }


    public static void addPseudoLegalMovesFromAttackTable(ulong attacks, ulong currentPiecesBitboard, ulong opposingPiecesBitboard, byte pieceIndex, Piece piece) {
      attacks = (currentPiecesBitboard & attacks) ^ attacks;
      ulong captures = attacks & opposingPiecesBitboard;
      attacks ^= captures;

      while (captures != 0) {
        byte attackIndex = captures.popLSB();
        moves.Add(new Move(pieceIndex, attackIndex, piece, MoveFlags.Capture));
      }

      while (attacks != 0) {
        byte attackIndex = attacks.popLSB();
        moves.Add(new Move(pieceIndex, attackIndex, piece, MoveFlags.None));
      }

    }



  }

}
