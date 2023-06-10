using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class PartialBoard : Board
    {
        public RootBoard RootBoard { get; set; }
        public Dictionary<Island, Island> PartialToRoot { get; set; } = new Dictionary<Island, Island>();
        public HashSet<Island> Merged = new HashSet<Island>();

        public PartialBoard(RootBoard rootBoard, int width, int height)
            :base(width, height)
        {
            RootBoard = rootBoard;
        }

        //public Dictionary<Island, Island> Enumerate()
        //{
        //    // Non-Pivot nodes are guaranteed to be untouched.

        //    var result = new Dictionary<Island, Island>();
        //    var queue = new Queue<Island>();

        //    queue.Enqueue(Pivots.First().Value);
        //    result.Add(queue.Peek(), queue.Peek());

        //    while (queue.Count > 0)
        //    {
        //        var current = queue.Dequeue();
        //        foreach (var island in current.Neighbours)
        //        {
        //            if (result.ContainsKey(island))
        //                continue;

        //            result.Add(island, island);
        //            queue.Enqueue(island);
        //        }
        //    }

        //    return result;
        //}

        //public List<int> GetRemainingColors()
        //{
        //    return Enumerate().Select(x => x.Key.Color).Distinct().ToList();
        //}

        public List<int> GetAdjacentColors(string pivot)
        {
            return Pivots[pivot].Neighbours.Select(x => x.Color).Distinct().ToList();
        }

        #region Painting
        public void Paint(Island source, int newColor)
        {
            if (source.Color == newColor)
                return;

            // Bring in merging nodes' adjacents into this partial board
            var rootToPartial = PartialToRoot.ToDictionary(x => x.Value, x => x.Key);
            foreach (var mergingNode in source.Neighbours.Where(x => x.Color == newColor))
            {
                GenerateMergingNodeNeighbours(rootToPartial, mergingNode);
            }

            int oldColor = source.Color;
            source.Color = newColor;

            // Looking beyond the immediate neighbours for same-color neighbours doesn't make sense / is not required, because these islands are assumed to be already valid.
            foreach (var neighbour in source.Neighbours.Where(x => x.Color == newColor).ToList())
            {
                Merge(source, neighbour);
                AdjustPivot(source, neighbour); // This is needed for when a pivot is absorbing another pivot
            }
        }

        private void GenerateMergingNodeNeighbours(Dictionary<Island, Island> rootToPartial, Island mergingNode)
        {
            foreach (var unexistingNeighbour in PartialToRoot[mergingNode].Neighbours)
            {
                if (!rootToPartial.ContainsKey(unexistingNeighbour)) // Neighbour already exists in graph
                    continue;

                // Check closed list (Already merged).
                if (Merged.Contains(unexistingNeighbour))
                    continue;

                // Clone and create bidirectional dictionary references from root to partial, and vice versa
                rootToPartial.Add(unexistingNeighbour, unexistingNeighbour.Clone());
                PartialToRoot.Add(rootToPartial[unexistingNeighbour], unexistingNeighbour);

                // Connect to neighbours which already exist in partial board
                foreach (var neighbour in unexistingNeighbour.Neighbours)
                {
                    if (rootToPartial.ContainsKey(neighbour))
                        rootToPartial[unexistingNeighbour].Connect(rootToPartial[neighbour]);
                }

                // If pivot, then add as pivot (can be multiple)
                foreach (var pivot in RootBoard.Pivots)
                {
                    if (pivot.Value == unexistingNeighbour)
                    {
                        Pivots.Add(pivot.Key, rootToPartial[unexistingNeighbour]);
                    }
                }
            }
        }

        private void Merge(Island source, Island island)
        {
            if (!source.Neighbours.Contains(island))
                throw new ArgumentException("An island merge must happen between neighbouring islands.");

            // Transfer properties, except Color which was already set
            source.Tiles += island.Tiles;

            // Transfer/adjust neighbour pointers
            source.Disconnect(island);
            foreach (var current in island.Neighbours.ToList())
            {
                // If already connected / disconnected, will be ignored.
                current.Disconnect(island);
                source.Connect(current);
            }

            island.Neighbours.Clear(); // Make sure we leave no pointers on merging object, just in case.
            Merged.Add(PartialToRoot[island]); // Add to closed hashset
            PartialToRoot.Remove(island);
        }

        private void AdjustPivot(Island source, Island island)
        {
            foreach (var pivot in Pivots)
            {
                if (pivot.Value == island)
                {
                    Pivots[pivot.Key] = source;
                }
            }
        }
        #endregion
    }
}
