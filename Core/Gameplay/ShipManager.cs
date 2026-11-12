using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace VectorRumble
{
    internal class ShipManager 
    {
        const string shipDirectory = "Ships";
        private readonly World world;
        private Ship[] ships;
        public List<Ship> SelectedPlayers = new List<Ship>();

        /// <summary>
        /// How many of the ships come with the game (the rest are the player's own). Only these are the
        /// same on every machine, so only these can be chosen in a network match.
        /// </summary>
        public int BuiltInCount { get; private set; }

        public ShipManager(World world)
        {
            this.world = world;
        }

        public Ship[] AvailableShips => Ships.Where(s => !string.IsNullOrEmpty(s.PlayerIndex)).OrderBy(p => p.PlayerStringToIndex).ToArray();
        public Ship[] Players => Ships.Where(s => s.Playing).ToArray();
        public Ship[] Ships { get { return ships; } }

        // allocation-free versions of Players, for the code that runs every frame

        /// <summary>True when at least one ship is being played.</summary>
        public bool AnyPlaying
        {
            get
            {
                for (int i = 0; i < ships.Length; i++)
                    if (ships[i].Playing)
                        return true;
                return false;
            }
        }

        /// <summary>The first ship being played by the given player, or null.</summary>
        public Ship FirstPlayingFor(PlayerIndex index)
        {
            for (int i = 0; i < ships.Length; i++)
                if (ships[i].Playing && ships[i].PlayerStringToIndex == index)
                    return ships[i];
            return null;
        }

        /// <summary>The first selected ship belonging to the given player, or null.</summary>
        public Ship FirstSelectedFor(PlayerIndex index)
        {
            for (int i = 0; i < SelectedPlayers.Count; i++)
                if (SelectedPlayers[i].PlayerStringToIndex == index)
                    return SelectedPlayers[i];
            return null;
        }
        public Ship[] SpareShips => Ships.Where(s => string.IsNullOrEmpty(s.PlayerIndex)).ToArray();

        internal void LoadContent(ContentManager content)
        {
            var shipFiles = Helper.GetFilesFromContent(shipDirectory, "ships.txt");
            ships = new Ship[shipFiles.Length];
            for (int i = 0; i < shipFiles.Length; i++)
            {
                using (var stream = TitleContainer.OpenStream(shipFiles[i]))
                {
                    var ship = Ship.Load(XDocument.Load(stream).XPathSelectElement("/XnaContent/Asset"));
                    ship.World = world;
                    ships[i] = ship;
                }
            }

            BuiltInCount = ships.Length;

            // Once our app is signed, we can't change it, so let's look in the Special "MyDocuments" folder for user created data
            shipFiles = Helper.GetFilesFromFolders(new string[] { Helper.GetMyDocumentsFolder(), Helper.GetAssemblyTitle(), shipDirectory }, "*.xml");
            if (shipFiles != null && shipFiles.Length > 0)
            {
                var currentLength = ships.Length;
                Array.Resize(ref ships, currentLength + shipFiles.Length);
                for (int i = 0; i < shipFiles.Length; i++)
                {
                    var ship = Ship.Load(XDocument.Load(shipFiles[i]).XPathSelectElement("/XnaContent/Asset"));
                    ship.World = world;
                    ships[currentLength + i] = ship;
                }
            }
        }

        public void UpdateSelectedList()
        {
            SelectedPlayers.Clear();
            foreach (var ship in Ships)
            {
                if (ship.Selected)
                {
                    SelectedPlayers.Add(ship);
                }
            }
        }
    }
}
