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
        public Board Board { get; set; }
        public State? Parent { get; set; }
        public int PathCost { get; set; }

        public State(Board board, State? parent = null)
        {
            Board = board;
            Parent = parent;
            PathCost = Parent != null ? Parent.PathCost + 1 : 0;
        }

        public int GetHeuristic()
        {
            return 0;
        }

        public int GetEvaluationFunction()
        {
            return PathCost + GetHeuristic();
        }

        public List<State> Expand()
        {
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
    }
}
