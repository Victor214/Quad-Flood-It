using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solver
{
    public class StatePriority
    {
        public State State { get; set; }
        public float Priority { get; set; }

        public StatePriority(State state, float priority)
        {
            State = state;
            Priority = priority;
        }
    }
}
