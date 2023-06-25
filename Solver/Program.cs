using Game;
using Solver;

//string boardText = File.ReadAllText(@"Boards/exemple8_100_100_30.txt");
string boardText = BoardInputReader.Read();

RootBoard board = new RootBoard(boardText);

AStar aStar = new AStar(board);
State? solution = aStar.Solve();
if (solution == null)
{
    Console.WriteLine("No solution found!");
    return;
}

solution.PrintActions();