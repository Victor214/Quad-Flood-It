using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class Island
    {
        public Board Board { get; set; }
        public int Color { get; set; }
        public List<Tile> Tiles { get; set; } = new List<Tile>();

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
                Tiles = this.Tiles.ToList()
            };
        }

        public void Paint(int color)
        {
            Color = color;

            // Looking beyond the immediate neighbours for same-color neighbours doesn't make sense / is not required, because these islands are assumed to be already valid.
            foreach (var neighbour in Neighbours.Where(x => x.Color == Color))
            {
                Merge(neighbour);
                AdjustPivot(neighbour);
            }
        }

        public void Connect(Island that)
        {
            if (this.Neighbours.Contains(that) || that.Neighbours.Contains(this))
                return;

            this.Neighbours.Add(that);
            that.Neighbours.Add(this);
        }

        public void Disconnect(Island that)
        {
            if (!this.Neighbours.Contains(that) || !that.Neighbours.Contains(this))
                return;

            this.Neighbours.Remove(that);
            that.Neighbours.Remove(this);
        }

        #region Helper Methods
        private void Merge(Island island)
        {
            if (!Neighbours.Contains(island))
                throw new ArgumentException("An island merge must happen between neighbouring islands.");

            // Transfer properties
            Color = island.Color;
            Tiles.AddRange(island.Tiles);

            // Transfer/adjust neighbour pointers
            this.Disconnect(island);
            foreach (var current in island.Neighbours)
            {
                current.Neighbours.Remove(island);
                if (!current.Neighbours.Contains(this))
                {
                    if (this.Neighbours.Contains(current))
                        throw new Exception("Unmirrored neighbour pointers, this should not happen!");

                    this.Connect(current);
                }
            }
            island.Neighbours.Clear(); // Make sure we leave no pointers on merging object, just in case.
        }

        private void AdjustPivot(Island island)
        {
            foreach (var pivot in Board.Pivots)
            {
                if (pivot.Value == island)
                {
                    Board.Pivots[pivot.Key] = this;
                }
            }
        }
        #endregion

    }
}
