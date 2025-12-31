namespace chessBot.ui
{
  public class ConsoleBoardUI
  {
    public static readonly ConsoleColor whiteColor = ConsoleColor.White;
    public static readonly ConsoleColor blackColor = ConsoleColor.DarkGreen;
    public static readonly ConsoleColor defaultColor = Console.ForegroundColor;
    public static readonly char[] symbols = {
      '♟', '♞', '♝', '♜', '♛', '♚'
    };

    public ConsoleBoardUI() { }

    public static void generateSingleBitboard(ulong bitboard)
    {
      string binaryString = "";
      ulong temp = bitboard;
      for (int i = 63; i >= 0; i--)
      {
        binaryString += ((temp >> i) & 1UL) == 1 ? "1" : "0";
      }
      Console.WriteLine(binaryString);

      for (int rank = 7; rank >= 0; rank--)
      {
        Console.ForegroundColor = defaultColor;
        Console.Write(rank + 1);
        Console.Write(' ');
        for (int file = 0; file < 8; file++)
        {
          int square = rank * 8 + file;

          if (((bitboard >> square) & 1UL) != 0)
          {
            Console.ForegroundColor = blackColor;
            Console.Write('1');
          }
          else
          {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write('.');
          }

          Console.Write(' ');
        }
        Console.WriteLine();
      }

      Console.ForegroundColor = defaultColor;
      Console.WriteLine("  a b c d e f g h");
    }


    public static void generateBoard(Board board)
    {


      ulong bitboard = BitboardHelper.combineBitboards(board.bitboards);
      Console.WriteLine(BitboardHelper.NumberOfSetBits(bitboard));
      string binaryString = "";
      for (int i = 63; i >= 0; i--)
      {
        binaryString += ((bitboard >> i) & 1UL) == 1 ? "1" : "0";
      }
      Console.WriteLine(binaryString);
      for (int rank = 7; rank >= 0; rank--)
      {
        Console.ForegroundColor = defaultColor;
        Console.Write(rank + 1);
        Console.Write(' ');

        for (int file = 0; file < 8; file++)
        {
          int square = rank * 8 + file;
          char pieceChar = '.';
          bool isWhite = false;
          bool found = false;

          for (int i = 0; i < board.bitboards.Length; i++)
          {
            ulong bb = board.bitboards[i];
            if (((bb >> square) & 1UL) != 0)
            {
              pieceChar = symbols[i % 6];
              isWhite = i < 6; // 0–5 = white, 6–11 = black
              found = true;
              break;
            }
          }

          if (found)
          {
            Console.ForegroundColor = isWhite ? whiteColor : blackColor;
            Console.Write(pieceChar);
          }
          else
          {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write('.');
          }

          Console.Write(' ');
        }
        Console.WriteLine();
      }

      Console.ForegroundColor = defaultColor;
      Console.WriteLine("  a b c d e f g h");
    }
  }

}

