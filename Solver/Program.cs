using Game;
using Solver;

string boardText = File.ReadAllText(@"Boards/example3_100_100_30.txt");
RootBoard board = new RootBoard(boardText);

AStar aStar = new AStar(board);
State? solution = aStar.Solve();
if (solution == null)
{
    Console.WriteLine("No solution found!");
    return;
}

solution.PrintState();