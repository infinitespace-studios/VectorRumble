using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace VectorRumble.Networking
{
    /// <summary>
    /// Connectivity test: sent to a peer, who answers with a <see cref="PongPacket"/> carrying the same token
    /// </summary>
    public class PingPacket : INetworkPacket
    {
        public PacketType Type { get { return PacketType.Ping; } }
        public uint Token { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Token);
        }

        public static PingPacket Deserialize(BinaryReader reader)
        {
            return new PingPacket { Token = reader.ReadUInt32() };
        }
    }

    public class PongPacket : INetworkPacket
    {
        public PacketType Type { get { return PacketType.Pong; } }
        public uint Token { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Token);
        }

        public static PongPacket Deserialize(BinaryReader reader)
        {
            return new PongPacket { Token = reader.ReadUInt32() };
        }
    }

    /// <summary>
    /// One player in the lobby or in a starting match
    /// </summary>
    public class LobbyPlayerInfo
    {
        public string Id { get; set; }          // network gamer id (unique per machine)
        public string Name { get; set; }
        public byte Ship { get; set; }          // index into the game's list of ships
        public Color Color { get; set; }
        public bool Ready { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(Name);
            writer.Write(Ship);
            writer.Write(Color);
            writer.Write(Ready);
        }

        public static LobbyPlayerInfo Deserialize(BinaryReader reader)
        {
            return new LobbyPlayerInfo
            {
                Id = reader.ReadString(),
                Name = reader.ReadString(),
                Ship = reader.ReadByte(),
                Color = reader.ReadColor(),
                Ready = reader.ReadBoolean(),
            };
        }

        public static void WriteList(BinaryWriter writer, List<LobbyPlayerInfo> players)
        {
            writer.Write((byte)players.Count);
            foreach (LobbyPlayerInfo player in players)
                player.Serialize(writer);
        }

        public static List<LobbyPlayerInfo> ReadList(BinaryReader reader)
        {
            int count = reader.ReadByte();
            List<LobbyPlayerInfo> players = new List<LobbyPlayerInfo>(count);
            for (int i = 0; i < count; i++)
                players.Add(Deserialize(reader));
            return players;
        }
    }

    /// <summary>
    /// A client tells the host about its own ship choice and ready state
    /// </summary>
    public class LobbyUpdatePacket : INetworkPacket
    {
        public PacketType Type { get { return PacketType.LobbyUpdate; } }
        public byte Ship { get; set; }
        public Color Color { get; set; }
        public bool Ready { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Ship);
            writer.Write(Color);
            writer.Write(Ready);
        }

        public static LobbyUpdatePacket Deserialize(BinaryReader reader)
        {
            return new LobbyUpdatePacket { Ship = reader.ReadByte(), Color = reader.ReadColor(), Ready = reader.ReadBoolean() };
        }
    }

    /// <summary>
    /// The host tells everybody the rules and who is in the lobby (in slot order, host first)
    /// </summary>
    public class LobbySyncPacket : INetworkPacket
    {
        public PacketType Type { get { return PacketType.LobbySync; } }
        public MatchRules Rules { get; set; }
        public List<LobbyPlayerInfo> Players { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            Rules.Write(writer);
            LobbyPlayerInfo.WriteList(writer, Players);
        }

        public static LobbySyncPacket Deserialize(BinaryReader reader)
        {
            return new LobbySyncPacket { Rules = MatchRules.Read(reader), Players = LobbyPlayerInfo.ReadList(reader) };
        }
    }

    /// <summary>
    /// The host starts the match. Slot order is the player index for the whole match.
    /// The host has already picked the arena if the rules said "Random".
    /// </summary>
    public class MatchStartPacket : INetworkPacket
    {
        public PacketType Type { get { return PacketType.MatchStart; } }
        public MatchRules Rules { get; set; }
        public int Seed { get; set; }                       // for anything every machine must random the same way
        public List<LobbyPlayerInfo> Players { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            Rules.Write(writer);
            writer.Write(Seed);
            LobbyPlayerInfo.WriteList(writer, Players);
        }

        public static MatchStartPacket Deserialize(BinaryReader reader)
        {
            return new MatchStartPacket
            {
                Rules = MatchRules.Read(reader),
                Seed = reader.ReadInt32(),
                Players = LobbyPlayerInfo.ReadList(reader),
            };
        }
    }

    /// <summary>
    /// Where a ship is and how it moves. Each machine simulates its own ship and sends
    /// this about 20 times a second, unreliably; the host passes it on to the other
    /// clients. Only the latest state matters, so a lost packet is never resent.
    /// </summary>
    public class ShipStatePacket : INetworkPacket
    {
        public PacketType Type { get { return PacketType.ShipState; } }
        public byte Slot { get; set; }                  // player index in the match
        public ushort Sequence { get; set; }            // wraps; orders unreliable packets
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }             // radians
        public Vector2 Velocity { get; set; }           // units per second
        public byte Shield { get; set; }                // 0 (none) to 100 (full)

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Slot);
            writer.Write(Sequence);
            writer.Write(Position);
            writer.Write(Rotation);
            writer.Write(Velocity);
            writer.Write(Shield);
        }

        public static ShipStatePacket Deserialize(BinaryReader reader)
        {
            return new ShipStatePacket
            {
                Slot = reader.ReadByte(),
                Sequence = reader.ReadUInt16(),
                Position = reader.ReadVector2(),
                Rotation = reader.ReadSingle(),
                Velocity = reader.ReadVector2(),
                Shield = reader.ReadByte(),
            };
        }
    }

    /// <summary>
    /// A ship fired. Sent reliably by the machine that owns the ship; every machine
    /// then makes the same shots. Only the host turns hits into damage.
    /// </summary>
    public class ProjectileFiredPacket : INetworkPacket
    {
        public PacketType Type { get { return PacketType.ProjectileFired; } }
        public byte Slot { get; set; }
        public byte Weapon { get; set; }                // a WeaponKind
        public Vector2 Direction { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Slot);
            writer.Write(Weapon);
            writer.Write(Direction);
        }

        public static ProjectileFiredPacket Deserialize(BinaryReader reader)
        {
            return new ProjectileFiredPacket
            {
                Slot = reader.ReadByte(),
                Weapon = reader.ReadByte(),
                Direction = reader.ReadVector2(),
            };
        }
    }

    /// <summary>
    /// The host tells the owner of a ship that it was hit
    /// </summary>
    public class DamagePacket : INetworkPacket
    {
        /// <summary>The attacker when nobody fired the shot (an asteroid, for instance)</summary>
        public const byte NoAttacker = 255;

        public PacketType Type { get { return PacketType.Damage; } }
        public byte Attacker { get; set; }
        public byte Defender { get; set; }
        public float Amount { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Attacker);
            writer.Write(Defender);
            writer.Write(Amount);
        }

        public static DamagePacket Deserialize(BinaryReader reader)
        {
            return new DamagePacket
            {
                Attacker = reader.ReadByte(),
                Defender = reader.ReadByte(),
                Amount = reader.ReadSingle(),
            };
        }
    }

    /// <summary>
    /// A ship exploded. The machine that flies it decides when, and tells the host, who scores it
    /// and tells everybody else.
    /// </summary>
    public class ShipDestroyedPacket : INetworkPacket
    {
        public PacketType Type { get { return PacketType.ShipDestroyed; } }
        public byte Attacker { get; set; }              // DamagePacket.NoAttacker if nobody
        public byte Defender { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Attacker);
            writer.Write(Defender);
        }

        public static ShipDestroyedPacket Deserialize(BinaryReader reader)
        {
            return new ShipDestroyedPacket { Attacker = reader.ReadByte(), Defender = reader.ReadByte() };
        }
    }

    /// <summary>
    /// The host tells everybody the scores, by player slot
    /// </summary>
    public class ScoreSyncPacket : INetworkPacket
    {
        public PacketType Type { get { return PacketType.ScoreSync; } }
        public short[] Scores { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((byte)Scores.Length);
            foreach (short score in Scores)
                writer.Write(score);
        }

        public static ScoreSyncPacket Deserialize(BinaryReader reader)
        {
            int count = reader.ReadByte();
            if (count > MultiplayerSessionService.MaxPlayers)
                throw new InvalidDataException("Scores for " + count + " players");

            short[] scores = new short[count];
            for (int i = 0; i < count; i++)
                scores[i] = reader.ReadInt16();
            return new ScoreSyncPacket { Scores = scores };
        }
    }

    /// <summary>
    /// The host tells everybody a power-up appeared. The host names it, so that it can say who took it.
    /// </summary>
    public class PowerupSpawnedPacket : INetworkPacket
    {
        public PacketType Type { get { return PacketType.PowerupSpawned; } }
        public ushort Id { get; set; }
        public byte Kind { get; set; }                  // a PowerUpKind
        public Vector2 Position { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(Kind);
            writer.Write(Position);
        }

        public static PowerupSpawnedPacket Deserialize(BinaryReader reader)
        {
            return new PowerupSpawnedPacket { Id = reader.ReadUInt16(), Kind = reader.ReadByte(), Position = reader.ReadVector2() };
        }
    }

    /// <summary>
    /// The host tells everybody which player picked up a power-up
    /// </summary>
    public class PowerupTakenPacket : INetworkPacket
    {
        public PacketType Type { get { return PacketType.PowerupTaken; } }
        public ushort Id { get; set; }
        public byte Player { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(Player);
        }

        public static PowerupTakenPacket Deserialize(BinaryReader reader)
        {
            return new PowerupTakenPacket { Id = reader.ReadUInt16(), Player = reader.ReadByte() };
        }
    }

    /// <summary>
    /// The host tells everybody who won
    /// </summary>
    public class MatchEndPacket : INetworkPacket
    {
        public PacketType Type { get { return PacketType.MatchEnd; } }
        public byte Winner { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Winner);
        }

        public static MatchEndPacket Deserialize(BinaryReader reader)
        {
            return new MatchEndPacket { Winner = reader.ReadByte() };
        }
    }

    /// <summary>
    /// The host tells everybody a player left the match
    /// </summary>
    public class PlayerLeftPacket : INetworkPacket
    {
        public PacketType Type { get { return PacketType.PlayerLeft; } }
        public byte Slot { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Slot);
        }

        public static PlayerLeftPacket Deserialize(BinaryReader reader)
        {
            return new PlayerLeftPacket { Slot = reader.ReadByte() };
        }
    }

    /// <summary>
    /// Where the host's asteroids are, in the order they were made (the same on every machine).
    /// Sent unreliably a few times a second; only the latest matters.
    /// </summary>
    public class AsteroidSyncPacket : INetworkPacket
    {
        public const int MaxAsteroids = 64;

        public PacketType Type { get { return PacketType.AsteroidSync; } }
        public ushort Sequence { get; set; }
        public Vector2[] Positions { get; set; }
        public Vector2[] Velocities { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Sequence);
            writer.Write((byte)Positions.Length);
            for (int i = 0; i < Positions.Length; i++)
            {
                writer.Write(Positions[i]);
                writer.Write(Velocities[i]);
            }
        }

        public static AsteroidSyncPacket Deserialize(BinaryReader reader)
        {
            ushort sequence = reader.ReadUInt16();
            int count = reader.ReadByte();
            if (count > MaxAsteroids)
                throw new InvalidDataException(count + " asteroids");

            var positions = new Vector2[count];
            var velocities = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                positions[i] = reader.ReadVector2();
                velocities[i] = reader.ReadVector2();
            }
            return new AsteroidSyncPacket { Sequence = sequence, Positions = positions, Velocities = velocities };
        }
    }

    /// <summary>
    /// The host tells a latecomer that the match has already started
    /// </summary>
    public class JoinRefusedPacket : INetworkPacket
    {
        public PacketType Type { get { return PacketType.JoinRefused; } }

        public void Serialize(BinaryWriter writer)
        {
        }

        public static JoinRefusedPacket Deserialize(BinaryReader reader)
        {
            return new JoinRefusedPacket();
        }
    }
}
