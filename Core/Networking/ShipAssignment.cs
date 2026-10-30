namespace VectorRumble.Networking
{
    /// <summary>
    /// Gives each player of a network match a ship of their own. A ship can fly for only one player, so
    /// when two players chose the same one, the later player gets the next ship along instead. Every
    /// machine runs this over the same choices and gets the same answer.
    /// </summary>
    public static class ShipAssignment
    {
        /// <summary>
        /// The ship each player flies.
        /// </summary>
        /// <param name="choices">The ship each player chose, in slot order</param>
        /// <param name="shipCount">How many ships there are</param>
        public static int[] Assign(int[] choices, int shipCount)
        {
            int[] result = new int[choices.Length];
            bool[] taken = new bool[shipCount];

            for (int slot = 0; slot < choices.Length; slot++)
            {
                int ship = ((choices[slot] % shipCount) + shipCount) % shipCount;
                for (int tries = 0; tries < shipCount && taken[ship]; tries++)
                    ship = (ship + 1) % shipCount;

                result[slot] = ship;
                taken[ship] = true;
            }
            return result;
        }
    }
}
