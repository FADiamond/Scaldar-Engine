using System.Collections.Generic;
using System.Reflection.Metadata;
using chessBot.ui;

namespace chessBot
{
  public class MoveGeneration
  {
    public static List<Move> generateMoves(Board board)
    {
      List<Move> moves = [];
      ulong whitePiecesBitboard = board.getWhitePiecesBitboard();
      ulong blackPiecesBitboard = board.getBlackPiecesBitboard();
      Side side = board.sideToMove;

      bool canCastleKingSide = side.Equals(Side.White) ? board.whiteCanCastleKingSide : board.blackCanCastleKingSide;
      bool canCastleQueenSide = side.Equals(Side.White) ? board.whiteCanCastleQueenSide : board.blackCanCastleQueenSide;

      Piece pawn = side.Equals(Side.White) ? Piece.WhitePawn : Piece.BlackPawn;
      Piece knight = side.Equals(Side.White) ? Piece.WhiteKnight : Piece.BlackKnight;
      Piece bishop = side.Equals(Side.White) ? Piece.WhiteBishop : Piece.BlackBishop;
      Piece rook = side.Equals(Side.White) ? Piece.WhiteRook : Piece.BlackRook;
      Piece queen = side.Equals(Side.White) ? Piece.WhiteQueen : Piece.BlackQueen;
      Piece king = side.Equals(Side.White) ? Piece.WhiteKing : Piece.BlackKing;

      Piece opposingKing = side.Equals(Side.White) ? Piece.BlackKing : Piece.WhiteKing;
      ulong opposingKingBitboard = board.bitboards[(byte)opposingKing];

      moves.AddRange(generatePseudoLegalKnightMoves(board.bitboards[(byte)knight], whitePiecesBitboard, blackPiecesBitboard, side, opposingKingBitboard));
      moves.AddRange(generatePseudoLegalPawnMoves(board.bitboards[(byte)pawn], whitePiecesBitboard, blackPiecesBitboard, side, board.enPassantSquare, opposingKingBitboard));
      moves.AddRange(generatePseudoLegalKingMoves(board.bitboards[(byte)king], whitePiecesBitboard, blackPiecesBitboard, side, canCastleKingSide, canCastleQueenSide, opposingKingBitboard, board));
      moves.AddRange(generatePseudoLegalRookMoves(board.bitboards[(byte)rook], whitePiecesBitboard, blackPiecesBitboard, side, opposingKingBitboard));
      moves.AddRange(generatePseudoLegalBishopMoves(board.bitboards[(byte)bishop], whitePiecesBitboard, blackPiecesBitboard, side, opposingKingBitboard));
      moves.AddRange(generatePseudoLegalQueenMoves(board.bitboards[(byte)queen], whitePiecesBitboard, blackPiecesBitboard, side, opposingKingBitboard));
      return moves;
    }

    public static List<Move> generatePseudoLegalKnightMoves(ulong knightsBitboard, ulong whitePiecesBitboard, ulong blackPiecesBitboard, Side sideToMove, ulong opposingKingBitboard)
    {
      List<Move> moves = [];
      ulong opposingPiecesBitboard = blackPiecesBitboard;
      ulong currentPiecesBitboard = whitePiecesBitboard;
      Piece currentSideKnight = Piece.WhiteKnight;
      if (sideToMove.Equals(Side.Black))
      {
        opposingPiecesBitboard = whitePiecesBitboard;
        currentPiecesBitboard = blackPiecesBitboard;
        currentSideKnight = Piece.BlackKnight;
      }

      while (knightsBitboard != 0)
      {
        byte pieceIndex = knightsBitboard.popLSB();
        ulong knightAttack = Attacks.KnightAttacks[pieceIndex];

        ulong attackedSquaresBitboard = knightAttack & opposingPiecesBitboard & ~opposingKingBitboard;
        while (attackedSquaresBitboard != 0)
        {
          byte captureIndex = attackedSquaresBitboard.popLSB();
          moves.Add(new Move(pieceIndex, captureIndex, currentSideKnight, MoveFlags.Capture));
        }
        ulong regularMove = knightAttack & ~(currentPiecesBitboard | opposingPiecesBitboard);
        while (regularMove != 0)
        {
          byte regularMoveIndex = regularMove.popLSB();
          moves.Add(new Move(pieceIndex, regularMoveIndex, currentSideKnight, MoveFlags.None));
        }
      }
      return moves;
    }

    public static List<Move> generatePseudoLegalPawnMoves(ulong pawnBitboard, ulong whitePiecesBitboard, ulong blackPiecesBitboard, Side sideToMove, EnPassantSquare? enPassantSquare, ulong opposingKingBitboard)
    {
      List<Move> moves = [];
      ulong opposingPiecesBitboard = blackPiecesBitboard;
      ulong currentPiecesBitboard = whitePiecesBitboard;
      ulong potentialPromotingPieces = Constants.Rank7 & pawnBitboard;
      Piece currentSidePawn = Piece.WhitePawn;
      if (sideToMove.Equals(Side.Black))
      {
        opposingPiecesBitboard = whitePiecesBitboard;
        currentPiecesBitboard = blackPiecesBitboard;
        currentSidePawn = Piece.BlackPawn;
        potentialPromotingPieces = Constants.Rank2 & pawnBitboard;
      }
      currentPiecesBitboard ^= potentialPromotingPieces;
      moves.AddRange(generatePseudoLegalPawnPromotions(potentialPromotingPieces, currentPiecesBitboard, opposingPiecesBitboard, currentSidePawn, opposingKingBitboard));


      ulong occupancyBitboard = whitePiecesBitboard | blackPiecesBitboard;
      while (pawnBitboard != 0)
      {
        byte currentPieceIndex = pawnBitboard.popLSB();
        ulong currentAttacks = Attacks.PawnAttacks[currentSidePawn][currentPieceIndex];

        if (enPassantSquare != null)
        {
          moves.AddRange(generatePseudoLegalPawnEnPassantCapture(currentPieceIndex, currentAttacks, currentSidePawn, enPassantSquare, opposingKingBitboard));
        }

        ulong attackedSquaresBitboard = currentAttacks & opposingPiecesBitboard & ~opposingKingBitboard;
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
          if (sideToMove.Equals(Side.White))
          {
            shiftedSquareBitboard = BitboardHelper.South(doublePushSquareBitboard);
          }
          else if (sideToMove.Equals(Side.Black))
          {
            shiftedSquareBitboard = BitboardHelper.North(doublePushSquareBitboard);
          }

          if ((shiftedSquareBitboard & occupancyBitboard) == 0)
          {
            moves.Add(new Move(currentPieceIndex, doublePushSquareIndex, currentSidePawn, MoveFlags.DoublePush));
          }
        }

      }

      return moves;
    }

    private static List<Move> generatePseudoLegalPawnEnPassantCapture(byte pieceIndex, ulong currentAttacksBitboard, Piece currentSidePawn, EnPassantSquare? enPassantSquare, ulong opposingKingBitboard)
    {
      List<Move> moves = [];
      byte enPassantIndex = (byte)enPassantSquare.Value;
      ulong enPassantSquareBitboard = BitboardHelper.getBitboardWithBitAt(enPassantIndex);
      ulong reachedEnPassantSquareBitboard = enPassantSquareBitboard & currentAttacksBitboard;
      if (reachedEnPassantSquareBitboard != 0)
      {
        moves.Add(new Move(pieceIndex, enPassantIndex, currentSidePawn, MoveFlags.Capture | MoveFlags.EnPassant));
      }
      return moves;
    }

    private static List<Move> generatePseudoLegalPawnPromotions(ulong possiblePromotingPawnsBitboard, ulong currentPiecesBitboard, ulong opposingPiecesBitboard, Piece currentSidePawn, ulong opposingKingBitboard)
    {
      List<Move> moves = [];
      while (possiblePromotingPawnsBitboard != 0)
      {
        byte pieceIndex = possiblePromotingPawnsBitboard.popLSB();

        ulong regularMovesPromotions = Attacks.PawnRegularMoves[currentSidePawn][pieceIndex] & ~(opposingPiecesBitboard | currentPiecesBitboard);
        while (regularMovesPromotions != 0)
        {
          byte targetPromotionIndex = regularMovesPromotions.popLSB();
          moves.AddRange(addPromotionMoves(pieceIndex, targetPromotionIndex, currentSidePawn));
        }

        ulong captureMovesPromotions = Attacks.PawnAttacks[currentSidePawn][pieceIndex] & opposingPiecesBitboard & ~opposingKingBitboard;
        while (captureMovesPromotions != 0)
        {
          byte targetPromotionIndex = captureMovesPromotions.popLSB();
          moves.AddRange(addPromotionMoves(pieceIndex, targetPromotionIndex, currentSidePawn, MoveFlags.Capture));
        }

      }
      return moves;
    }

    private static List<Move> addPromotionMoves(byte fromSquare, byte toSquare, Piece pawn, MoveFlags extraFlags = MoveFlags.None)
    {
      List<Move> moves = [];
      moves.Add(new Move(fromSquare, toSquare, pawn, extraFlags | MoveFlags.QueenPromotion));
      moves.Add(new Move(fromSquare, toSquare, pawn, extraFlags | MoveFlags.RookPromotion));
      moves.Add(new Move(fromSquare, toSquare, pawn, extraFlags | MoveFlags.KnightPromotion));
      moves.Add(new Move(fromSquare, toSquare, pawn, extraFlags | MoveFlags.BishopPromotion));
      return moves;
    }

    public static List<Move> generatePseudoLegalKingMoves(ulong kingBitboard, ulong whitePiecesBitboard, ulong blackPiecesBitboard, Side sideToMove, bool canCastleKingSide, bool canCastleQueenSide, ulong opposingKingBitboard, Board board)
    {
      if (kingBitboard == 0UL)
        throw new InvalidOperationException($"No king for sideToMove={sideToMove}.");
      List<Move> moves = [];
      ulong opposingPiecesBitboard = blackPiecesBitboard;
      ulong currentPiecesBitboard = whitePiecesBitboard;
      Piece currentSideKing = Piece.WhiteKing;
      if (sideToMove.Equals(Side.Black))
      {
        opposingPiecesBitboard = whitePiecesBitboard;
        currentPiecesBitboard = blackPiecesBitboard;
        currentSideKing = Piece.BlackKing;
      }

      byte pieceIndex = kingBitboard.popLSB();
      ulong kingAttacks = Attacks.KingAttacks[pieceIndex];


      kingAttacks &= ~currentPiecesBitboard;

      ulong kingCaptures = kingAttacks & opposingPiecesBitboard & ~opposingKingBitboard;

      // Captures
      while (kingCaptures != 0)
      {
        byte targetIndex = kingCaptures.popLSB();
        moves.Add(new Move(pieceIndex, targetIndex, currentSideKing, MoveFlags.Capture));
      }

      kingAttacks ^= kingCaptures;  // Fitler out Attacks

      // Regular Moves
      while (kingAttacks != 0)
      {
        byte targetIndex = kingAttacks.popLSB();
        moves.Add(new Move(pieceIndex, targetIndex, currentSideKing, MoveFlags.None));
      }
      ulong occupancyBitboard = currentPiecesBitboard | opposingPiecesBitboard;

      moves.AddRange(generatePseudoLegalKingCastle(kingBitboard, canCastleKingSide, canCastleQueenSide, sideToMove, pieceIndex, occupancyBitboard, board));

      return moves;
    }

    private static List<Move> generatePseudoLegalKingCastle(ulong kingBitboard, bool canCastleKingSide, bool canCastleQueenSide, Side sideToMove, byte kingPosition, ulong occupancyBitboard, Board board)
    {
      List<Move> moves = [];
      Piece king = Piece.WhiteKing;
      byte[] queenSideRaySquares = [1, 2, 3];
      byte[] kingSideRaySquares = [5, 6];
      if (sideToMove.Equals(Side.Black))
      {
        king = Piece.BlackKing;
        queenSideRaySquares = [57, 58, 59];
        kingSideRaySquares = [61, 62];
      }

      ulong queenSideOccupancy = BitboardHelper.getBitboardWithBitsAt(queenSideRaySquares) | kingBitboard;
      ulong kingSideOccupancy = BitboardHelper.getBitboardWithBitsAt(kingSideRaySquares) | kingBitboard;

      bool isQueenSideIncheck = board.isInCheck(sideToMove);
      foreach (byte square in queenSideRaySquares) {
        isQueenSideIncheck = isQueenSideIncheck && board.isInCheck(sideToMove, square);
      }

      bool isKingSideInCheck = board.isInCheck(sideToMove);
      foreach (byte square in kingSideRaySquares) {
        isKingSideInCheck = isKingSideInCheck && board.isInCheck(sideToMove, square);
      }
      
      if (canCastleQueenSide && (occupancyBitboard & queenSideOccupancy) == 0 && !isQueenSideIncheck)
      {
        moves.Add(new Move(kingPosition, (byte)(kingPosition - 2), king, MoveFlags.CastleQueenSide));
      }
      if (canCastleKingSide && (occupancyBitboard & kingSideOccupancy) == 0 && !isKingSideInCheck)
      {
        moves.Add(new Move(kingPosition, (byte)(kingPosition + 2), king, MoveFlags.CastleKingSide));
      }
      return moves;
    }

    public static List<Move> generatePseudoLegalRookMoves(ulong rooksBitboard, ulong whitePiecesBitboard, ulong blackPiecesBitboard, Side sideToMove, ulong opposingKingBitboard)
    {
      List<Move> moves = [];
      ulong opposingPiecesBitboard = blackPiecesBitboard;
      ulong currentPiecesBitboard = whitePiecesBitboard;
      Piece currentSideRook = Piece.WhiteRook;
      if (sideToMove.Equals(Side.Black))
      {
        opposingPiecesBitboard = whitePiecesBitboard;
        currentPiecesBitboard = blackPiecesBitboard;
        currentSideRook = Piece.BlackRook;
      }
      while (rooksBitboard != 0)
      {
        byte pieceIndex = rooksBitboard.popLSB();

        ulong attacks = Attacks.getRookAttacks(currentPiecesBitboard, opposingPiecesBitboard, pieceIndex);

        moves.AddRange(addPseudoLegalMovesFromAttackTable(attacks, currentPiecesBitboard, opposingPiecesBitboard, pieceIndex, currentSideRook, opposingKingBitboard));
      }
      return moves;
    }

    public static List<Move> generatePseudoLegalBishopMoves(ulong bishopsBitboard, ulong whitePiecesBitboard, ulong blackPiecesBitboard, Side sideToMove, ulong opposingKingBitboard)
    {
      List<Move> moves = [];
      ulong opposingPiecesBitboard = blackPiecesBitboard;
      ulong currentPiecesBitboard = whitePiecesBitboard;
      Piece currentSideBishop = Piece.WhiteBishop;
      if (sideToMove.Equals(Side.Black))
      {
        opposingPiecesBitboard = whitePiecesBitboard;
        currentPiecesBitboard = blackPiecesBitboard;
        currentSideBishop = Piece.BlackBishop;
      }
      while (bishopsBitboard != 0)
      {
        byte pieceIndex = bishopsBitboard.popLSB();

        ulong attacks = Attacks.getBishopAttacks(currentPiecesBitboard, opposingPiecesBitboard, pieceIndex);

        moves.AddRange(addPseudoLegalMovesFromAttackTable(attacks, currentPiecesBitboard, opposingPiecesBitboard, pieceIndex, currentSideBishop, opposingKingBitboard));
      }
      return moves;
    }

    public static List<Move> generatePseudoLegalQueenMoves(ulong queensBitboard, ulong whitePiecesBitboard, ulong blackPiecesBitboard, Side sideToMove, ulong opposingKingBitboard)
    {
      List<Move> moves = [];
      ulong opposingPiecesBitboard = blackPiecesBitboard;
      ulong currentPiecesBitboard = whitePiecesBitboard;
      Piece currentSidequeen = Piece.WhiteQueen;
      if (sideToMove.Equals(Side.Black))
      {
        opposingPiecesBitboard = whitePiecesBitboard;
        currentPiecesBitboard = blackPiecesBitboard;
        currentSidequeen = Piece.BlackQueen;
      }
      while (queensBitboard != 0)
      {
        byte pieceIndex = queensBitboard.popLSB();

        ulong attacks = Attacks.getBishopAttacks(currentPiecesBitboard, opposingPiecesBitboard, pieceIndex) | Attacks.getRookAttacks(currentPiecesBitboard, opposingPiecesBitboard, pieceIndex);

        moves.AddRange(addPseudoLegalMovesFromAttackTable(attacks, currentPiecesBitboard, opposingPiecesBitboard, pieceIndex, currentSidequeen, opposingKingBitboard));
      }
      return moves;
    }


    public static List<Move> addPseudoLegalMovesFromAttackTable(ulong attacks, ulong currentPiecesBitboard, ulong opposingPiecesBitboard, byte pieceIndex, Piece piece, ulong opposingKingBitboard)
    {
      List<Move> moves = [];
      attacks = (currentPiecesBitboard & attacks) ^ attacks;
      ulong captures = attacks & opposingPiecesBitboard & ~opposingKingBitboard;
      attacks ^= captures;

      while (captures != 0)
      {
        byte attackIndex = captures.popLSB();
        moves.Add(new Move(pieceIndex, attackIndex, piece, MoveFlags.Capture));
      }

      while (attacks != 0)
      {
        byte attackIndex = attacks.popLSB();
        moves.Add(new Move(pieceIndex, attackIndex, piece, MoveFlags.None));
      }

      return moves;
    }
  }

}
