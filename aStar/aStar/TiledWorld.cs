﻿using Indigo;
using Indigo.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aStar
{
	public class TiledWorld : World
	{
		private TileMap tiles = new TileMap();
		public Game game;

		public TiledWorld(Game game)
		{
			this.Add(tiles);
			this.game = game;
		}

		public override void Update()
		{
			base.Update();
			if (Keyboard.Up.Down)
				Camera.Y -= 100 * FP.Elapsed;
			if (Keyboard.Down.Down)
				Camera.Y += 100 * FP.Elapsed;
			if (Keyboard.Left.Down)
				Camera.X -= 100 * FP.Elapsed;
			if (Keyboard.Right.Down)
				Camera.X += 100 * FP.Elapsed;
			if (Keyboard.R.Pressed)
			{
				this.Remove(tiles);
				PathNode.ConnectedNodes.Clear();
				this.Add(tiles = new TileMap());
			}
		}
	}
}
