﻿using Game;
using System;
using System.Collections.Generic;
using System.Drawing;
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
                {
                    _currentBoard = RootBoard.Clone();
                    foreach (var action in Actions)
                    {
                        _currentBoard.Paint(_currentBoard.Pivots[action.Pivot!], action.Color);
                    }
                }

                return _currentBoard;
            }
        }

        public State(Board board)
        {
            RootBoard = board;
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
            List<State> result = new List<State>();
            for (int i = 1; i < CurrentBoard.TotalColors; i++)
            {
                if (i == CurrentBoard.Pivots[Pivot!].Color) // Except current color.
                    continue;

                Action action = new Action(this.Pivot, i);
                result.Add(new State(this, action));
            }

            return result;
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
        #endregion
    }
}
