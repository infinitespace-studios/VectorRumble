using System;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace VectorRumble
{
    internal class ArenaManager
    {
        const string arenaDirectory = "Arenas";
        private readonly World world;

        private Arena[] arenas;
        public Arena[] Arenas { get { return arenas; } }

        /// <summary>
        /// The arenas that come with the game, and "Random" after them (the rest are the player's
        /// own). Only these are the same on every machine, so only these can be played in a network match.
        /// </summary>
        public Arena[] BuiltInArenas => arenas.Take(builtInCount).ToArray();

        private int builtInCount;

        public ArenaManager(World world)
        {
            this.world = world;
        }

        internal void LoadContent(ContentManager content)
        {
            var arenaFiles = Helper.GetFilesFromContent(arenaDirectory, "arenas.txt");
            var arenaLength = arenaFiles.Length; // Caching the value so that the property isn't accessed throughout the loop.

            arenas = new Arena[arenaLength + 1]; // Adding an extra slot for the Random Arena later.
            for (int i = 0; i < arenaLength; i++)
            {
                using (var stream = TitleContainer.OpenStream(arenaFiles[i]))
                {
                    var arena = Arena.Load(XDocument.Load(stream).XPathSelectElement("/XnaContent/Asset"));
                    arenas[i] = arena;
                }
            }

            // Add an extra element for our Random option.
            var randomArena = new Arena
            {
                Name = Strings.Arena_Random,
            };
            arenas[arenas.Length - 1] = randomArena;
            builtInCount = arenas.Length;

            // Once our app is signed, we can't change it, so let's look in the Special "MyDocuments" folder for user created data
            arenaFiles = Helper.GetFilesFromFolders(new string[] { Helper.GetMyDocumentsFolder(), Helper.GetAssemblyTitle(), arenaDirectory }, "*.xml");
            if (arenaFiles != null && arenaFiles.Length > 0)
            {
                var currentLength = arenas.Length;
                Array.Resize(ref arenas, currentLength + arenaFiles.Length);
                for (int i = 0; i < arenaFiles.Length; i++)
                {
                    var arena = Arena.Load(XDocument.Load(arenaFiles[i]).XPathSelectElement("/XnaContent/Asset"));
                    arenas[currentLength + i] = arena;
                }
            }
        }

        internal Arena Random()
        {
            int length = Arenas.Length;
            var index = ParticleSystem.random.Next(length);
            return Arenas[index];
        }
    }
}
