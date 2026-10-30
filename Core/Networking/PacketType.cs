namespace VectorRumble.Networking
{
    /// <summary>
    /// Gameplay packet types. The first byte of every payload.
    /// 
    /// The Net library treats a datagram whose first byte is a registered framework
    /// message id (currently 1..10) as one of its own messages, so gameplay ids start
    /// well above that range. The library also wraps every datagram in a reliable or
    /// unreliable channel whose first byte (0x21..0x23) stays clear of the framework ids.
    /// </summary>
    public enum PacketType : byte
    {
        // connectivity test / latency measurement
        Ping = 64,
        Pong = 65,

        // lobby and match setup
        LobbyUpdate = 70,       // client -> host: my ship choice and ready state
        LobbySync = 71,         // host -> clients: the whole lobby
        MatchStart = 72,        // host -> all: the match begins

        // match play
        ShipState = 80,         // unreliable, ~20Hz: where a ship is and how it moves
        ProjectileFired = 81,   // a ship fired: every machine makes the same shots (the host does the damage)
        Damage = 82,            // host -> ship owner: you were hit
        ShipDestroyed = 83,     // ship owner -> host -> others: this ship exploded
        ScoreSync = 84,         // host -> all: the scores
        PowerupTaken = 85,      // host -> all: someone picked up a power-up
        MatchEnd = 86,          // host -> all: somebody won
        PlayerLeft = 87,        // host -> all: a player left the match
        JoinRefused = 88,       // host -> a latecomer: the match has already started
        PowerupSpawned = 89,    // host -> all: a power-up appeared
        AsteroidSync = 90,      // unreliable, ~5Hz, host -> clients: where the asteroids are
    }
}
