using Game;
using System.Collections.Concurrent;

namespace Solver
{
    public class State
    {
        public RootBoard RootBoard { get; set; } // Pointer to the initial-state board. This points to the same RootBoard for every state
        public List<Action> Actions { get; set; }
        public State? Parent { get; set; }
        public int PathCost { get; set; }
        public string? Pivot { get; set; }

        private PartialBoard? _currentBoard; // This recreates a board from root board and all the actions which have been made. A Partial Board is generated, which is composed only of the Painted island (node), and the adjacent islands (nodes) which are eligible to be painted in the next turn.
        public PartialBoard CurrentBoard
        {
            get
            {
                if (_currentBoard == null)
                {
                    // Optimization: If parent's current board is loaded, take a shortcut, loading from it.
                    if (Parent?._currentBoard != null)
                    {
                        _currentBoard = Parent._currentBoard.CreatePartialBoard();

                        Action lastAction = Actions.LastOrDefault()!;
                        _currentBoard.Paint(_currentBoard.Pivots[lastAction.Pivot!], lastAction.Color); // Only need to apply the latest action
                    }
                    else
                    {
                        _currentBoard = RootBoard.CreatePartialBoard(Pivot!);
                        foreach (var action in Actions)
                        {
                            _currentBoard.Paint(_currentBoard.Pivots[action.Pivot!], action.Color);
                        }
                    }
                }

                return _currentBoard;
            }
        }

        // Initial State Constructor
        public State(RootBoard rootBoard)
        {
            RootBoard = rootBoard;
            PathCost = 0;
            Actions = new List<Action>();
            Pivot = PickPivot();
        }

        // Child States Constructor
        public State(State source, Action action)
        {
            RootBoard = source.RootBoard;
            Parent = source;
            PathCost = source.PathCost + 1;
            Pivot = source.Pivot;
            Actions = source.Actions.ToList();
            Actions.Add(action);
        }

        public float GetHeuristic(string? pivot = null)
        {
            if (pivot == null)
                pivot = Pivot;

            // Maybe make heuristic as board total colors, and board max depth as tiebreaker
            float tilesRemaining = ((CurrentBoard.Width * CurrentBoard.Height) - CurrentBoard.Pivots[pivot!].Tiles);
            return CurrentBoard.TotalColors + tilesRemaining/3;
        }

        public float GetEvaluationFunction()
        {
            return PathCost + GetHeuristic();
        }

        public List<State> Expand()
        {
            ConcurrentBag<State> states = new ConcurrentBag<State>();

            foreach (var color in CurrentBoard.GetAdjacentColors())
            {
                if (color == CurrentBoard.Pivots[Pivot!].Color) // Except current color. (This check is likely never reached, there will never be an adjacent of same color)
                    continue;

                Action action = new Action(Pivot, color);
                State state = new State(this, action);

                state._currentBoard = CurrentBoard.CreatePartialBoard(Pivot!);
                state._currentBoard.Paint(state._currentBoard.Pivots[action.Pivot!], action.Color);

                states.Add(state);
            }

            return states.ToList();
        }

        // Checks if all pivots have been generated, and if none of them have neighbours (which is a cheap way to check if it is a goal state)
        public bool IsGoal()
        {
            if (CurrentBoard.Pivots.Count < 4)
                return false;

            foreach (var pivot in CurrentBoard.Pivots)
            {
                if (pivot.Value.Neighbours.Count > 0)
                    return false;
            }

            return true;
        }

        // Removes currentBoard from memory
        public void ClearBoard()
        {
            _currentBoard = null;
        }

        public void PrintActions()
        {
            Console.WriteLine($"{Actions.Count}");
            Console.WriteLine(String.Join(" ", Actions.Select(x => $"{x.Pivot} {x.Color}")) + "\n");
        }

        #region Private Methods
        // Selects which corner is going to be painted (from now until the solution is found) based on the corner with the least depth.
        private string? PickPivot()
        {
            string? bestPivot = null;
            int bestPivotDepth = int.MaxValue;

            foreach (var pivot in RootBoard.Pivots)
            {
                int depth = RootBoard.GetBoardMaxDepth(pivot.Key);
                if (depth < bestPivotDepth)
                {
                    bestPivotDepth = depth;
                    bestPivot = pivot.Key;
                }
            }

            return bestPivot;
        }
        #endregion
    }
}
