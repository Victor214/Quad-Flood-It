using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solver
{
    public class State
    {
        public Board RootBoard { get; set; }
        public List<Action> Actions { get; set; }
        public State? Parent { get; set; }
        public int PathCost { get; set; }
        public string? Pivot { get; set; }

        public State(Board board, State? parent = null)
        {
            RootBoard = board;
            Parent = parent;
            PathCost = Parent != null ? Parent.PathCost + 1 : 0;
            // Actions??
            Pivot = PickPivot();
        }

        public int GetHeuristic(Board? board = null, string? pivot = null)
        {
            if (board == null) // Make this not instantiate every time?
                board = ReconstructBoard();

            if (pivot == null)
                pivot = Pivot;
            
            // Maybe make heuristic as board total colors, and board max depth as tiebreaker
            return board.GetBoardMaxDepth(pivot!);
        }

        public int GetEvaluationFunction()
        {
            return PathCost + GetHeuristic();
        }

        public List<State> Expand()
        {
            Board board = ReconstructBoard();
            for (int i = 0; i < board.; i++)
            {
                var board = this.Clone();
                // Continue here
            }

            return Board.GetChildrenMoves()
                .Select(x => new State(x, this))
                .ToList();
        }

        public bool IsGoal()
        {
            foreach (var pivot in Board.Pivots)
            {
                if (pivot.Value.Neighbours.Count > 0)
                    return false;
            }

            return true;
        }

        #region Private Methods
        private string? PickPivot()
        {
            if (Parent != null)
                return Parent.Pivot;

            string? bestPivot = null;
            int bestPivotMax = int.MaxValue;

            Board board = ReconstructBoard();
            foreach (var pivot in board.Pivots)
            {
                // Get minimum distance among each pivot's max reach.
                // In other words, the best pivot is the one with the least flips to solution.
                int max = GetHeuristic(board, pivot.Key);
                if (max < bestPivotMax)
                {
                    bestPivotMax = max;
                    bestPivot = pivot.Key;
                }
            }

            return bestPivot;
        }

        private Board ReconstructBoard()
        {
            Board board = RootBoard.Clone();
            foreach (var action in Actions)
            {
                board.Pivots[action.Pivot!].Paint(action.Color);
            }

            return board;
        }
        #endregion
    }
}
