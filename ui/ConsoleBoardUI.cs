namespace chessBot.ui
{
  public class ConsoleBoardUI
  {

    public ConsoleBoardUI()
    {
    }

    public static void generateBoard(Board board)
    {
      char[] symbols = {
        '♟', '♞', '♝', '♜', '♛', '♚'
      };

      ConsoleColor whiteColor = ConsoleColor.White;
      ConsoleColor blackColor = ConsoleColor.DarkGreen;
      ConsoleColor defaultColor = Console.ForegroundColor;

      ulong bitboard = BitboardHelper.combineAllBitboards(board.bitboards);
      Console.WriteLine(Convert.ToString((long)bitboard, 2));
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
              pieceChar = symbols[i%6];
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

