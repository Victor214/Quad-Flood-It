using Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solver
{
    public class AStar
    {
        private State InitialState { get; set; }
        private List<StatePriority> Open { get; set; } = new();
        private HashSet<State> Closed { get; set; } = new();

        public AStar(RootBoard rootBoard)
        {
            InitialState = new State(rootBoard);
            Open.AddSorted(new StatePriority(InitialState, InitialState.GetEvaluationFunction()));
            //InitialState.ClearBoard();
        }

        #region Public Methods
        public State? Solve()
        {

            while (Open.Count > 0)
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();

                State state = Open.Dequeue();

                if (state.IsGoal())
                    return state;

                List<State> children = state.Expand();
                foreach (State child in children)
                {
                    // Boards won't repeat because we only choose adjacent colors as a branching strategy, merging with other squares uniquely.
                    Open.AddSorted(new StatePriority(child, child.GetEvaluationFunction()));
                    child.ClearBoard();
                }

                timer.Stop();
                Console.WriteLine($"Actions: {state.Actions.Count} / Tiles: {state.CurrentBoard.Pivots[state.Pivot!].Tiles} / TotalColors: {state.CurrentBoard.TotalColors} / Heuristic: {state.GetHeuristic()} / Elapsed: {timer.ElapsedMilliseconds}ms");

                Closed.Add(state);
                state.ClearBoard();
            }

            return null;
        }
        #endregion
    }

    public static class ListExtensions
    {
        public static State Dequeue(this List<StatePriority> list)
        {
            var result = list[0];
            list.RemoveAt(0);
            return result.State;
        }

        public static void AddSorted(this List<StatePriority> list, StatePriority element)
        {
            if (list.Count == Configuration.MaxElements)
                list.RemoveAt(list.Count-1); // Remove last

            int x = list.BinarySearch(element, new StatePriorityComparer());
            list.Insert((x >= 0) ? x : ~x, element);
        }
    }

    public class StatePriorityComparer : Comparer<StatePriority>
    {
        public override int Compare(StatePriority? x, StatePriority? y)
        {
            return x!.Priority.CompareTo(y!.Priority);
        }
    }
}
