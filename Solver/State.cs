using Game;
using System.Collections.Concurrent;

namespace Solver
{
    public class State //: IEquatable<State>
    {
        public RootBoard RootBoard;
        public List<Action> Actions { get; set; }
        public State? Parent { get; set; }
        public int PathCost { get; set; }
        public string? Pivot { get; set; }

        private PartialBoard? _currentBoard;
        public PartialBoard CurrentBoard
        {
            get
            {
                if (_currentBoard == null)
                {
                    // Optimization: If parent's current board is loaded, take a shortcut, loading from it.
                    if (Parent?._currentBoard != null)
                    {
                        _currentBoard = Parent._currentBoard.CreatePartialBoard(Pivot!);

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

        public State(RootBoard rootBoard)
        {
            RootBoard = rootBoard;
            PathCost = 0;
            Actions = new List<Action>();
            Pivot = PickPivot();
        }

        public State(State source, Action action)
        {
            RootBoard = source.RootBoard;
            Parent = source;
            PathCost = source.PathCost + 1;
            Pivot = source.Pivot;
            Actions = source.Actions.ToList();
            Actions.Add(action);
        }

        public int GetHeuristic(string? pivot = null)
        {
            if (pivot == null)
                pivot = Pivot;

            // Maybe make heuristic as board total colors, and board max depth as tiebreaker
            int tilesRemaining = ((CurrentBoard.Width * CurrentBoard.Height) - CurrentBoard.Pivots[pivot!].Tiles);
            return CurrentBoard.GetRemainingColors().Count + tilesRemaining;
        }

        public int GetEvaluationFunction()
        {
            return PathCost + GetHeuristic();
        }

        //public List<State> Expand()
        //{
        //    List<State> states = new List<State>();
        //    var remainingColors = CurrentBoard.GetRemainingColors();
        //    var clones = CurrentBoard.BulkClone(remainingColors.Count);

        //    for (int i = 0; i < remainingColors.Count; i++)
        //    {
        //        var color = remainingColors[i];
        //        if (color == CurrentBoard.Pivots[Pivot!].Color) // Except current color.
        //            continue;


        //        Action action = new Action(this.Pivot, color);
        //        State state = new State(this, action);

        //        state._currentBoard = clones[i];
        //        state._currentBoard.Paint(state._currentBoard.Pivots[action.Pivot!], action.Color);

        //        states.Add(state);
        //    }

        //    return states;
        //}

        public List<State> Expand()
        {
            ConcurrentBag<State> states = new ConcurrentBag<State>();

            Parallel.ForEach(CurrentBoard.GetAdjacentColors(Pivot!), color =>
            {
                if (color == CurrentBoard.Pivots[Pivot!].Color) // Except current color.
                    return;

                Action action = new Action(Pivot, color);
                State state = new State(this, action);

                state._currentBoard = CurrentBoard.CreatePartialBoard(Pivot!);
                state._currentBoard.Paint(state._currentBoard.Pivots[action.Pivot!], action.Color);

                states.Add(state);
            });

            return states.ToList();
        }

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

        public void ClearBoard()
        {
            _currentBoard = null;
        }

        #region Equals / GetHashCode Overriding
        //public bool Equals(State? other)
        //{
        //    if (ReferenceEquals(other, null))
        //        return false;
        //    if (ReferenceEquals(this, other))
        //        return true;

        //    return CurrentBoard.Equals(other.CurrentBoard);
        //}

        //public override bool Equals(object? other) => Equals(other as State);

        //public static bool operator==(State? left, State? right)
        //{
        //    if (ReferenceEquals(left, right))
        //        return true;
        //    if (ReferenceEquals(left, null))
        //        return false;
        //    if (ReferenceEquals(right, null))
        //        return false;

        //    return left.Equals(right);
        //}

        //public static bool operator!=(State? left, State? right)
        //{
        //    return !(left == right);
        //}

        //public override int GetHashCode()
        //{
        //    unchecked
        //    {
        //        return CurrentBoard.GetHashCode();
        //    }
        //}
        #endregion

        #region Private Methods
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
