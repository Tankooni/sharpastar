using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Indigo;
using Indigo.Core;
using Indigo.Graphics;
using MoogGeneral.Collections;

namespace aStar
{
	public class MapRow
	{
		public List<MapCell> Columns = new List<MapCell>();
	}

	public class TileMap : Entity
	{
		public List<MapRow> Rows = new List<MapRow>();
		public int MapWidth = 50;
		public int MapHeight = 50;

		const int TileSize = 16;

		public TileMap()
		{
			var startIndex = Tuple.Create<int, int>((int)(FP.Random * 50), (int)(FP.Random * 50));
			var endIndex = Tuple.Create<int, int>((int)(FP.Random * 50), (int)(FP.Random * 50));
			while(startIndex.Item1 == endIndex.Item1 && startIndex.Item1 == endIndex.Item2)
				endIndex = Tuple.Create<int, int>((int)(FP.Random * 50), (int)(FP.Random * 50));
			for (int y = 0; y < MapHeight; y++)
			{
				MapRow row = new MapRow();
				for (int x = 0; x < MapWidth; x++)
				{
					TileType tileType;
					if (x == startIndex.Item1 && y == startIndex.Item2)
						tileType = TileType.Start;
					else if (x == endIndex.Item1 && y == endIndex.Item2)
						tileType = TileType.End;
					else
						tileType = FP.Choose.EnumWeighted<TileType>(0, 0, 0, 1, .3);
					MapCell mc = new MapCell(tileType, TileSize, Tuple.Create<int, int>(x, y))
					{
						X = x * TileSize,
						Y = y * TileSize
					};
					this.AddComponent<Image>(mc);
					row.Columns.Add(mc);
				}
				Rows.Add(row);
			}

			foreach (var tile in SelectAstarPath(startIndex, endIndex, false))
				tile.ChangeTileType(TileType.Path);
			//foreach (var mc in SelectTilesAroundTile(Tuple.Create<int, int>(0, 0), true))
			//	mc.Item1.ChangeTileType(TileType.Path);
			//foreach (var mc in SelectTilesAroundTile(Tuple.Create<int, int>(5, 5), false))
			//	mc.Item1.ChangeTileType(TileType.Path);
			//foreach (var mc in SelectTilesAroundTile(Tuple.Create<int, int>(49, 0), true))
			//	mc.Item1.ChangeTileType(TileType.Path);
			//foreach (var mc in SelectTilesAroundTile(Tuple.Create<int, int>(0, 49), true))
			//	mc.Item1.ChangeTileType(TileType.Path);
			//foreach (var mc in SelectTilesAroundTile(Tuple.Create<int, int>(49, 49), true))
			//	mc.Item1.ChangeTileType(TileType.Path);
		}

		/// <summary>
		/// Selects all tiles and the movement cost of moving to that tile around a specified tile
		/// </summary>
		/// <param name="curPosition"></param>
		/// <param name="includeDiag">This will determine if diagnals are to be included. Defaults to false</param>
		/// <returns></returns>
		public IEnumerable<Tuple<MapCell, float>> SelectTilesAroundTile(Tuple<int, int> curPosition, bool includeDiag = false)
		{
			int[] search = new int[]{-1, 0, 1};
			foreach (int x in search)
			{
				if (curPosition.Item1 + x >= MapWidth)
					break;
				if (curPosition.Item1 + x < 0)
					continue;
				foreach (int y in search)
				{
					if (curPosition.Item2 + y >= MapHeight)
						break;
					if (curPosition.Item2 + y < 0 || (x == 0 && y == 0))
						continue;
					MapCell mapCell = Rows[curPosition.Item1 + x].Columns[curPosition.Item2 + y];
					//if (MapCell.TileSpeeds[mapCell.TileType] <= 0)
					//	continue;

					float moveMultiplier = MapCell.TileSpeeds[mapCell.TileType];
					if(Math.Abs(x) == 1 && Math.Abs(y) == 1)
					{
						if (!includeDiag)
							continue;
						moveMultiplier *= 1.41f;
					}
					yield return Tuple.Create<MapCell, float>(mapCell, moveMultiplier);
				}
			}
		}

		public float CalculateHeuristic(int x1, int y1, int x2, int y2)
		{
			return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
		}

		public float CalculateHeuristic(Tuple<int, int> p1, Tuple<int, int> p2)
		{
			return Math.Abs(p1.Item1 - p2.Item1) + Math.Abs(p1.Item2 - p2.Item2);
		}

		public IEnumerable<MapCell> SelectAstarPath(Tuple<int, int> start, Tuple<int, int> end, bool includeDiag)
		{
			MapCell startTile = Rows[start.Item1].Columns[start.Item2];
			MapCell endTile = Rows[end.Item1].Columns[end.Item2];
			PriorityQueue<MapCell> frontier = new PriorityQueue<MapCell>();
			frontier.Push(0, startTile);
			Dictionary<MapCell, MapCell> cameFrom = new Dictionary<MapCell, MapCell>();
			Dictionary<MapCell, float> costSoFar = new Dictionary<MapCell, float>();

			cameFrom.Add(startTile, null);
			costSoFar.Add(startTile, 0);

			if (startTile.Index.Item1 != endTile.Index.Item1 && startTile.Index.Item2 != endTile.Index.Item2)
			{
				while(!frontier.Empty)
				{
					MapCell current = frontier.Pop();
					foreach(var nextTile in SelectTilesAroundTile(current.Index))
					{
						float newCost = costSoFar[nextTile.Item1] + nextTile.Item2;
						if (!costSoFar.ContainsKey(nextTile.Item1))
							costSoFar.Add(nextTile.Item1, newCost);
						else if (newCost < costSoFar[nextTile.Item1])
							costSoFar[nextTile.Item1] = newCost;
						else
							continue;
						float priority = newCost + CalculateHeuristic(endTile.Index, nextTile.Item1.Index);
						frontier.Push(priority, nextTile.Item1);
						if (cameFrom.ContainsKey(nextTile.Item1))
							cameFrom[nextTile.Item1] = current;
						else
							cameFrom.Add(nextTile.Item1, current);
					}
				}
			}

			MapCell currentTile = endTile;
			while (currentTile.Index.Item1 != endTile.Index.Item1 && currentTile.Index.Item2 != endTile.Index.Item2)
				yield return currentTile = cameFrom[currentTile];

		}
		//public override void Added()
		//{
		//	base.Added();

		//	foreach (MapRow mr in Rows)
		//		foreach (MapCell mc in mr.Columns)
		//			FP.World.Add(mc);
		//}
		//public override void Removed()
		//{
		//	base.Removed();
		//	foreach (MapRow mr in Rows)
		//		//this.World.AddList(mr.Columns);
		//		foreach (MapCell mc in mr.Columns)
		//			FP.World.Remove(mc);
		//}
	}
}
