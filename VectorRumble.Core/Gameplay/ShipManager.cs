using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Xna.Framework.Content;

namespace VectorRumble
{
    internal class ShipManager 
    {
        const string shipDirectory = "Ships";
        private readonly World world;
        private Ship[] ships;
        public List<Ship> SelectedPlayers = new List<Ship>();

        // Cached arrays for performance
        private Ship[] cachedAvailableShips;
        private Ship[] cachedPlayers;
        private Ship[] cachedSpareShips;
        private bool cachesDirty = true;

        public ShipManager(World world)
        {
            this.world = world;
        }

        public Ship[] AvailableShips 
        {
            get 
            {
                if (cachesDirty)
                    UpdateCaches();
                return cachedAvailableShips;
            }
        }

        public Ship[] Players 
        {
            get 
            {
                if (cachesDirty)
                    UpdateCaches();
                return cachedPlayers;
            }
        }

        public Ship[] Ships { get { return ships; } }
        
        public Ship[] SpareShips 
        {
            get 
            {
                if (cachesDirty)
                    UpdateCaches();
                return cachedSpareShips;
            }
        }

        private void UpdateCaches()
        {
            cachedAvailableShips = Ships.Where(s => !string.IsNullOrEmpty(s.PlayerIndex)).OrderBy(p => p.PlayerStringToIndex).ToArray();
            cachedPlayers = Ships.Where(s => s.Playing).ToArray();
            cachedSpareShips = Ships.Where(s => string.IsNullOrEmpty(s.PlayerIndex)).ToArray();
            cachesDirty = false;
        }

        public void InvalidateCaches()
        {
            cachesDirty = true;
        }

        internal void LoadContent(ContentManager content)
        {
            var shipFiles = Helper.GetFilesFromFolders(new string[] { content.RootDirectory, shipDirectory }, "*.xml");
            ships = new Ship[shipFiles.Length];
            for (int i = 0; i < shipFiles.Length; i++)
            {
                var ship = Ship.Load(XDocument.Load(shipFiles[i]).XPathSelectElement("/XnaContent/Asset"));
                ship.World = world;
                ships[i] = ship;
            }

            // Once our app is signed, we can't change it, so let's look in the Special "MyDocuments" folder for user created data
            shipFiles = Helper.GetFilesFromFolders(new string[] { Helper.GetMyDocumentsFolder(), Helper.GetAssemblyTitle(), shipDirectory }, "*.xml");
            if (shipFiles != null && shipFiles.Length > 0)
            {
                Array.Resize(ref ships, ships.Length + shipFiles.Length);
                for (int i = ships.Length; i < ships.Length + shipFiles.Length; i++)
                {
                    var ship = Ship.Load(XDocument.Load(shipFiles[i]).XPathSelectElement("/XnaContent/Asset"));
                    ship.World = world;
                    ships[i] = ship;
                }
            }
            InvalidateCaches();
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
            InvalidateCaches();
        }
    }
}
