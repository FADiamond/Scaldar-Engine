using chessBot.Pieces;
using chessBot.ui;


namespace chessBot
{

  // Bitboarrds format :
  // 56 57 58 59 60 61 62 63
  // 48 49 50 51 52 53 54 55
  // ...
  // 00 01 02 03 04 05 06 07
  public class Board
  {
    public Side sideToMove;
    public bool whiteCanCastleKingSide;
    public bool whiteCanCastleQueenSide;
    public bool blackCanCastleKingSide;
    public bool blackCanCastleQueenSide;
    public byte halfwayMoveClock;
    public short fullMoveClock;
    public EnPassantSquare? enPassantSquare;
    public ulong[] bitboards = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
    public static readonly Dictionary<char, Piece> CharPieces = new()
    {
      { 'P', Piece.WhitePawn },
      { 'N', Piece.WhiteKnight },
      { 'B', Piece.WhiteBishop },
      { 'R', Piece.WhiteRook },
      { 'Q', Piece.WhiteQueen },
      { 'K', Piece.WhiteKing },
      { 'p', Piece.BlackPawn },
      { 'n', Piece.BlackKnight },
      { 'b', Piece.BlackBishop },
      { 'r', Piece.BlackRook },
      { 'q', Piece.BlackQueen },
      { 'k', Piece.BlackKing }
    };


    public Board(string startPos)
    {
      parseFen(startPos);
    }

    public Board(Board board)
    {
      sideToMove = board.sideToMove;
      whiteCanCastleKingSide = board.whiteCanCastleKingSide;
      whiteCanCastleQueenSide = board.whiteCanCastleQueenSide;
      blackCanCastleKingSide = board.blackCanCastleKingSide;
      blackCanCastleQueenSide = board.blackCanCastleQueenSide;
      halfwayMoveClock = board.halfwayMoveClock;
      fullMoveClock = board.fullMoveClock;
      enPassantSquare = board.enPassantSquare;
      bitboards = board.bitboards;
    }

    public bool isInCheckAfterMove() {
      Side previousSide = sideToMove.Equals(Side.White) ? Side.Black : Side.White;
      Piece pawn = Piece.BlackPawn;
      Piece currentKing = Piece.WhiteKing;

      ulong pawnBoard = bitboards[(byte)Piece.BlackPawn];
      ulong knightBoard = bitboards[(byte)Piece.BlackKnight];
      ulong bishopBoard = bitboards[(byte)Piece.BlackBishop];
      ulong rookBoard = bitboards[(byte)Piece.BlackRook];
      ulong queenBoard = bitboards[(byte)Piece.BlackQueen];
      ulong kingBoard = bitboards[(byte)Piece.BlackKing];

      ulong currentPiecesBitboard = getWhitePiecesBitboard();
      ulong opponentPiecesBitboard = getBlackPiecesBitboard();
      if (previousSide.Equals(Side.Black)) {
        pawn = Piece.WhitePawn;
        currentKing = Piece.BlackKing;

        pawnBoard = bitboards[(byte)Piece.WhitePawn];
        knightBoard = bitboards[(byte)Piece.WhiteKnight];
        bishopBoard = bitboards[(byte)Piece.WhiteBishop];
        rookBoard = bitboards[(byte)Piece.WhiteRook];
        queenBoard = bitboards[(byte)Piece.WhiteQueen];
        kingBoard = bitboards[(byte)Piece.WhiteKing];

        currentPiecesBitboard = getBlackPiecesBitboard();
        opponentPiecesBitboard = getWhitePiecesBitboard();
      }

      ulong attacksBitboard = 0UL;

      while (pawnBoard != 0) {
        byte index = pawnBoard.popLSB();
        attacksBitboard |= Attacks.PawnAttacks[pawn][index];
      }

      while (knightBoard != 0) {
        byte index = knightBoard.popLSB();
        attacksBitboard |= Attacks.KnightAttacks[index];
      }

      while (bishopBoard != 0) {
        byte index = bishopBoard.popLSB();
        attacksBitboard |= Attacks.getBishopAttacks(currentPiecesBitboard, opponentPiecesBitboard, index);
      }

      while (rookBoard != 0) {
        byte index = rookBoard.popLSB();
        attacksBitboard |= Attacks.getRookAttacks(currentPiecesBitboard, opponentPiecesBitboard, index);
      }

      while (queenBoard != 0) {
        byte index = queenBoard.popLSB();
        attacksBitboard |= Attacks.getBishopAttacks(currentPiecesBitboard, opponentPiecesBitboard, index) | Attacks.getRookAttacks(currentPiecesBitboard, opponentPiecesBitboard, index);
      }

      while (kingBoard != 0) {
        byte index = kingBoard.popLSB();
        attacksBitboard |= Attacks.KingAttacks[index];
      }

      return (attacksBitboard & bitboards[(byte)currentKing]) != 0;
    }

    public void makeMove(Move move)
    {
      updateClocks(move);
      changePiecePosition(move.piece, move.fromSquare, move.toSquare);

      if (move.piece.Equals(Piece.WhitePawn) || move.piece.Equals(Piece.BlackPawn))
      {
        updateEnPassantSquare(move);
        applyEnPassantMoveBitboardUpdate(move);
      }
      else if ((move.flags & MoveFlags.Capture) != 0)
      {
        removePieceFromSquare(move.toSquare);
      }
      updateCastlingRights(move);
      applyPromotionBitboardUpdate(move);
      applyCastleBitboardUpdate(move);

      sideToMove = sideToMove.Equals(Side.White) ? Side.Black : Side.White;
    }


    private void updateClocks(Move move)
    {
      halfwayMoveClock++;
      if ((move.flags & MoveFlags.Capture) != 0 || move.piece.Equals(Piece.WhitePawn) || move.piece.Equals(Piece.BlackPawn))
      {
        halfwayMoveClock = 0;
      }
      if (move.piece.IsBlack()) fullMoveClock++;
    }

    private void updateEnPassantSquare(Move move)
    {
      enPassantSquare = null;
      if ((move.flags & MoveFlags.DoublePush) != 0)
      {
        if (move.piece.Equals(Piece.WhitePawn))
        {
          enPassantSquare = (EnPassantSquare)(move.toSquare - 8);
        }
        else
        {
          enPassantSquare = (EnPassantSquare)(move.toSquare + 8);
        }
      }
    }

    private void applyEnPassantMoveBitboardUpdate(Move move)
    {
      if ((move.flags & MoveFlags.EnPassant) != 0)
      {
        if (move.piece.Equals(Piece.WhitePawn))
        {
          bitboards[(byte)Piece.BlackPawn] ^= BitboardHelper.getBitboardWithBitAt((byte)(move.toSquare - 8));
        }
        else
        {
          bitboards[(byte)Piece.WhitePawn] ^= BitboardHelper.getBitboardWithBitAt((byte)(move.toSquare + 8));
        }
      }
    }

    private void applyPromotionBitboardUpdate(Move move)
    {
      if ((move.flags & MoveFlags.QueenPromotion) != 0)
      {
        removePieceFromSquare(move.fromSquare);
        removePieceFromSquare(move.toSquare);

        Piece promotedPiece = move.piece.IsWhite() ? Piece.WhiteQueen : Piece.BlackQueen;
        bitboards[(byte)promotedPiece] ^= BitboardHelper.getBitboardWithBitAt(move.toSquare);
      }
      else if ((move.flags & MoveFlags.RookPromotion) != 0)
      {
        removePieceFromSquare(move.fromSquare);
        removePieceFromSquare(move.toSquare);

        Piece promotedPiece = move.piece.IsWhite() ? Piece.WhiteRook : Piece.BlackRook;
        bitboards[(byte)promotedPiece] ^= BitboardHelper.getBitboardWithBitAt(move.toSquare);
      }
      else if ((move.flags & MoveFlags.KnightPromotion) != 0)
      {
        removePieceFromSquare(move.fromSquare);
        removePieceFromSquare(move.toSquare);

        Piece promotedPiece = move.piece.IsWhite() ? Piece.WhiteKnight : Piece.BlackKnight;
        bitboards[(byte)promotedPiece] ^= BitboardHelper.getBitboardWithBitAt(move.toSquare);
      }
      else if ((move.flags & MoveFlags.BishopPromotion) != 0)
      {
        removePieceFromSquare(move.fromSquare);
        removePieceFromSquare(move.toSquare);

        Piece promotedPiece = move.piece.IsWhite() ? Piece.WhiteBishop : Piece.BlackBishop;
        bitboards[(byte)promotedPiece] ^= BitboardHelper.getBitboardWithBitAt(move.toSquare);
      }
    }

    private void applyCastleBitboardUpdate(Move move)
    {
      byte queenSideRookSquare = 0;
      byte kingSideRookSquare = 7;
      Piece rook = Piece.WhiteRook;
      if (move.piece.IsBlack())
      {
        queenSideRookSquare = 56;
        kingSideRookSquare = 63;
        rook = Piece.BlackRook;
      }

      ulong rookMoveBitboard = 0UL;

      if ((move.flags & MoveFlags.CastleKingSide) != 0)
      {
        rookMoveBitboard = BitboardHelper.getBitboardWithBitAt((byte)(move.toSquare - 1));
        rookMoveBitboard |= BitboardHelper.getBitboardWithBitAt(kingSideRookSquare);
      }
      else if ((move.flags & MoveFlags.CastleQueenSide) != 0)
      {
        rookMoveBitboard = BitboardHelper.getBitboardWithBitAt((byte)(move.toSquare - 1));
        rookMoveBitboard |= BitboardHelper.getBitboardWithBitAt(queenSideRookSquare);
      }

      bitboards[(byte)rook] ^= rookMoveBitboard;

    }

    private void changePiecePosition(Piece piece, byte fromSquare, byte toSquare)
    {
      bitboards[(byte)piece] ^= BitboardHelper.getBitboardWithBitAt(fromSquare) & BitboardHelper.getBitboardWithBitAt(toSquare);
    }

    private void removePieceFromSquare(byte target)
    {
      for (byte i = 0; i < bitboards.Length; i++)
      {
        if (BitboardHelper.HasActiveBit(bitboards[i], target))
        {
          BitboardHelper.clearBitAtPosition(ref bitboards[i], target);
        }
      }
    }

    private void updateCastlingRights(Move move)
    {
      if (move.piece.Equals(Piece.WhiteKing))
      {
        whiteCanCastleKingSide = false;
        whiteCanCastleQueenSide = false;
      }
      else if (move.piece.Equals(Piece.BlackKing))
      {
        blackCanCastleKingSide = false;
        blackCanCastleQueenSide = false;
      }

      if (move.piece.Equals(Piece.WhiteRook))
      {
        if (move.fromSquare.Equals(7)) whiteCanCastleKingSide = false;
        else if (move.fromSquare.Equals(0)) whiteCanCastleQueenSide = false;
      }
      else if (move.piece.Equals(Piece.BlackRook))
      {
        if (move.fromSquare.Equals(63)) blackCanCastleKingSide = false;
        else if (move.fromSquare.Equals(56)) blackCanCastleQueenSide = false;
      }
    }

    public ulong getWhitePiecesBitboard()
    {
      return bitboards[(byte)CharPieces['P']] | bitboards[(byte)CharPieces['N']] | bitboards[(byte)CharPieces['B']] | bitboards[(byte)CharPieces['R']] | bitboards[(byte)CharPieces['Q']] | bitboards[(byte)CharPieces['K']];
    }

    public ulong getBlackPiecesBitboard()
    {
      return bitboards[(byte)CharPieces['p']] | bitboards[(byte)CharPieces['n']] | bitboards[(byte)CharPieces['b']] | bitboards[(byte)CharPieces['r']] | bitboards[(byte)CharPieces['q']] | bitboards[(byte)CharPieces['k']];
    }

    public ulong getOccupancyBitboard()
    {
      return getBlackPiecesBitboard() | getWhitePiecesBitboard();
    }


    public void parseFen(string fen)
    {
      string[] fields = fen.Split();
      string piecePlacement = fields[0];
      string colorToMove = fields[1];
      string castlingAvailability = fields[2];
      string enPassantTargetSquare = fields[3];
      string halfmoveClock = fields[4];
      string fullmoveNumber = fields[5];

      parseFenPiecePlacement(piecePlacement);
      sideToMove = colorToMove.Equals("w") ? Side.White : Side.Black;
      whiteCanCastleKingSide = castlingAvailability.Contains('K');
      whiteCanCastleQueenSide = castlingAvailability.Contains('Q');
      blackCanCastleKingSide = castlingAvailability.Contains('k');
      blackCanCastleQueenSide = castlingAvailability.Contains('q');
      enPassantSquare = Enum.TryParse<EnPassantSquare>(enPassantTargetSquare, out var result) ? result : null;
      halfwayMoveClock = byte.Parse(halfmoveClock);
      fullMoveClock = short.Parse(fullmoveNumber);

    }

    public void parseFenPiecePlacement(string piecePlacement)
    {
      Console.WriteLine(piecePlacement);
      byte squareNbr = 0;
      string[] rows = piecePlacement.Split('/');
      for (int i = rows.Length - 1; i >= 0; i--)
      {
        string rowPieces = rows[i];
        for (int j = 0; j < rowPieces.Length; j++)
        {
          char squareContent = rowPieces[j];
          if (squareNbr >= 64) break;
          if (char.IsNumber(squareContent))
          {
            squareNbr += (byte)char.GetNumericValue(squareContent);
          }
          else if (CharPieces.TryGetValue(squareContent, out Piece piece))
          {
            BitboardHelper.setBitAtPosition(ref bitboards[(int)piece], squareNbr);
            squareNbr++;
          }
        }
      }
    }


  }
}
