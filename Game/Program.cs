using Game;

string boardText = File.ReadAllText(@"Boards/example1_6_6_3.txt");
Board board = new Board(boardText);

string board2Text = File.ReadAllText(@"Boards/example2_30_30_10.txt");
Board board2 = new Board(board2Text);

Console.WriteLine("Test");