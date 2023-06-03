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

        private Board? _currentBoard;
        public Board CurrentBoard
        {
            get
            {
                if (_currentBoard == null)
                    _currentBoard = ReconstructBoard();

                return _currentBoard;
            }
        }

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
            if (pivot == null)
                pivot = Pivot;
            
            // Maybe make heuristic as board total colors, and board max depth as tiebreaker
            return CurrentBoard.GetBoardMaxDepth(pivot!);
        }

        public int GetEvaluationFunction()
        {
            return PathCost + GetHeuristic();
        }

        public List<State> Expand()
        {
            for (int i = 0; i < CurrentBoard; i++)
            {
                // Continue here
            }
        }

        public bool IsGoal()
        {
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

        #region Private Methods
        private string? PickPivot()
        {
            if (Parent != null)
                return Parent.Pivot;

            string? bestPivot = null;
            int bestPivotMax = int.MaxValue;

            foreach (var pivot in CurrentBoard.Pivots)
            {
                // Get minimum distance among each pivot's max reach.
                // In other words, the best pivot is the one with the least flips to solution.
                int max = GetHeuristic(CurrentBoard, pivot.Key);
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
