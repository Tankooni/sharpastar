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

namespace hexStar
{
	public class MapRow
	{
		public List<MapCell> Columns = new List<MapCell>();
	}

	public class TileMap : Entity
	{
		public List<MapRow> Rows = new List<MapRow>();
		public int MapWidth = 20;
		public int MapHeight = 40;

		const int TileSize = 16;
		const int TileWidth = 33;
		const int TileHeight = 27;
		const int TileStepX = 52;
		const int TileStepY = 14;
		const int OddRowXOffset = 26;

		public TileMap()
		{
			var startIndex = Tuple.Create<int, int>(FP.Rand(MapWidth), FP.Rand(MapHeight));
			var endIndex = Tuple.Create<int, int>(FP.Rand(MapWidth), FP.Rand(MapHeight));
			while (startIndex.Item1 == endIndex.Item1 && startIndex.Item1 == endIndex.Item2)
				endIndex = Tuple.Create<int, int>(FP.Rand(MapWidth), FP.Rand(MapHeight));
			for (int y = 0; y < MapHeight; y++)
			{
				int rowOffset = (y % 2 == 1) ? OddRowXOffset : 0;
				MapRow row = new MapRow();
				for (int x = 0; x < MapWidth; x++)
				{
					TileType tileType;
					if (x == startIndex.Item1 && y == startIndex.Item2)
						tileType = TileType.Start;
					else if (x == endIndex.Item1 && y == endIndex.Item2)
						tileType = TileType.End;
					else
						tileType = FP.Choose.EnumWeighted<TileType>(0, 0, 0, 1, .65f, 0 );
					MapCell mc = new MapCell(tileType, TileSize, Tuple.Create<int, int>(x, y))
					{
						X = x * TileStepX + rowOffset,
						Y = y * TileStepY
					};
					mc.SetPathNode(new PathNode(mc, null, mc.X, mc.Y));
					this.AddComponent<Image>(mc);
					row.Columns.Add(mc);
				}
				Rows.Add(row);
			}

			//CoroutineHost coHost1 = AddComponent<CoroutineHost>(new CoroutineHost());
			//coHost1.Start(RunBuildNodeConnections());

			foreach (MapRow mr in Rows)
				foreach (MapCell mc in mr.Columns)
					PathNode.ConnectedNodes[mc.MyNode] = SelectTilesAroundTile(mc.Index.Item1, mc.Index.Item2);

			CoroutineHost coHost2 = AddComponent<CoroutineHost>(new CoroutineHost());
			coHost2.Start(RunBuildPath(startIndex, endIndex));
		}

		public List<Tuple<PathNode, float>> SelectTilesAroundTile(int centerX, int centerY, bool includeDiag = false)
		{
			List<Tuple<PathNode, float>> nodes = new List<Tuple<PathNode, float>>();
			int evens = (centerY % 2 == 0 ? -1 : 1);
			List<Tuple<int, int>> toCheck = new List<Tuple<int,int>>
			{
				Tuple.Create<int, int>(0, -2),
				Tuple.Create<int, int>(0, 2),
				Tuple.Create<int, int>(0, -1),
				Tuple.Create<int, int>(evens , -1),
				Tuple.Create<int, int>(0, 1),
				Tuple.Create<int, int>(evens, 1)
			};

			foreach(var checkPosition in toCheck)
			{
				int x = checkPosition.Item1 + centerX;
				if (x < 0 || x >= MapWidth)
					continue;
				int y = checkPosition.Item2 + centerY;
				if (y < 0 || y >= MapHeight)
					continue;
				MapCell mapCell = Rows[y].Columns[x];
				float moveMultiplier = MapCell.TileSpeeds[mapCell.TileType];
				if (moveMultiplier <= 0 || mapCell.MyNode == null || !mapCell.MyNode.Enabled)
					continue;
				nodes.Add(Tuple.Create<PathNode, float>(mapCell.MyNode, moveMultiplier));
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

		IEnumerator RunBuildNodeConnections()
		{
			MapCell prevMapCell = null;
			bool holdUp = false;
			foreach (MapRow mr in Rows)
				foreach (MapCell mc in mr.Columns)
				{
					while (!Keyboard.D.Pressed || holdUp)
					{
						holdUp = false;
						yield return null;
					}
					holdUp = true;
					if (prevMapCell != null)
					{
						prevMapCell.ChangeTileType(TileType.Regular);
						foreach (var cell in SelectTilesAroundTile(prevMapCell.Index.Item1, prevMapCell.Index.Item2))
						{
							cell.Item1.attachedObject.ChangeTileType(TileType.Regular);
						}
					}
					mc.ChangeTileType(TileType.Path);
					foreach (var node in PathNode.ConnectedNodes[mc.MyNode] = SelectTilesAroundTile(mc.Index.Item1, mc.Index.Item2))
						node.Item1.attachedObject.ChangeTileType(TileType.Considered);
					prevMapCell = mc;
				}
		}

		IEnumerator RunBuildPath(Tuple<int, int> startIndex, Tuple<int, int> endIndex)
		{
			while (!Keyboard.Space.Pressed)
				yield return null;
			foreach (var thing in SelectAstarPath(startIndex, endIndex))
			{
				thing.attachedObject.ChangeTileType(TileType.Path);
				yield return CoroutineHost.WaitForSeconds(.15f);
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
						if (next.Item1.attachedObject.TileType != TileType.Start && next.Item1.attachedObject.TileType != TileType.End)
							next.Item1.attachedObject.ChangeTileType(TileType.Considered);
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