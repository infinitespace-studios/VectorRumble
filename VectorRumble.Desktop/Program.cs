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
#if DEBUG // Purely for testing purposes
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("es-ES");
            System.Globalization.CultureInfo.CurrentUICulture = new System.Globalization.CultureInfo("es-ES");
#endif

            using (var game = new VectorRumbleGame ()) {
				game.Run ();
			}
		}
	}
}

