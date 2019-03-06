#region Using Statements
using System;
#endregion

namespace VectorRumble.Desktop
{
	static class Program
	{
		[STAThread]
		static void Main (string [] args)
		{
			using (var game = new VectorRumbleGame ()) {
				game.Run ();
			}
		}
	}
}

