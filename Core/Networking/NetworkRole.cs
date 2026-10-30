namespace VectorRumble.Networking
{
    /// <summary>
    /// The role of this machine in a match
    /// </summary>
    public enum NetworkRole
    {
        None = 0,                // not a networked match
        Host,                    // hosts the match (decides combat, score, pickups)
        Client                   // joined a match hosted by another machine
    }
}
