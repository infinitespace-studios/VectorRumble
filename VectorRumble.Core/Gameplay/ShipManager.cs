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

        public ShipManager(World world)
        {
            this.world = world;
        }

        public Ship[] AvailableShips => Ships.Where(s => !string.IsNullOrEmpty(s.PlayerIndex)).OrderBy(p => p.PlayerStringToIndex).ToArray();
        public Ship[] Players => Ships.Where(s => s.Playing).ToArray();
        public Ship[] Ships { get { return ships; } }
        public Ship[] SpareShips => Ships.Where(s => string.IsNullOrEmpty(s.PlayerIndex)).ToArray();

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
        }
    }
}
