using System.Runtime.CompilerServices;

namespace Game
{
    public abstract class Board //: IEquatable<Board>
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Dictionary<string, Island> Pivots { get; set; } = new Dictionary<string, Island>();

        protected Board(int width, int height)
        {
            Width = width;
            Height = height;
        }

        #region Board Cloning
        public abstract PartialBoard CreatePartialBoard(string pivot);

        protected void CloneIslands(PartialBoard board, string pivot)
        {
            var cloneMap = new Dictionary<Island, Island>();

            // Clone pivot, and add to merged/closed list
            // * It cannot be painted/merged, so its considered as "already painted", as its already part of the pivot
            Island pivotIsland = this.Pivots[pivot];
            cloneMap.Add(pivotIsland, pivotIsland.Clone());
            board.Merged.Add(pivotIsland);

            // Connect neighbouring islands
            foreach (var neighbour in pivotIsland.Neighbours)
            {
                // Clone Island
                cloneMap.Add(neighbour, neighbour.Clone());
                board.PartialToRoot.Add(cloneMap[neighbour], neighbour);
                cloneMap[pivotIsland].Connect(cloneMap[neighbour]);
            }

            // Generate / Adjust Pivot Pointers, if they exist
            foreach (var rootPivot in this.Pivots)
            {
                if (cloneMap.ContainsKey(rootPivot.Value))
                    board.Pivots.Add(rootPivot.Key, cloneMap[rootPivot.Value]);
            }
        }
        #endregion

        #region Equals / GetHashCode Overriding
        // Seems redundant, but required to enable hash and value equality checks on other methods

        // Two boards are equal if islands are equal value-wise
        //public bool Equals(Board? other)
        //{
        //    if (ReferenceEquals(other, null))
        //        return false;
        //    if (ReferenceEquals(this, other))
        //        return true;

        //    var islands = this.Enumerate();
        //    var otherIslands = other.Enumerate();
        //    if (islands.Count != otherIslands.Count)
        //        return false;

        //    foreach (var island in islands)
        //    {
        //        if (!otherIslands.ContainsKey(island.Key)) // GetHashCode Check
        //            return false;

        //        if (!island.Key.Equals(otherIslands[island.Key])) // Actual ValueEquals Check
        //            return false;
        //    }

        //    return true;
        //}

        //public override bool Equals(object? other) => Equals(other as Board);

        //public static bool operator ==(Board? left, Board? right)
        //{
        //    if (ReferenceEquals(left, right))
        //        return true;
        //    if (ReferenceEquals(left, null))
        //        return false;
        //    if (ReferenceEquals(right, null))
        //        return false;

        //    return left.Equals(right);
        //}

        //public static bool operator !=(Board? left, Board? right)
        //{
        //    return !(left == right);
        //}

        //// Board's hashcode is the bitwise XOR combination of all islands
        //public override int GetHashCode()
        //{
        //    unchecked
        //    {
        //        var islands = this.Enumerate();
        //        int hashcode = 0;
        //        foreach (var island in islands)
        //        {
        //            hashcode ^= island.GetHashCode();

        //        }

        //        return hashcode;
        //    }
        //}
        #endregion
    }
}
