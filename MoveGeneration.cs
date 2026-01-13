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

      moves.AddRange(generatePseudoLegalKnightMoves(board.bitboards[(byte)knight], whitePiecesBitboard, blackPiecesBitboard, side));
      moves.AddRange(generatePseudoLegalPawnMoves(board.bitboards[(byte)pawn], whitePiecesBitboard, blackPiecesBitboard, side, board.enPassantSquare));
      moves.AddRange(generatePseudoLegalKingMoves(board.bitboards[(byte)king], whitePiecesBitboard, blackPiecesBitboard, side, canCastleKingSide, canCastleQueenSide));
      moves.AddRange(generatePseudoLegalRookMoves(board.bitboards[(byte)rook], whitePiecesBitboard, blackPiecesBitboard, side));
      moves.AddRange(generatePseudoLegalBishopMoves(board.bitboards[(byte)bishop], whitePiecesBitboard, blackPiecesBitboard, side));
      moves.AddRange(generatePseudoLegalQueenMoves(board.bitboards[(byte)queen], whitePiecesBitboard, blackPiecesBitboard, side));
      return moves;
    }

    public static List<Move> generatePseudoLegalKnightMoves(ulong knightsBitboard, ulong whitePiecesBitboard, ulong blackPiecesBitboard, Side sideToMove)
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

        ulong attackedSquaresBitboard = knightAttack & opposingPiecesBitboard;
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

    public static List<Move> generatePseudoLegalPawnMoves(ulong pawnBitboard, ulong whitePiecesBitboard, ulong blackPiecesBitboard, Side sideToMove, EnPassantSquare? enPassantSquare)
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

    private static List<Move> generatePseudoLegalPawnEnPassantCapture(byte pieceIndex, ulong currentAttacksBitboard, Piece currentSidePawn, EnPassantSquare? enPassantSquare)
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

    private static List<Move> generatePseudoLegalPawnPromotions(ulong possiblePromotingPawnsBitboard, ulong currentPiecesBitboard, ulong opposingPiecesBitboard, Piece currentSidePawn)
    {
      List<Move> moves = [];
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

    public static List<Move> generatePseudoLegalKingMoves(ulong kingBitboard, ulong whitePiecesBitboard, ulong blackPiecesBitboard, Side sideToMove, bool canCastleKingSide, bool canCastleQueenSide)
    {
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

      generatePseudoLegalWhiteKingCastle(canCastleKingSide, canCastleQueenSide, sideToMove);

      return moves;
    }

    private static List<Move> generatePseudoLegalWhiteKingCastle(bool canCastleKingSide, bool canCastleQueenSide, Side sideToMove)
    {
      List<Move> moves = [];
      if (sideToMove.Equals(Side.White))
      {
        if (canCastleQueenSide) moves.Add(new Move(4, 2, Piece.WhiteKing, MoveFlags.CastleQueenSide));
        if (canCastleKingSide) moves.Add(new Move(4, 6, Piece.WhiteKing, MoveFlags.CastleKingSide));
      }
      else
      {
        if (canCastleQueenSide) moves.Add(new Move(4, 58, Piece.BlackKing, MoveFlags.CastleQueenSide));
        if (canCastleKingSide) moves.Add(new Move(4, 62, Piece.BlackKing, MoveFlags.CastleKingSide));
      }
      return moves;
    }

    public static List<Move> generatePseudoLegalRookMoves(ulong rooksBitboard, ulong whitePiecesBitboard, ulong blackPiecesBitboard, Side sideToMove)
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

        addPseudoLegalMovesFromAttackTable(attacks, currentPiecesBitboard, opposingPiecesBitboard, pieceIndex, currentSideRook);
      }
      return moves;
    }

    public static List<Move> generatePseudoLegalBishopMoves(ulong bishopsBitboard, ulong whitePiecesBitboard, ulong blackPiecesBitboard, Side sideToMove)
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

        addPseudoLegalMovesFromAttackTable(attacks, currentPiecesBitboard, opposingPiecesBitboard, pieceIndex, currentSideBishop);
      }
      return moves;
    }

    public static List<Move> generatePseudoLegalQueenMoves(ulong queensBitboard, ulong whitePiecesBitboard, ulong blackPiecesBitboard, Side sideToMove)
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

        addPseudoLegalMovesFromAttackTable(attacks, currentPiecesBitboard, opposingPiecesBitboard, pieceIndex, currentSidequeen);
      }
      return moves;
    }


    public static List<Move> addPseudoLegalMovesFromAttackTable(ulong attacks, ulong currentPiecesBitboard, ulong opposingPiecesBitboard, byte pieceIndex, Piece piece)
    {
      List<Move> moves = [];
      attacks = (currentPiecesBitboard & attacks) ^ attacks;
      ulong captures = attacks & opposingPiecesBitboard;
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
