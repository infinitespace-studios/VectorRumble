using System;
using System.Collections.Generic;
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
        
        // Cached arrays to avoid creating garbage on each access
        private Ship[] cachedAvailableShips;
        private Ship[] cachedPlayers;
        private Ship[] cachedSpareShips;
        private bool availableShipsCacheDirty = true;
        private bool playersCacheDirty = true;
        private bool spareShipsCacheDirty = true;
        
        // Static comparer to avoid delegate allocation
        private static readonly Comparison<Ship> playerIndexComparison = 
            (a, b) => a.PlayerStringToIndex.CompareTo(b.PlayerStringToIndex);

        public ShipManager(World world)
        {
            this.world = world;
        }

        public Ship[] AvailableShips 
        { 
            get 
            {
                if (availableShipsCacheDirty)
                {
                    RebuildAvailableShipsCache();
                }
                return cachedAvailableShips;
            }
        }
        
        public Ship[] Players 
        { 
            get 
            {
                if (playersCacheDirty)
                {
                    RebuildPlayersCache();
                }
                return cachedPlayers;
            }
        }
        
        public Ship[] Ships { get { return ships; } }
        
        public Ship[] SpareShips 
        { 
            get 
            {
                if (spareShipsCacheDirty)
                {
                    RebuildSpareShipsCache();
                }
                return cachedSpareShips;
            }
        }
        
        private void RebuildAvailableShipsCache()
        {
            // Count ships with PlayerIndex set
            int count = 0;
            for (int i = 0; i < ships.Length; i++)
            {
                if (!string.IsNullOrEmpty(ships[i].PlayerIndex))
                {
                    count++;
                }
            }
            
            // Create or resize cache
            if (cachedAvailableShips == null || cachedAvailableShips.Length != count)
            {
                cachedAvailableShips = new Ship[count];
            }
            
            // Fill cache
            int index = 0;
            for (int i = 0; i < ships.Length; i++)
            {
                if (!string.IsNullOrEmpty(ships[i].PlayerIndex))
                {
                    cachedAvailableShips[index++] = ships[i];
                }
            }
            
            // Sort by PlayerStringToIndex using Array.Sort with static comparer
            Array.Sort(cachedAvailableShips, playerIndexComparison);
            
            availableShipsCacheDirty = false;
        }
        
        private void RebuildPlayersCache()
        {
            // Count playing ships
            int count = 0;
            for (int i = 0; i < ships.Length; i++)
            {
                if (ships[i].Playing)
                {
                    count++;
                }
            }
            
            // Create or resize cache
            if (cachedPlayers == null || cachedPlayers.Length != count)
            {
                cachedPlayers = new Ship[count];
            }
            
            // Fill cache
            int index = 0;
            for (int i = 0; i < ships.Length; i++)
            {
                if (ships[i].Playing)
                {
                    cachedPlayers[index++] = ships[i];
                }
            }
            
            playersCacheDirty = false;
        }
        
        private void RebuildSpareShipsCache()
        {
            // Count spare ships
            int count = 0;
            for (int i = 0; i < ships.Length; i++)
            {
                if (string.IsNullOrEmpty(ships[i].PlayerIndex))
                {
                    count++;
                }
            }
            
            // Create or resize cache
            if (cachedSpareShips == null || cachedSpareShips.Length != count)
            {
                cachedSpareShips = new Ship[count];
            }
            
            // Fill cache
            int index = 0;
            for (int i = 0; i < ships.Length; i++)
            {
                if (string.IsNullOrEmpty(ships[i].PlayerIndex))
                {
                    cachedSpareShips[index++] = ships[i];
                }
            }
            
            spareShipsCacheDirty = false;
        }
        
        public void InvalidateCaches()
        {
            availableShipsCacheDirty = true;
            playersCacheDirty = true;
            spareShipsCacheDirty = true;
        }
        
        public void InvalidatePlayersCache()
        {
            playersCacheDirty = true;
        }
        
        public void InvalidateAvailableAndSpareShipsCache()
        {
            availableShipsCacheDirty = true;
            spareShipsCacheDirty = true;
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
            // When selection changes, invalidate caches
            InvalidateCaches();
        }
    }
}
