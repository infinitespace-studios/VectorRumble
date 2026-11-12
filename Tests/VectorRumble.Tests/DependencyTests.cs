using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using Xunit;

namespace VectorRumble.Tests
{
    public class DependencyTests
    {
        [Fact]
        public void The_framework_the_game_ships_satisfies_the_networking_library()
        {
            // The networking library is built against one exact MonoGame.Framework build. If the game ships an
            // older one, the library fails to load ("Could not load file or assembly 'MonoGame.Framework,
            // Version=3.8.6.1") the first time a network session is made. Keep MonoGameVersion at or above it.
            var wanted = typeof(NetworkSession).Assembly.GetReferencedAssemblies()
                .First(a => a.Name == "MonoGame.Framework").Version;
            var shipped = typeof(Game).Assembly.GetName().Version;

            Assert.True(shipped >= wanted,
                "The game ships MonoGame.Framework " + shipped + " but the networking library needs " + wanted + ".");
        }
    }
}
