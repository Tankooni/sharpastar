using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Indigo;
using Indigo.Graphics;
using Priority_Queue;

namespace hexStar
{
	public class MapCell : Image
	{
		public TileType TileType { get; private set; }
		public int CellSize { get; private set; }
		public Tuple<int, int> Index { get; private set; }
		public PathNode MyNode { get; private set; }

		public MapCell(TileType tileType, int cellSize, Tuple<int, int> index)
			: this(tileType, cellSize, index, null)
		{
		}
		public MapCell(TileType tileType, int cellSize, Tuple<int, int> index, PathNode pathNode)
			: base(Library.GetTexture("content/baseTile.png")/*Image.CreateRect(cellSize, cellSize, new Color(0x000000))*/)
		{
			TileType = tileType;
			CellSize = cellSize;
			Color = SelectTileColor(tileType);
			//Scale = CellSize / (float)this.Height;
			Index = index;
			MyNode = pathNode;
		}

		public TileType ChangeTileType(TileType tileType)
		{
			Color = SelectTileColor(tileType);
			return TileType = tileType;
		}

		public PathNode SetPathNode(PathNode pathNode)
		{
			return MyNode = pathNode;
		}

		public static Color SelectTileColor(TileType tileType)
		{
			switch (tileType)
			{
				default: //wat?
					return new Color(0xff00ff);
				case TileType.End:
					return new Color(0xff0000);
				case TileType.Start:
					return new Color(0x00ff00);
				case TileType.Regular:
					return new Color(0x00ffff);
				case TileType.Wall:
					return new Color(0x0a0069);
				case TileType.Path:
					return new Color(0xe69609);
				case TileType.Considered:
					return new Color(0xff00ff);
			}
		}

		public static Dictionary<TileType, float> TileSpeeds = new Dictionary<TileType, float>
		{
			{TileType.Regular, 1},
			{TileType.Wall, 0},
			{TileType.Path, 1},
			{TileType.Start, 1},
			{TileType.End, 1},
			{TileType.Considered, 1}
		};
	}

	public enum TileType
	{
		Start,
		End,
		Path,
		Regular,
		Wall,
		Considered
	}
}
