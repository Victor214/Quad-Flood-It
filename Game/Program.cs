using Game;

string boardText = File.ReadAllText(@"Boards/example1_6_6_3.txt");
RootBoard board = new RootBoard(boardText);

string board2Text = File.ReadAllText(@"Boards/example2_30_30_10.txt");
RootBoard board2 = new RootBoard(board2Text);

Console.WriteLine("Test");