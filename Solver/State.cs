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
    }
}
