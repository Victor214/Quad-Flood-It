using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class Board
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

                    Island island = new Island(this);
                    island.Tiles = GetIslandTiles(board, i, j).ToList();
                    island.Color = board[i, j];
                    _colorMap[island.Color] += 1;

                    foreach (var tile in island.Tiles)
                        islandMap[tile.Y, tile.X] = island;

                    List<Island> distinctGeneratedNeighbours = GetGeneratedNeighbours(islandMap, island).ToList();
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

        private HashSet<Island> GetGeneratedNeighbours(Island[,] islandMap, Island mainIsland)
        {
            int[] i = new int[] { 1, -1, 0,  0 };
            int[] j = new int[] { 0,  0, 1, -1 };

            HashSet<Island> distinctAdjacentGeneratedIslands = new HashSet<Island>();

            foreach (var tile in mainIsland.Tiles)
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

        #region Board Replicating
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
            visitedMap.Add(rootIsland, rootIsland.Clone(board));

            while (queue.Count > 0)
            {
                var island = queue.Dequeue();
                var clonedIsland = visitedMap[island];

                foreach (var neighbour in island.Neighbours)
                {
                    if (visitedMap.ContainsKey(neighbour))
                    {
                        continue;
                    }

                    Island clonedNeighbour = neighbour.Clone(board);
                    clonedIsland.Connect(clonedNeighbour);

                    queue.Enqueue(neighbour);
                    visitedMap.Add(neighbour, clonedNeighbour);
                }
            }

            foreach (var pivot in Pivots)
            {
                board.Pivots.Add(pivot.Key, visitedMap[pivot.Value]);
            }
        }
        #endregion

    }
}
