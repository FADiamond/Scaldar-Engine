using ChessBot.ui;


namespace ChessBot
{

  // Bitboarrds format :
  // 56 57 58 59 60 61 62 63
  // 48 49 50 51 52 53 54 55
  // ...
  // 00 01 02 03 04 05 06 07
  public class Board
  {
    public ulong zobristKey;

    public Side sideToMove;
    public bool whiteCanCastleKingSide;
    public bool whiteCanCastleQueenSide;
    public bool blackCanCastleKingSide;
    public bool blackCanCastleQueenSide;
    public byte halfMoveClock;
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

    public Board() { }

    public Board(string startPos)
    {
      parseFen(startPos);
    }

    public Board copy()
    {
      Board board = new()
      {
        zobristKey = zobristKey,
        sideToMove = sideToMove,
        whiteCanCastleKingSide = whiteCanCastleKingSide,
        whiteCanCastleQueenSide = whiteCanCastleQueenSide,
        blackCanCastleKingSide = blackCanCastleKingSide,
        blackCanCastleQueenSide = blackCanCastleQueenSide,
        halfMoveClock = halfMoveClock,
        fullMoveClock = fullMoveClock,
        enPassantSquare = enPassantSquare,
        bitboards = (ulong[])bitboards.Clone()
      };
      return board;
    }

    public bool isAttacked(byte square, Side? side = null)
    {
      Side? previousSide = side;
      if (side == null)
      {
        // Check the previous side if not specified
        previousSide = sideToMove.Equals(Side.White) ? Side.Black : Side.White;
      }

      Piece pawn = Piece.WhitePawn;

      ulong pawnBoard = bitboards[(byte)Piece.BlackPawn];
      ulong knightBoard = bitboards[(byte)Piece.BlackKnight];
      ulong bishopBoard = bitboards[(byte)Piece.BlackBishop];
      ulong rookBoard = bitboards[(byte)Piece.BlackRook];
      ulong queenBoard = bitboards[(byte)Piece.BlackQueen];
      ulong kingBoard = bitboards[(byte)Piece.BlackKing];

      ulong currentPiecesBitboard = getWhitePiecesBitboard();
      ulong opponentPiecesBitboard = getBlackPiecesBitboard();

      if (previousSide.Equals(Side.Black))
      {
        pawn = Piece.BlackPawn;

        pawnBoard = bitboards[(byte)Piece.WhitePawn];
        knightBoard = bitboards[(byte)Piece.WhiteKnight];
        bishopBoard = bitboards[(byte)Piece.WhiteBishop];
        rookBoard = bitboards[(byte)Piece.WhiteRook];
        queenBoard = bitboards[(byte)Piece.WhiteQueen];
        kingBoard = bitboards[(byte)Piece.WhiteKing];

        currentPiecesBitboard = getBlackPiecesBitboard();
        opponentPiecesBitboard = getWhitePiecesBitboard();
      }

      ulong occupancy = currentPiecesBitboard | opponentPiecesBitboard;


      if ((Attacks.PawnAttacks[pawn][square] & pawnBoard) != 0) return true;
      if ((Attacks.KnightAttacks[square] & knightBoard) != 0) return true;
      if ((Attacks.getBishopAttacks(occupancy, square) & bishopBoard) != 0) return true;
      if ((Attacks.getRookAttacks(occupancy, square) & rookBoard) != 0) return true;
      if ((Attacks.getQueenAttacks(occupancy, square) & queenBoard) != 0) return true;
      if ((Attacks.KingAttacks[square] & kingBoard) != 0) return true;

      return false;
    }

    public bool isInCheck(Side? side = null)
    {
      Side? previousSide = side;
      if (side == null)
      {
        previousSide = sideToMove.Equals(Side.White) ? Side.Black : Side.White;
      }

      byte kingSquare = bitboards[(byte)Piece.WhiteKing].getLSBIndex();
      if (previousSide.Equals(Side.Black))
      {
        kingSquare = bitboards[(byte)Piece.BlackKing].getLSBIndex();
      }


      return isAttacked(kingSquare, side);
    }

    public bool makeMove(Move move)
    {

      byte whiteKingPosition = bitboards[(byte)Piece.WhiteKing].getLSBIndex();
      byte blackKingPosition = bitboards[(byte)Piece.BlackKing].getLSBIndex();

      // Prevent from eating king
      if (move.toSquare == whiteKingPosition || move.toSquare == blackKingPosition) return false;

      updateClocks(move);

      if (enPassantSquare != null) zobristKey ^= Zobrist.EnPassantSquare[((byte)enPassantSquare.Value) % 16];
      enPassantSquare = null;

      bool isPromotion = (move.flags & (MoveFlags.QueenPromotion | MoveFlags.RookPromotion | MoveFlags.BishopPromotion | MoveFlags.KnightPromotion)) != 0;

      if (isPromotion)
      {
        BitboardHelper.clearBitAtPosition(ref bitboards[(byte)move.piece], move.fromSquare);
        zobristKey ^= Zobrist.PieceSquare[(byte)move.piece, move.fromSquare];

        if ((move.flags & MoveFlags.Capture) != 0)
          removePieceFromSquare(move.toSquare);

        Piece promoted = GetPromotedPiece(move);
        bitboards[(byte)promoted] |= BitboardHelper.getBitboardWithBitAt(move.toSquare);
        zobristKey ^= Zobrist.PieceSquare[(byte)promoted, move.toSquare];

      }
      else
      {

        if ((move.flags & MoveFlags.EnPassant) != 0)
        {
          changePiecePosition(move.piece, move.fromSquare, move.toSquare);
          applyEnPassantMoveBitboardUpdate(move);
        }
        else
        {
          if ((move.flags & MoveFlags.Capture) != 0)
            removePieceFromSquare(move.toSquare);

          changePiecePosition(move.piece, move.fromSquare, move.toSquare);
          if ((move.flags & MoveFlags.DoublePush) != 0)
            updateEnPassantSquare(move);
        }
        applyCastleBitboardUpdate(move);
      }


      updateCastlingRights(move);
      zobristKey ^= Zobrist.BlackToMove;
      sideToMove = sideToMove.Equals(Side.White) ? Side.Black : Side.White;

      return true;
    }


    private void updateClocks(Move move)
    {
      halfMoveClock++;
      if ((move.flags & MoveFlags.Capture) != 0 || move.piece.Equals(Piece.WhitePawn) || move.piece.Equals(Piece.BlackPawn))
      {
        halfMoveClock = 0;
      }
      if (move.piece.IsBlack()) fullMoveClock++;
    }

    private Piece GetPromotedPiece(Move move)
    {
      bool white = move.piece.IsWhite();
      if ((move.flags & MoveFlags.QueenPromotion) != 0) return white ? Piece.WhiteQueen : Piece.BlackQueen;
      if ((move.flags & MoveFlags.RookPromotion) != 0) return white ? Piece.WhiteRook : Piece.BlackRook;
      if ((move.flags & MoveFlags.BishopPromotion) != 0) return white ? Piece.WhiteBishop : Piece.BlackBishop;
      return white ? Piece.WhiteKnight : Piece.BlackKnight;
    }

    private void updateEnPassantSquare(Move move)
    {
      if ((move.flags & MoveFlags.DoublePush) != 0)
      {
        if (move.piece.Equals(Piece.WhitePawn))
        {
          enPassantSquare = (EnPassantSquare)(move.toSquare - 8);
          zobristKey ^= Zobrist.EnPassantSquare[((byte)enPassantSquare.Value) % 16];
        }
        else
        {
          enPassantSquare = (EnPassantSquare)(move.toSquare + 8);
          zobristKey ^= Zobrist.EnPassantSquare[((byte)enPassantSquare.Value) % 16];
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
          zobristKey ^= Zobrist.PieceSquare[(byte)Piece.BlackPawn, (byte)(move.toSquare - 8)];
        }
        else
        {
          bitboards[(byte)Piece.WhitePawn] ^= BitboardHelper.getBitboardWithBitAt((byte)(move.toSquare + 8));
          zobristKey ^= Zobrist.PieceSquare[(byte)Piece.WhitePawn, (byte)(move.toSquare + 8)];
        }
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
        zobristKey ^= Zobrist.PieceSquare[(byte)rook, kingSideRookSquare];
        zobristKey ^= Zobrist.PieceSquare[(byte)rook, (byte)(move.toSquare - 1)];
      }
      else if ((move.flags & MoveFlags.CastleQueenSide) != 0)
      {
        rookMoveBitboard = BitboardHelper.getBitboardWithBitAt((byte)(move.toSquare + 1));
        rookMoveBitboard |= BitboardHelper.getBitboardWithBitAt(queenSideRookSquare);
        zobristKey ^= Zobrist.PieceSquare[(byte)rook, queenSideRookSquare];
        zobristKey ^= Zobrist.PieceSquare[(byte)rook, (byte)(move.toSquare + 1)];
      }

      bitboards[(byte)rook] ^= rookMoveBitboard;

    }

    private void changePiecePosition(Piece piece, byte fromSquare, byte toSquare)
    {
      bitboards[(byte)piece] ^= BitboardHelper.getBitboardWithBitAt(fromSquare);
      bitboards[(byte)piece] ^= BitboardHelper.getBitboardWithBitAt(toSquare);
      zobristKey ^= Zobrist.PieceSquare[(byte)piece, fromSquare];
      zobristKey ^= Zobrist.PieceSquare[(byte)piece, toSquare];
    }

    private void removePieceFromSquare(byte target)
    {
      for (byte i = 0; i < bitboards.Length; i++)
      {
        if (BitboardHelper.HasActiveBit(bitboards[i], target))
        {
          BitboardHelper.clearBitAtPosition(ref bitboards[i], target);
          zobristKey ^= Zobrist.PieceSquare[i, target];
        }
      }
    }

    private void updateCastlingRights(Move move)
    {
      if (move.piece.Equals(Piece.WhiteKing))
      {
        if (whiteCanCastleKingSide) zobristKey ^= Zobrist.CastlingRights[0];
        if (whiteCanCastleQueenSide) zobristKey ^= Zobrist.CastlingRights[1];
        whiteCanCastleKingSide = false;
        whiteCanCastleQueenSide = false;
      }
      else if (move.piece.Equals(Piece.BlackKing))
      {
        if (blackCanCastleKingSide) zobristKey ^= Zobrist.CastlingRights[2];
        if (blackCanCastleQueenSide) zobristKey ^= Zobrist.CastlingRights[3];
        blackCanCastleKingSide = false;
        blackCanCastleQueenSide = false;
      }

      if ((move.flags & MoveFlags.Capture) != 0)
      {
        if (move.toSquare.Equals(63))
        {
          if (blackCanCastleKingSide) zobristKey ^= Zobrist.CastlingRights[2];
          blackCanCastleKingSide = false;
        }
        else if (move.toSquare.Equals(56))
        {
          if (blackCanCastleQueenSide) zobristKey ^= Zobrist.CastlingRights[3];
          blackCanCastleQueenSide = false;
        }
        else if (move.toSquare.Equals(7))
        {
          if (whiteCanCastleKingSide) zobristKey ^= Zobrist.CastlingRights[0];
          whiteCanCastleKingSide = false;
        }
        else if (move.toSquare.Equals(0))
        {
          if (whiteCanCastleQueenSide) zobristKey ^= Zobrist.CastlingRights[1];
          whiteCanCastleQueenSide = false;
        }
      }

      if (move.piece.Equals(Piece.WhiteRook))
      {
        if (move.fromSquare.Equals(7))
        {
          if (whiteCanCastleKingSide) zobristKey ^= Zobrist.CastlingRights[0];
          whiteCanCastleKingSide = false;
        }
        else if (move.fromSquare.Equals(0))
        {
          if (whiteCanCastleQueenSide) zobristKey ^= Zobrist.CastlingRights[1];
          whiteCanCastleQueenSide = false;
        }
      }
      else if (move.piece.Equals(Piece.BlackRook))
      {
        if (move.fromSquare.Equals(63))
        {
          if (blackCanCastleKingSide) zobristKey ^= Zobrist.CastlingRights[2];
          blackCanCastleKingSide = false;
        }
        else if (move.fromSquare.Equals(56))
        {
          if (blackCanCastleQueenSide) zobristKey ^= Zobrist.CastlingRights[3];
          blackCanCastleQueenSide = false;
        }
      }
    }

    public Piece? getPieceAtSquare(byte square)
    {

      for (byte i = 0; i < bitboards.Length; i++)
      {
        if (BitboardHelper.HasActiveBit(bitboards[i], square))
        {
          return (Piece)i;
        }
      }
      return null;
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
      halfMoveClock = byte.Parse(halfmoveClock);
      fullMoveClock = short.Parse(fullmoveNumber);

      zobristKey = Zobrist.computeKey(this);
    }

    public void parseFenPiecePlacement(string piecePlacement)
    {
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

    public void AssertKingsPresent(string context = "")
    {
      ulong wk = bitboards[(byte)Piece.WhiteKing];
      ulong bk = bitboards[(byte)Piece.BlackKing];

      if (wk == 0UL || bk == 0UL)
      {
        ConsoleBoardUI.generateBoard(this);
        throw new InvalidOperationException(
            $"KING MISSING ({context}) | WK={wk:X16} BK={bk:X16} | sideToMove={sideToMove}"
        );
      }
    }


  }
}
