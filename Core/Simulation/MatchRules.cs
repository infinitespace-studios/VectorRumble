using System;
using System.IO;

namespace VectorRumble
{
    /// <summary>
    /// The rules of one match. Every player in a match plays by the same rules, so these
    /// are chosen on the game setup screen (by the host in a network game) and never
    /// depend on one machine's own settings, which live in <see cref="WorldRules"/>.
    /// </summary>
    public class MatchRules
    {
        public const int MinScoreLimit = 5;
        public const int MaxScoreLimit = 25;

        /// <summary>The rules for the next match; the game setup screen edits them.</summary>
        public static MatchRules Current { get; set; } = new MatchRules();

        public AsteroidDensity AsteroidDensity = AsteroidDensity.Low;
        public bool ControllersCanShootInAllDirections;

        /// <summary>An arena's name, or <see cref="Strings.Arena_Random"/> to have one picked for the match.</summary>
        public string Arena = Strings.Arena_Random;
        public int ScoreLimit = 10;

        public bool IsRandomArena => Arena == Strings.Arena_Random;

        public MatchRules Clone() => (MatchRules)MemberwiseClone();

        /// <summary>
        /// Writes the rules for the network. "Random" is written as an empty name, because the
        /// word is translated and so differs between machines.
        /// </summary>
        public void Write(BinaryWriter writer)
        {
            writer.Write(IsRandomArena ? string.Empty : Arena);
            writer.Write((byte)AsteroidDensity);
            writer.Write((byte)ScoreLimit);
            writer.Write(ControllersCanShootInAllDirections);
        }

        /// <summary>
        /// Reads rules written by <see cref="Write"/>. Throws <see cref="InvalidDataException"/> for values
        /// no game screen can produce, so a damaged packet is dropped rather than played.
        /// </summary>
        public static MatchRules Read(BinaryReader reader)
        {
            string arena = reader.ReadString();
            byte density = reader.ReadByte();
            byte scoreLimit = reader.ReadByte();
            bool shootInAllDirections = reader.ReadBoolean();

            if (!Enum.IsDefined(typeof(AsteroidDensity), (int)density))
                throw new InvalidDataException("Unknown asteroid density " + density);
            if (scoreLimit < MinScoreLimit || scoreLimit > MaxScoreLimit)
                throw new InvalidDataException("Score limit " + scoreLimit + " out of range");

            return new MatchRules
            {
                Arena = arena.Length == 0 ? Strings.Arena_Random : arena,
                AsteroidDensity = (AsteroidDensity)density,
                ScoreLimit = scoreLimit,
                ControllersCanShootInAllDirections = shootInAllDirections,
            };
        }
    }
}
