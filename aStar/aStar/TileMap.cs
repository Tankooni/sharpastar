using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Indigo;
using Indigo.Graphics;

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

		const int TileSize = 32;

		public TileMap()
		{
			for (int y = 0; y < MapHeight; y++)
			{
				MapRow row = new MapRow();
				for (int x = 0; x < MapWidth; x++)
				{
					MapCell mc = new MapCell(0, TileSize)
					{
						X = x * TileSize,
						Y = y * TileSize
					};
					this.AddComponent<Image>(mc);
					row.Columns.Add(mc);
				}
				Rows.Add(row);
			}
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
