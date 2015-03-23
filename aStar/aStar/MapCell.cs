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
		public int TileId { get; private set; }
		public int CellSize { get; private set; }

		public MapCell(int tileId, int cellSize)
			: base(Library.GetTexture("content/white.png")/*Image.CreateRect(cellSize, cellSize, new Color(0x000000))*/)
		{
			TileId = tileId;
			CellSize = cellSize;
			Color = SelectTileColor(tileId);
			this.Scale = CellSize / (float)this.Height;
		}

		public static Color SelectTileColor(int tileID)
		{
			switch (tileID)
			{
				default: //wat?
					return new Color(0xff00ff);
				case -2: //end
					return new Color(0xff00ff);
				case -1: //start
					return new Color(0xff00ff);
				case 0: //reg
					return new Color(0x00ffff);
				case 1: //wall
					return new Color(0xff00ff);
				case 2: //path
					return new Color(0xff00ff);
			}
		}
	}
}
