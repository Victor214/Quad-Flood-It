using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class Island //: IEquatable<Island>
    {
        public Board Board { get; set; }
        public int Color { get; set; }
        public int Tiles { get; set; }

        public HashSet<Island> Neighbours { get; set; } = new HashSet<Island>();

        public Island(Board board)
        {
            Board = board;
        }

        public Island Clone(Board board)
        {
            return new Island(board)
            {
                Color = this.Color,
                Tiles = this.Tiles
            };
        }

        public bool Connect(Island that)
        {
            if (this.Neighbours.Contains(that) || that.Neighbours.Contains(this))
                return false;

            this.Neighbours.Add(that);
            that.Neighbours.Add(this);
            return true;
        }

        public void Disconnect(Island that)
        {
            if (!this.Neighbours.Contains(that) || !that.Neighbours.Contains(this))
                return;

            this.Neighbours.Remove(that);
            that.Neighbours.Remove(this);
        }

        #region Equals / GetHashCode Overriding
        //public bool Equals(Island? other)
        //{
        //    if (ReferenceEquals(other, null))
        //        return false;
        //    if (ReferenceEquals(this, other))
        //        return true;

        //    if (this.Color != other.Color)
        //        return false;

        //    var tiles = Tiles.ToDictionary(x => x, x => x);
        //    var otherTiles = other.Tiles.ToDictionary(x => x, x => x);

        //    if (tiles.Count != otherTiles.Count)
        //        return false;

        //    foreach (var tile in tiles)
        //    {
        //        if (!otherTiles.ContainsKey(tile.Key)) // GetHashCode Check
        //            return false;

        //        if (!tile.Key.Equals(otherTiles[tile.Key])) // Actual ValueEquals Check
        //            return false;
        //    }

        //    return true;
        //}

        //public override bool Equals(object? other) => Equals(other as Island);

        //public static bool operator ==(Island? left, Island? right)
        //{
        //    if (ReferenceEquals(left, right))
        //        return true;
        //    if (ReferenceEquals(left, null))
        //        return false;
        //    if (ReferenceEquals(right, null))
        //        return false;

        //    return left.Equals(right);
        //}

        //public static bool operator !=(Island? left, Island? right)
        //{
        //    return !(left == right);
        //}

        //public override int GetHashCode()
        //{
        //    int tilesHashCode = Tiles
        //        .Select(x => x.GetHashCode())
        //        .Aggregate((result, item) => result ^ item);

        //    return HashCode.Combine(Color, tilesHashCode);
        //}
        #endregion

    }
}
