using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solver
{
    public class AStar
    {
        private State InitialState { get; set; }
        private PriorityQueue<State, int> Open { get; set; } = new PriorityQueue<State, int>();

        public AStar(Board board)
        {
            InitialState = new State(board);
            Open.Enqueue(InitialState, InitialState.GetEvaluationFunction());
        }

        #region Public Methods
        public void Solve()
        {
            while (Open.Count > 0)
            {
                State state = Open.Dequeue();

            }
        }
        #endregion


        #region Private Methods
        
        #endregion
    }
}
