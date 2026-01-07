using System.Collections.Generic;
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
    public SideToMove sideToMove;
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


    public Board(string startPos)
    {
      parseFen(startPos);
      MoveGeneration.generateMoves(this);      
      List<Move> moves = MoveGeneration.moves;
      ConsoleBoardUI.showBoardState(this, moves);
    }

    public Board(Board board) {
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
      sideToMove = colorToMove.Equals("w") ? SideToMove.White : SideToMove.Black;
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
