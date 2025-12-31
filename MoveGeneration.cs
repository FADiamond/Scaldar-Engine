using System.Collections.Generic;
using System.Reflection.Metadata;
using chessBot.ui;

namespace chessBot
{
  public class MoveGeneration
  {


    private static List<Move> moves = new();



    public void generatePseudoLegalKnightMoves(ulong knightsBitboard, ulong whitePiecesBitboard, ulong blackPiecesBitboard, SideToMove sideToMove)
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
        while (attackedSquaresBitboard != 0)
        {
          byte regularMoveIndex = regularMove.popLSB();
          moves.Add(new Move(pieceIndex, regularMoveIndex, currentSideKnight, MoveFlags.None));
        }
      }
    }

    public void generatePseudoLegalPawnMoves(ulong pawnBitboard, ulong whitePiecesBitboard, ulong blackPiecesBitboard, SideToMove sideToMove, EnPassantSquare enPassantSquare)
    {
      ulong opposingPiecesBitboard = blackPiecesBitboard;
      ulong currentPiecesBitboard = whitePiecesBitboard;
      ulong potentialPromotingPieces = Constants.Rank7 & currentPiecesBitboard;
      ulong promotionRank = Constants.Rank8;
      Piece currentSidePawn = Piece.WhitePawn;
      if (sideToMove.Equals(SideToMove.Black))
      {
        opposingPiecesBitboard = whitePiecesBitboard;
        currentPiecesBitboard = blackPiecesBitboard;
        currentSidePawn = Piece.BlackPawn;
        potentialPromotingPieces = Constants.Rank2 & currentPiecesBitboard;
        promotionRank = Constants.Rank1;
      }
      currentPiecesBitboard ^= potentialPromotingPieces;
      generatePseudoLegalPawnPromotions(potentialPromotingPieces, currentPiecesBitboard, opposingPiecesBitboard, currentSidePawn);


      ulong occupancyBitboard = whitePiecesBitboard | blackPiecesBitboard;
      while (pawnBitboard != 0)
      {
        byte currentPieceIndex = currentPiecesBitboard.popLSB();
        ulong currentAttacks = Attacks.PawnAttacks[currentSidePawn][currentPieceIndex];

        generatePseudoLegalPawnEnPassantCapture(currentPieceIndex, currentAttacks, currentSidePawn, enPassantSquare);

        ulong attackedSquaresBitboard = currentAttacks & opposingPiecesBitboard;
        while (attackedSquaresBitboard != 0) {
          byte attackedSquareIndex = attackedSquaresBitboard.popLSB();
          moves.Add(new Move(currentPieceIndex, attackedSquareIndex, currentSidePawn, MoveFlags.Capture));
        }

        ulong regularMove = Attacks.PawnRegularMoves[currentSidePawn][currentPieceIndex] & ~occupancyBitboard;
        while (regularMove != 0) {
          byte movedToSquareIndex = regularMove.popLSB();
          moves.Add(new Move(currentPieceIndex, movedToSquareIndex, currentSidePawn, MoveFlags.None));
        }

        // Still have to filter out doublepush
        ulong doublePushMoves = Attacks.PawnDoublePush[currentSidePawn][currentPieceIndex] & ~occupancyBitboard;
        while (doublePushMoves != 0) {
          byte doublePushSquareIndex = doublePushMoves.popLSB();
          ulong doublePushSquareBitboard = BitboardHelper.getBitboardWithBitAt(doublePushSquareIndex);
          ulong shiftedSquareBitboard = 0UL;
          if (sideToMove.Equals(SideToMove.White)) {
            shiftedSquareBitboard = BitboardHelper.South(doublePushSquareBitboard);
          }
          else if (sideToMove.Equals(SideToMove.Black)) {
            shiftedSquareBitboard = BitboardHelper.North(doublePushSquareBitboard);
          }

          if ((shiftedSquareBitboard & occupancyBitboard) != 0) {
            moves.Add(new Move(currentPieceIndex, doublePushSquareIndex, currentSidePawn, MoveFlags.DoublePush));
          }
        }

      }

    }

    private void generatePseudoLegalPawnEnPassantCapture(byte pieceIndex, ulong currentAttacksBitboard, Piece currentSidePawn, EnPassantSquare enPassantSquare) {
      byte enPassantIndex = (byte)enPassantSquare;
      ulong enPassantSquareBitboard = BitboardHelper.getBitboardWithBitAt(enPassantIndex);
      ulong reachedEnPassantSquareBitboard = enPassantSquareBitboard & currentAttacksBitboard;
      if (reachedEnPassantSquareBitboard != 0) {
        moves.Add(new Move(pieceIndex, enPassantIndex, currentSidePawn, MoveFlags.Capture | MoveFlags.EnPassant));
      }
    }
    
    private void generatePseudoLegalPawnPromotions(ulong possiblePromotingPawnsBitboard, ulong currentPiecesBitboard, ulong opposingPiecesBitboard, Piece currentSidePawn)
    {
      while (possiblePromotingPawnsBitboard != 0)
      {
        byte pieceIndex = possiblePromotingPawnsBitboard.popLSB();

        ulong regularMovesPromotions = Attacks.PawnRegularMoves[currentSidePawn][pieceIndex] & (~opposingPiecesBitboard | ~currentPiecesBitboard);
        while (regularMovesPromotions != 0)
        {
          byte targetPromotionIndex = regularMovesPromotions.popLSB();
          addPromotionMoves(pieceIndex, targetPromotionIndex, currentSidePawn);
        }

        ulong captureMovesPromotions = Attacks.PawnAttacks[currentSidePawn][pieceIndex] & (opposingPiecesBitboard | ~currentPiecesBitboard);
        while (captureMovesPromotions != 0)
        {
          byte targetPromotionIndex = captureMovesPromotions.popLSB();
          addPromotionMoves(pieceIndex, targetPromotionIndex, currentSidePawn, MoveFlags.Capture);
        }

      }
    }

    private void addPromotionMoves(byte fromSquare, byte toSquare, Piece pawn, MoveFlags extraFlags = MoveFlags.None)
    {
      moves.Add(new Move(fromSquare, toSquare, pawn, extraFlags | MoveFlags.QueenPromotion));
      moves.Add(new Move(fromSquare, toSquare, pawn, extraFlags | MoveFlags.RookPromotion));
      moves.Add(new Move(fromSquare, toSquare, pawn, extraFlags | MoveFlags.KnightPromotion));
      moves.Add(new Move(fromSquare, toSquare, pawn, extraFlags | MoveFlags.BishopPromotion));
    }




  }

}
