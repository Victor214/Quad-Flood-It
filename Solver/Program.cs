using Game;
using Solver;

string boardText = File.ReadAllText(@"Boards/example3_100_100_30.txt");
Board board = new Board(boardText);

AStar aStar = new AStar(board);
State? solution = aStar.Solve();
if (solution == null)
{
    Console.WriteLine("No solution found!");
    return;
}

Console.WriteLine(String.Join(" ", solution.Actions.Select(x => $"{x.Pivot}{x.Color}")));