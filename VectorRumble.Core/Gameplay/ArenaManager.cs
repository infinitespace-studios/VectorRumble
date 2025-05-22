﻿using System;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Xna.Framework.Content;

namespace VectorRumble
{
    internal class ArenaManager
    {
        const string arenaDirectory = "Arenas";
        private readonly World world;

        private Arena[] arenas;
        public Arena[] Arenas { get { return arenas; } }

        public ArenaManager(World world)
        {
            this.world = world;
        }

        internal void LoadContent(ContentManager content)
        {
            var arenaFiles = Helper.GetFilesFromFolders(new string[] { content.RootDirectory, arenaDirectory }, "*.xml");
            var arenaLength = arenaFiles.Length; // Caching the value so that the property isn't accessed throughout the loop.

            arenas = new Arena[arenaLength + 1]; // Adding an extra slot for the Random Arena later.
            for (int i = 0; i < arenaLength; i++)
            {
                var arena = Arena.Load(XDocument.Load(arenaFiles[i]).XPathSelectElement("/XnaContent/Asset"));
                arenas[i] = arena;
            }

            // Add an extra element for our Random option.
            var randomArena = new Arena
            {
                Name = Strings.Arena_Random,
            };
            arenas[arenas.Length - 1] = randomArena;

            // Once our app is signed, we can't change it, so let's look in the Special "MyDocuements" folder for user created data
            arenaFiles = Helper.GetFilesFromFolders(new string[] { Helper.GetMyDocumentsFolder(), Helper.GetAssemblyTitle(), arenaDirectory }, "*.xml");
            if (arenaFiles != null && arenaFiles.Length > 0)
            {
                Array.Resize(ref arenas, arenas.Length + arenaFiles.Length);
                for (int i = arenas.Length; i < arenas.Length + arenaFiles.Length; i++)
                {
                    var arena = Arena.Load(XDocument.Load(arenaFiles[i]).XPathSelectElement("/XnaContent/Asset"));
                    arenas[i] = arena;
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
