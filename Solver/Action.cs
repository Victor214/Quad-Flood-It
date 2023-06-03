using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solver
{
    public class Action
    {
        public string? Pivot { get; set; }
        public int Color { get; set; }

        public Action(string? pivot, int color)
        {
            Pivot = pivot;
            Color = color;
        }
    }
}
