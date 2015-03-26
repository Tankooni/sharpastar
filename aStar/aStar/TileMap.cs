using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Indigo;
using Indigo.Core;
using Indigo.Graphics;
using Priority_Queue;
using Indigo.Components;
using Indigo.Inputs;

namespace aStar
{
	public class MapRow
	{
		public List<MapCell> Columns = new List<MapCell>();
	}

	public class TileMap : Entity
	{
		public List<MapRow> Rows = new List<MapRow>();
		public int MapWidth = 35;
		public int MapHeight = 35;

		const int TileSize = 16;

		public TileMap()
		{
			var startIndex = Tuple.Create<int, int>(FP.Rand(MapWidth), FP.Rand(MapHeight));
			var endIndex = Tuple.Create<int, int>(FP.Rand(MapWidth), FP.Rand(MapHeight));
			while(startIndex.Item1 == endIndex.Item1 && startIndex.Item1 == endIndex.Item2)
				endIndex = Tuple.Create<int, int>(FP.Rand(MapWidth), FP.Rand(MapHeight));
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
						tileType = FP.Choose.EnumWeighted<TileType>(0, 0, 0, 1, .65);
					MapCell mc = new MapCell(tileType, TileSize, Tuple.Create<int, int>(x, y))
					{
						X = x * TileSize,
						Y = y * TileSize
					};
					mc.SetPathNode(new PathNode(mc, null, x, y));
					this.AddComponent<Image>(mc);
					row.Columns.Add(mc);
				}
				Rows.Add(row);
			}

			foreach (MapRow mr in Rows)
				foreach (MapCell mc in mr.Columns)
					PathNode.ConnectedNodes[mc.MyNode] = SelectTilesAroundTile(mc.Index.Item1, mc.Index.Item2);

			CoroutineHost coHost = AddComponent<CoroutineHost>(new CoroutineHost());
			coHost.Start(Things(startIndex, endIndex));

			//foreach (var bob in SelectTilesAroundTile(startIndex.Item1, startIndex.Item2))
			//	bob.Item1.attachedObject.ChangeTileType(TileType.Path);

			
			//foreach (var tile in SelectAstarPath(startIndex, endIndex, false))
			//	tile.ChangeTileType(TileType.Path);
		}

		/// <summary>
		/// Selects all tiles and the movement cost of moving to that tile around a specified tile
		/// </summary>
		/// <param name="curPosition"></param>
		/// <param name="includeDiag">This will determine if diagnals are to be included. Defaults to false</param>
		/// <returns></returns>
		public List<Tuple<PathNode, float>> SelectTilesAroundTile(int centerX, int centerY, bool includeDiag = false)
		{
			List<Tuple<PathNode, float>> nodes = new List<Tuple<PathNode, float>>();
			int[] search = new int[]{-1, 0, 1};
			foreach (int x in search)
			{
				if (centerX + x >= MapWidth)
					break;
				if (centerX + x < 0)
					continue;
				foreach (int y in search)
				{
					if (centerY + y >= MapHeight)
						break;
					if (centerY + y < 0 || (x == 0 && y == 0))
						continue;
					MapCell mapCell = Rows[centerY + y].Columns[centerX + x];
					if (MapCell.TileSpeeds[mapCell.TileType] <= 0)
						continue;

					float moveMultiplier = MapCell.TileSpeeds[mapCell.TileType];
					if(Math.Abs(x) == 1 && Math.Abs(y) == 1)
					{
						if (!includeDiag)
							continue;
						moveMultiplier *= 1.41f;
					}
					if(mapCell.MyNode.Enabled)
						nodes.Add(Tuple.Create<PathNode, float>(mapCell.MyNode, moveMultiplier));
				}
			}
			return nodes;
		}

		public float CalculateHeuristic(float x1, float y1, float x2, float y2)
		{
			return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
		}

		public float CalculateHeuristic(Tuple<int, int> p1, Tuple<int, int> p2)
		{
			return Math.Abs(p1.Item1 - p2.Item1) + Math.Abs(p1.Item2 - p2.Item2);
		}

		IEnumerator Things(Tuple<int, int> startIndex, Tuple<int, int> endIndex)
		{
			while (!Keyboard.Space.Pressed)
				yield return null;
			foreach (var thing in SelectAstarPath(startIndex, endIndex))
			{
				thing.attachedObject.ChangeTileType(TileType.Path);
				yield return CoroutineHost.WaitForSeconds(.1f);
			}
		}

		public IEnumerable<PathNode> SelectAstarPath(Tuple<int, int> start, Tuple<int, int> end)
		{
			PathNode startNode = Rows[start.Item2].Columns[start.Item1].MyNode;
			PathNode endNode = Rows[end.Item2].Columns[end.Item1].MyNode;
			HeapPriorityQueue<PathNode> frontier = new HeapPriorityQueue<PathNode>(PathNode.ConnectedNodes.Keys.Count);

			frontier.Enqueue(startNode, 0);

			Dictionary<PathNode, PathNode> cameFrom = new Dictionary<PathNode, PathNode>();
			Dictionary<PathNode, float> costSoFar = new Dictionary<PathNode, float>();

			cameFrom.Add(startNode, null);
			costSoFar.Add(startNode, 0);

			if (startNode.X != endNode.X && startNode.Y != endNode.Y)
			{
				while (frontier.Count > 0)
				{
					PathNode current = frontier.Dequeue();
					foreach (var next in PathNode.ConnectedNodes[current])
					{
						float newCost = costSoFar[current] + next.Item2;
						if (!costSoFar.ContainsKey(next.Item1))
							costSoFar.Add(next.Item1, newCost);
						else if (newCost < costSoFar[next.Item1])
							costSoFar[next.Item1] = newCost;
						else
							continue;
						float priority = newCost + CalculateHeuristic(endNode.X, endNode.Y, startNode.X, startNode.Y);
						frontier.Enqueue(next.Item1, priority);
						
						if (cameFrom.ContainsKey(next.Item1))
							cameFrom[next.Item1] = current;
						else
							cameFrom.Add(next.Item1, current);
					}
				}
			}

			PathNode currentNode = endNode;
			while (startNode.X != currentNode.X || startNode.Y != currentNode.Y)
			{
				if (cameFrom.ContainsKey(currentNode))
				{
					yield return currentNode = cameFrom[currentNode];
				}
				else
					break;
			}

		}
	}
}
