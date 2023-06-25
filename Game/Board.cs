using System.Runtime.CompilerServices;

namespace Game
{
    public abstract class Board //: IEquatable<Board>
    {
        public int Width { get; set; }
        public int Height { get; set; }

        // A dictionary of corner indexes ("a", "b", "c" or "d"), and their respective Island (node) in the graph
        public Dictionary<string, Island> Pivots { get; set; } = new Dictionary<string, Island>();

        // Stores a map of colors, with the color id as index, and the amount of islands remaining of that color as value.
        // When a color reaches 0, it means it is not present in the board anymore.
        protected int[] _colorMap;
        public int TotalColors
        {
            get
            {
                return _colorMap.Count(x => x > 0);
            }
        }

        protected Board(int width, int height)
        {
            Width = width;
            Height = height;
            _colorMap = new int[1];
        }

        #region Board Cloning
        // Creates partial board (only pivot and adjacent nodes present)
        public abstract PartialBoard CreatePartialBoard(string? pivot = null);


        // Creates a clone of "this" onto PartialBoard. Note that this is a Partial Clone, as it only clones the pivot, and the adjacent islands.
        protected Dictionary<Island, Island> CloneIslands(PartialBoard board, string pivot)
        {
            var cloneMap = new Dictionary<Island, Island>();

            Island pivotIsland = Pivots[pivot];
            cloneMap.Add(pivotIsland, pivotIsland.Clone());

            // Create islands
            foreach (var neighbour in pivotIsland.Neighbours)
            {
                cloneMap.Add(neighbour, neighbour.Clone());
            }

            // Clone Pointers
            foreach (var islandPair in cloneMap)
            {
                var island = islandPair.Key;
                var clonedIsland = islandPair.Value;

                foreach (var neighbour in island.Neighbours)
                {
                    if (cloneMap.ContainsKey(neighbour))
                        clonedIsland.Neighbours.Add(cloneMap[neighbour]);
                }
            }

            // Generate / Adjust Pivot Pointers, if they exist
            foreach (var rootPivot in Pivots)
            {
                if (cloneMap.ContainsKey(rootPivot.Value))
                    board.Pivots.Add(rootPivot.Key, cloneMap[rootPivot.Value]);
            }

            return cloneMap;
        }

        #endregion
    }
}
