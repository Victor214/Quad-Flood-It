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

    }
}
