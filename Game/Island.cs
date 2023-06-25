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
        public int Color { get; set; }
        public int Tiles { get; set; } // Amount of tiles of the current island node

        // Neighbour islands (edges of graph).
        public HashSet<Island> Neighbours { get; set; } = new HashSet<Island>();

        public Island Clone()
        {
            return new Island()
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

    }
}
