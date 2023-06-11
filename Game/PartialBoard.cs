using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class PartialBoard : Board
    {
        public RootBoard RootBoard { get; set; }
        public string ExpandingPivot { get; set; }
        public Dictionary<Island, Island> PartialToRoot { get; set; } = new Dictionary<Island, Island>();
        public HashSet<Island> Merged = new HashSet<Island>();

        public PartialBoard(RootBoard rootBoard, string expandingPivot, int width, int height)
            :base(width, height)
        {
            RootBoard = rootBoard;
            ExpandingPivot = expandingPivot;
        }

        public HashSet<Island> Enumerate()
        {
            // Returns all islands not part of ExpandingPivot (All - Merged).
            // Non-ExpandingPivot islands are guaranteed to be untouched.

            var visited = new HashSet<Island>();
            var result = new HashSet<Island>();
            var queue = new Queue<Island>();

            queue.Enqueue(RootBoard.Pivots[ExpandingPivot]);
            visited.Add(queue.Peek());

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                // Iterate through all, but only add non-merged (not part of ExpandingPivot) as result
                if (!Merged.Contains(current))
                    result.Add(current);

                foreach (var island in current.Neighbours)
                {
                    if (visited.Contains(island))
                        continue;

                    visited.Add(island);
                    queue.Enqueue(island);
                }
            }

            return result;
        }

        public List<int> GetRemainingColors()
        {
            return Enumerate().Select(x => x.Color).Distinct().ToList();
        }

        public List<int> GetAdjacentColors()
        {
            return Pivots[ExpandingPivot].Neighbours.Select(x => x.Color).Distinct().ToList();
        }

        #region Board Cloning
        public override PartialBoard CreatePartialBoard(string? pivot = null)
        {
            PartialBoard board = new PartialBoard(rootBoard: RootBoard, expandingPivot: ExpandingPivot, width: Width, height: Height);
            var clonedMap = CloneIslands(board, ExpandingPivot);

            // Copy closed list to cloned partial board
            board.Merged = Merged.ToHashSet();

            // Build PartialToRoot with board's cloned islands as keys, and root's islands as values 
            board.PartialToRoot = Pivots[ExpandingPivot].Neighbours
                .ToDictionary(x => clonedMap[x], x => PartialToRoot[x]);

            return board;
        }
        #endregion

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
                // Already Merged (Part of partial's pivot)
                if (Merged.Contains(unexistingNeighbour))
                    continue;

                // Already exists as an open node in graph (leaf)
                if (rootToPartial.ContainsKey(unexistingNeighbour))
                    continue;

                // Clone and create bidirectional dictionary references from root to partial, and vice versa
                rootToPartial.Add(unexistingNeighbour, unexistingNeighbour.Clone());
                PartialToRoot.Add(rootToPartial[unexistingNeighbour], unexistingNeighbour);

                // Connect to neighbours which already exist in partial board
                // 'unexistingNeighbour' will never be adjacent to pivot node, so neighbour will never be the main pivot node
                foreach (var neighbour in unexistingNeighbour.Neighbours)
                {
                    if (rootToPartial.ContainsKey(neighbour))
                        rootToPartial[unexistingNeighbour].Connect(rootToPartial[neighbour]);
                }

                // If pivot node in root board, then add as pivot in partial (can be multiple)
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
