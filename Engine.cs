using chessBot.ui;

namespace chessBot
{
  public class Engine
  {
    public static readonly string startPos = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public static Board board;

    public static void Main(string[] args)
    {
      const string SICILIAN = "rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w KQkq c6 0 2";
      const string MIDDLE_GAME = "r1bq1rk1/ppp1bppp/2n1pn2/1B2N3/3P4/2N5/PPP2PPP/R1BQ1RK1 w - - 5 10";
      const string EN_PASSANT = "rnbqkbnr/pppp2pp/5p2/3Pp3/8/8/PPP1PPPP/RNBQKBNR w KQkq e6 0 3";
      const string RANDOM_GAME = "r1bqkb1r/pp1n1pp1/4pn2/2Pp2p1/1p1P4/2N1PN2/PP3PPP/R2Q1RK1 w kq - 0 10";
      const string BOTTOM_LEFT_L = "8/8/8/8/8/8/P7/PP6 w - - 0 1";
      const string KIWIPETE = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 5 10";

      Attacks.Init();
      string customPos = "r1bqkbnr/pppppppp/n7/8/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 1 2";
      // board = new Board(startPos);
      board = new Board(customPos);

      List<Move> moves = MoveGeneration.generateMoves(board);

      // ConsoleBoardUI.showBoardState(board, moves);
      Perft.RunPerft(board, 4);
      Perft.Divide(board, 2);
      
      // board = new Board(startPos);

    }
  }
}
