using System;
using Indigo;
using Indigo.Inputs;
using Indigo.Graphics;

namespace hexStar
{
	public class Game : Engine
	{
		public static float Lol = 10;

		public static void Main(string[] args)
		{
			var game = new Game();
			game.Run();
		}

		public Game() :
			base(1136, 641, 60)
		{
			FP.Console.Enable();
			FP.Screen.ClearColor = new Color(0x000000);
			Mouse.CursorVisible = true;

			FP.World = new TiledWorld(this);
		}

		public override void FocusLost()
		{
			base.FocusLost();
			this.Paused = true;
		}

		public override void FocusGained()
		{
			base.FocusGained();
			if (!FP.Console.IsOpen)
				this.Paused = false;
		}
	}
}
