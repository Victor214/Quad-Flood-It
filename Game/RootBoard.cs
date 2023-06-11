using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class RootBoard : Board
    {
        public RootBoard(string boardText)
            :base(0, 0)
        {
            string[] boardLines = boardText.Replace("\r", "").Split('\n');
            string[] line = boardLines[0].Split(' ');
            Width = Convert.ToInt32(line[0]);
            Height = Convert.ToInt32(line[1]);

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

        #region Board Cloning
        public override PartialBoard CreatePartialBoard(string? pivot)
        {
            PartialBoard board = new PartialBoard(rootBoard: this, expandingPivot: pivot!, width: this.Width, height: this.Height);
            var clonedMap = CloneIslands(board, pivot!);

            // Add expandingPivot to merged, as nothing besides the root has been painted so far
            board.Merged.Add(Pivots[pivot!]);

            // Build PartialToRoot with board's cloned islands as keys, and root's islands as values 
            board.PartialToRoot = Pivots[pivot!].Neighbours
                .ToDictionary(x => clonedMap[x], x => x);

            return board;
        }
        #endregion


        #region Board Parsing
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

                    foreach (var tile in tiles)
                        islandMap[tile.Y, tile.X] = island;

                    List<Island> distinctGeneratedNeighbours = GetGeneratedNeighbours(islandMap, island, tiles).ToList();
                    foreach (var neighbour in distinctGeneratedNeighbours)
                        island.Connect(neighbour);
                }
            }

            Pivots.Add("a", islandMap[0, 0]);
            Pivots.Add("b", islandMap[0, board.GetLength(1) - 1]);
            Pivots.Add("c", islandMap[board.GetLength(0) - 1, board.GetLength(1) - 1]);
            Pivots.Add("d", islandMap[board.GetLength(0) - 1, 0]);
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

            if (board[i, j] != color)
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
            int[] i = new int[] { 1, -1, 0, 0 };
            int[] j = new int[] { 0, 0, 1, -1 };

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
    }
}
