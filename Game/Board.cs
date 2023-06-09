namespace Game
{
    public class Board //: IEquatable<Board>
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Dictionary<string, Island> Pivots { get; set; } = new Dictionary<string, Island>();

        private int[] _colorMap;
        public int TotalColors
        {
            get
            {
                return _colorMap.Count(x => x > 0);
            }
        }

        public List<int> GetRemainingColors()
        {
            return Enumerate().Select(x => x.Key.Color).Distinct().ToList();
        }

        public List<int> GetAdjacentColors(string pivot)
        {
            return Pivots[pivot].Neighbours.Select(x => x.Color).Distinct().ToList();
        }

        public int GetBoardMaxDepth(string pivot)
        {
            var visitedMap = new Dictionary<Island, int>();
            var queue = new Queue<Island>();

            queue.Enqueue(Pivots[pivot]);
            visitedMap.Add(queue.Peek(), 0);
            int max = 0;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var island in current.Neighbours)
                {
                    if (visitedMap.ContainsKey(island))
                        continue;

                    int distance = visitedMap[current] + 1;
                    if (distance > max)
                        max = distance;

                    visitedMap.Add(island, distance);
                    queue.Enqueue(island);
                }
            }

            return max;
        }

        #region Board Parsing
        public Board(string boardText)
        {
            string[] boardLines = boardText.Replace("\r", "").Split('\n');
            string[] line = boardLines[0].Split(' ');
            Width = Convert.ToInt32(line[0]);
            Height = Convert.ToInt32(line[1]);
            _colorMap = new int[Convert.ToInt32(line[2])];

            var board = new int[Height, Width];
            for (int i = 0; i < Height; i++)
            {
                line = boardLines[i + 1].Split(' ');
                for (int j = 0; j < Width; j++)
                {
                    board[i, j] = Convert.ToInt32(line[j]);
                }
            }

            GenerateGraph(board);
        }

        private void GenerateGraph(int[,] board)
        {
            Island[,] islandMap = new Island[board.GetLength(0), board.GetLength(1)];

            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (islandMap[i, j] != null)
                        continue;

                    Island island = new Island();
                    var tiles = GetIslandTiles(board, i, j).ToList();
                    island.Color = board[i, j];
                    island.Tiles = tiles.Count;
                    _colorMap[island.Color - 1] += 1;

                    foreach (var tile in tiles)
                        islandMap[tile.Y, tile.X] = island;

                    List<Island> distinctGeneratedNeighbours = GetGeneratedNeighbours(islandMap, island, tiles).ToList();
                    foreach (var neighbour in distinctGeneratedNeighbours)
                        island.Connect(neighbour);
                }
            }

            Pivots.Add("a", islandMap[                   0,                    0]);
            Pivots.Add("b", islandMap[                   0, board.GetLength(1)-1]);
            Pivots.Add("c", islandMap[board.GetLength(0)-1, board.GetLength(1)-1]);
            Pivots.Add("d", islandMap[board.GetLength(0)-1,                    0]);
        }

        private HashSet<Tile> GetIslandTiles(int[,] board, int i, int j, HashSet<Tile>? tiles = null, int? color = null)
        {
            if (tiles == null)
                tiles = new HashSet<Tile>();

            // Boundary Check
            if (i < 0 || j < 0 || i > board.GetLength(0) - 1 || j > board.GetLength(1) - 1)
                return tiles;

            // Repetition Check
            var currentTile = new Tile(j, i);
            if (tiles.Contains(currentTile))
                return tiles;

            // Color Check
            if (color == null)
                color = board[i, j];

            if (board[i,j] != color)
                return tiles;

            // Add tiles
            tiles.Add(currentTile);
            GetIslandTiles(board, i + 1, j, tiles, color);
            GetIslandTiles(board, i - 1, j, tiles, color);
            GetIslandTiles(board, i, j + 1, tiles, color);
            GetIslandTiles(board, i, j - 1, tiles, color);
            return tiles;
        }

        private HashSet<Island> GetGeneratedNeighbours(Island[,] islandMap, Island mainIsland, List<Tile> tiles)
        {
            int[] i = new int[] { 1, -1, 0,  0 };
            int[] j = new int[] { 0,  0, 1, -1 };

            HashSet<Island> distinctAdjacentGeneratedIslands = new HashSet<Island>();

            foreach (var tile in tiles)
            {
                for (int k = 0; k < 4; k++)
                { 
                    var neighbour = GetIslandFromMap(islandMap, mainIsland, tile.Y + i[k], tile.X + j[k]);
                    if (neighbour == null || neighbour == mainIsland)
                        continue;

                    if (distinctAdjacentGeneratedIslands.Contains(neighbour))
                        continue;

                    distinctAdjacentGeneratedIslands.Add(neighbour);
                }
            }

            return distinctAdjacentGeneratedIslands;
        }

        private Island? GetIslandFromMap(Island[,] islandMap, Island mainIsland, int i, int j)
        {
            if (i < 0 || j < 0 || i > islandMap.GetLength(0) - 1 || j > islandMap.GetLength(1) - 1)
                return null;

            return islandMap[i, j];
        }
        #endregion

        #region Painting
        public void Paint(Island source, int newColor)
        {
            if (source.Color == newColor)
                return;

            int oldColor = source.Color;
            source.Color = newColor;
            _colorMap[oldColor - 1] -= 1;
            _colorMap[newColor - 1] += 1;

            // Looking beyond the immediate neighbours for same-color neighbours doesn't make sense / is not required, because these islands are assumed to be already valid.
            foreach (var neighbour in source.Neighbours.Where(x => x.Color == newColor).ToList())
            {
                Merge(source, neighbour);
                AdjustPivot(source, neighbour); // This is needed for when a pivot is absorbing another pivot
                _colorMap[newColor - 1] -= 1;
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

        #region Equals / GetHashCode Overriding
        // Seems redundant, but required to enable hash and value equality checks on other methods
        public Dictionary<Island, Island> Enumerate()
        {
            var result = new Dictionary<Island, Island>();
            var queue = new Queue<Island>();

            queue.Enqueue(Pivots.First().Value);
            result.Add(queue.Peek(), queue.Peek());

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var island in current.Neighbours)
                {
                    if (result.ContainsKey(island))
                        continue;

                    result.Add(island, island);
                    queue.Enqueue(island);
                }
            }

            return result;
        }

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


        #region Board Replicating (Bulk)
        //private Board(int width, int height, int[] colorMap)
        //{
        //    Width = width;
        //    Height = height;
        //    _colorMap = colorMap.ToArray();
        //}

        //public Board Clone()
        //{
        //    return BulkClone(1).First();
        //}

        //public List<Board> BulkClone(int amount)
        //{
        //    List<Board> result = new List<Board>();
        //    for (int i = 0; i < amount; i++)
        //    {
        //        result.Add(new Board(width: this.Width, height: this.Height, colorMap: this._colorMap));
        //    }

        //    BulkCloneIslandGraph(result);
        //    return result;
        //}

        //private void BulkCloneIslandGraph(List<Board> boards)
        //{
        //    var totalIslands = new HashSet<Island>();
        //    var visitedMap = new ConcurrentDictionary<Island, ConcurrentDictionary<Board, Island>>();
        //    var queue = new Queue<Island>();

        //    var rootIsland = Pivots.FirstOrDefault().Value;
        //    queue.Enqueue(rootIsland);
        //    totalIslands.Add(rootIsland);

        //    // Add rootIsland to all cloned boards
        //    //visitedMap.BulkAddIsland(boards, rootIsland);


        //    int count = 0;
        //    while (queue.Count > 0)
        //    {
        //        count++;
        //        var island = queue.Dequeue();

        //        foreach (var neighbour in island.Neighbours)
        //        {
        //            if (totalIslands.Contains(neighbour))
        //            {
        //                continue;
        //            }

        //            // Add current neighbour to all cloned boards, and connect to respective islands
        //            //visitedMap.BulkAddIsland(boards, neighbour);
        //            totalIslands.Add(neighbour);
        //            queue.Enqueue(neighbour);
        //        }
        //    }

        //    // Clone Islands
        //    Parallel.ForEach(totalIslands, island =>
        //    {
        //        visitedMap.BulkAddIsland(boards, island);
        //    });

        //    // Copy Neighbours
        //    Parallel.ForEach(visitedMap, island =>
        //    {
        //        foreach (var board in boards)
        //        {
        //            foreach (var neighbour in island.Key.Neighbours)
        //            {
        //                //visitedMap[island.Key][board].Neighbours.Add(visitedMap[neighbour][board]);
        //            }
        //        }
        //    });


        //    // Clone Pivots
        //    foreach (var pivot in Pivots)
        //    {
        //        visitedMap.BulkClonePivot(boards, pivot);
        //    }
        //}
        #endregion

        #region Board Replicating (Single)
        private Board(int width, int height, int[] colorMap)
        {
            Width = width;
            Height = height;
            _colorMap = colorMap.ToArray();
        }

        public Board Clone()
        {
            Board board = new Board(width: this.Width, height: this.Height, colorMap: this._colorMap);
            CloneIslands(board);
            return board;
        }

        private void CloneIslands(Board board)
        {
            var visitedMap = new Dictionary<Island, Island>();
            var queue = new Queue<Island>();

            var rootIsland = Pivots.FirstOrDefault().Value;
            queue.Enqueue(rootIsland);
            visitedMap.Add(rootIsland, rootIsland.Clone());

            while (queue.Count > 0)
            {
                var island = queue.Dequeue();
                var clonedIsland = visitedMap[island];

                foreach (var neighbour in island.Neighbours)
                {
                    if (visitedMap.ContainsKey(neighbour))
                    {
                        //clonedIsland.Connect(visitedMap[neighbour]);
                        continue;
                    }

                    Island clonedNeighbour = neighbour.Clone();
                    //clonedIsland.Connect(clonedNeighbour);

                    queue.Enqueue(neighbour);
                    visitedMap.Add(neighbour, clonedNeighbour);
                }
            }

            // Clone Pointers
            foreach (var island in visitedMap)
            {
                foreach (var neighbour in island.Key.Neighbours)
                {
                    island.Value.Neighbours.Add(visitedMap[neighbour]);
                }
            }

            foreach (var pivot in Pivots)
            {
                board.Pivots.Add(pivot.Key, visitedMap[pivot.Value]);
            }
        }
        #endregion
    }

    //public static class BoardExtensions
    //{
    //    public static void BulkAddIsland(this ConcurrentDictionary<Island, ConcurrentDictionary<Board, Island>> dictionary, List<Board> boards, Island key)
    //    {
    //        if (dictionary.ContainsKey(key))
    //            throw new ArgumentException("This key already exists in the visited dictionary.");

    //        dictionary.TryAdd(key, new ConcurrentDictionary<Board, Island>());
    //        foreach (var board in boards)
    //            dictionary[key].TryAdd(board, key.Clone(board));
    //    }

    //    public static void BulkClonePivot(this ConcurrentDictionary<Island, ConcurrentDictionary<Board, Island>> dictionary, List<Board> boards, KeyValuePair<string, Island> pivot)
    //    {
    //        foreach (var board in boards)
    //            board.Pivots.Add(pivot.Key, dictionary[pivot.Value][board]);
    //    }
    //}
}
