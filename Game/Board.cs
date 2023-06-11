using System.Runtime.CompilerServices;

namespace Game
{
    public abstract class Board //: IEquatable<Board>
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Dictionary<string, Island> Pivots { get; set; } = new Dictionary<string, Island>();

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
        public abstract PartialBoard CreatePartialBoard(string? pivot = null);

        protected Dictionary<Island, Island> CloneIslands(PartialBoard board, string pivot)
        {
            var cloneMap = new Dictionary<Island, Island>();

            // Clone pivot, and add to merged/closed list
            // * It cannot be painted/merged, so its considered as "already painted", as its already part of the pivot
            Island pivotIsland = Pivots[pivot];
            cloneMap.Add(pivotIsland, pivotIsland.Clone());

            // Create islands
            foreach (var neighbour in pivotIsland.Neighbours)
            {
                // Clone Island
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
