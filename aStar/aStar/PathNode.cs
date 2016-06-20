using System;
using System.Collections.Generic;
using System.Linq;
using Priority_Queue;

namespace aStar
{
	public class PathNode : PriorityQueueNode
	{
		/// <summary>
		/// Enter a node, receive a list of nodes connected to node and how much effort it is to travel to that node from input node
		/// </summary>
		public readonly static Dictionary<PathNode, List<Tuple<PathNode, float>>> ConnectedNodes = new Dictionary<PathNode, List<Tuple<PathNode, float>>>();

		public bool Enabled;
		public MapCell attachedObject;
		public float X { get; private set; }
		public float Y { get; private set; }

		public PathNode(MapCell mapCell, List<Tuple<PathNode, float>> connections, float x = 0, float y = 0, bool enabled = true)
		{
			ConnectedNodes.Add(this, connections ?? new List<Tuple<PathNode, float>>());
			Enabled = enabled;
			attachedObject = mapCell;
			X = x;
			Y = y;
		}

		//~PathNode()
		//{
		//	Console.Write("Destroyed");
		//	//ConnectedNodes.Remove(this);
		//}


	}
}
