namespace VectorRumble.Networking
{
    /// <summary>
    /// The scoring rules of a match, kept apart from the game so the host's decisions can be tested.
    /// </summary>
    public static class MatchScoring
    {
        /// <summary>
        /// A ship was destroyed. Whoever shot it scores a point; a ship that blew itself up, or was
        /// destroyed by something that nobody fired (an asteroid), loses one.
        /// </summary>
        /// <param name="scores">The scores by player slot, changed in place.</param>
        /// <param name="attacker">The slot of the player who fired the shot, or <see cref="DamagePacket.NoAttacker"/>.</param>
        /// <param name="defender">The slot of the ship that was destroyed.</param>
        public static void ApplyKill(short[] scores, int attacker, int defender)
        {
            if (defender < 0 || defender >= scores.Length)
                return;

            if (attacker == defender || attacker < 0 || attacker >= scores.Length)
                scores[defender]--;
            else
                scores[attacker]++;
        }

        /// <summary>
        /// The first player at the score limit, or -1 if nobody is.
        /// </summary>
        public static int PlayerAtLimit(short[] scores, int limit)
        {
            for (int i = 0; i < scores.Length; i++)
                if (scores[i] >= limit)
                    return i;
            return -1;
        }
    }
}
