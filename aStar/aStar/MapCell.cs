using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Indigo;
using Indigo.Graphics;

namespace aStar
{
	public class MapCell : Image
	{
		public TileType TileType { get; private set; }
		public int CellSize { get; private set; }

		public MapCell(TileType tileType, int cellSize)
			: base(Library.GetTexture("content/white.png")/*Image.CreateRect(cellSize, cellSize, new Color(0x000000))*/)
		{
			TileType = tileType;
			CellSize = cellSize;
			Color = SelectTileColor(tileType);
			this.Scale = CellSize / (float)this.Height;
		}

		public TileType ChangeTileType(TileType tileType)
		{
			Color = SelectTileColor(tileType);
			return TileType = tileType;
		}

		public static Color SelectTileColor(TileType tileType)
		{
			switch (tileType)
			{
				default: //wat?
					return new Color(0xff00ff);
				case TileType.End: //end
					return new Color(0xff0000);
				case TileType.Start: //start
					return new Color(0x00ff00);
				case TileType.Regular: //reg
					return new Color(0x00ffff);
				case TileType.Wall: //wall
					return new Color(0x0a0069);
				case TileType.Path: //path
					return new Color(0xe69609);
			}
		}

		public static Dictionary<TileType, float> TileSpeeds = new Dictionary<TileType, float>
		{
			{TileType.Regular, 1},
			{TileType.Wall, 0},
			{TileType.Path, 1},
			{TileType.Start, 1},
			{TileType.End, 1}
		};
	}

	public enum TileType
	{
		Start,
		End,
		Path,
		Regular,
		Wall
	}
}
