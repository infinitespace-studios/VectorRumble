using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using VectorRumble.Networking;
using Xunit;

namespace VectorRumble.Tests
{
    public class PacketTests
    {
        static byte[] Write(INetworkPacket packet)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                packet.Serialize(writer);
                writer.Flush();
                return stream.ToArray();
            }
        }

        static BinaryReader Reader(byte[] bytes)
        {
            return new BinaryReader(new MemoryStream(bytes));
        }

        static List<LobbyPlayerInfo> SomePlayers()
        {
            return new List<LobbyPlayerInfo>
            {
                new LobbyPlayerInfo { Id = "a", Name = "Ann", Ship = 0, Color = new Color(10, 20, 30), Ready = true },
                new LobbyPlayerInfo { Id = "b", Name = "Bob", Ship = 1, Color = Color.Red, Ready = false },
            };
        }

        static MatchRules SomeRules()
        {
            return new MatchRules
            {
                Arena = "Box",
                AsteroidDensity = AsteroidDensity.High,
                ScoreLimit = 15,
                ControllersCanShootInAllDirections = true,
            };
        }

        [Fact]
        public void Ping_and_pong_keep_their_token()
        {
            Assert.Equal(0xDEADBEEFu, PingPacket.Deserialize(Reader(Write(new PingPacket { Token = 0xDEADBEEF }))).Token);
            Assert.Equal(7u, PongPacket.Deserialize(Reader(Write(new PongPacket { Token = 7 }))).Token);
        }

        [Fact]
        public void Packet_types_match_what_the_dispatcher_registers()
        {
            Assert.Equal(PacketType.Ping, new PingPacket().Type);
            Assert.Equal(PacketType.Pong, new PongPacket().Type);
            Assert.Equal(PacketType.LobbyUpdate, new LobbyUpdatePacket().Type);
            Assert.Equal(PacketType.LobbySync, new LobbySyncPacket().Type);
            Assert.Equal(PacketType.MatchStart, new MatchStartPacket().Type);
            Assert.Equal(PacketType.JoinRefused, new JoinRefusedPacket().Type);
        }

        [Fact]
        public void The_lobby_update_keeps_ship_colour_and_ready()
        {
            var update = LobbyUpdatePacket.Deserialize(Reader(Write(
                new LobbyUpdatePacket { Ship = 2, Color = new Color(1, 2, 3), Ready = true })));

            Assert.Equal(2, update.Ship);
            Assert.Equal(new Color(1, 2, 3), update.Color);
            Assert.True(update.Ready);
        }

        [Fact]
        public void The_lobby_sync_keeps_the_rules_and_the_players_in_order()
        {
            var sync = LobbySyncPacket.Deserialize(Reader(Write(
                new LobbySyncPacket { Rules = SomeRules(), Players = SomePlayers() })));

            Assert.Equal("Box", sync.Rules.Arena);
            Assert.Equal(AsteroidDensity.High, sync.Rules.AsteroidDensity);
            Assert.Equal(2, sync.Players.Count);
            Assert.Equal("Bob", sync.Players[1].Name);
            Assert.Equal("a", sync.Players[0].Id);
            Assert.Equal(new Color(10, 20, 30), sync.Players[0].Color);
            Assert.True(sync.Players[0].Ready);
            Assert.False(sync.Players[1].Ready);
        }

        [Fact]
        public void Match_start_round_trips_with_players_in_slot_order()
        {
            var start = MatchStartPacket.Deserialize(Reader(Write(
                new MatchStartPacket { Rules = SomeRules(), Seed = -12345, Players = SomePlayers() })));

            Assert.Equal(15, start.Rules.ScoreLimit);
            Assert.Equal(-12345, start.Seed);
            Assert.Equal(new[] { "a", "b" }, start.Players.ConvertAll(p => p.Id));
        }

        [Fact]
        public void A_colour_loses_only_its_transparency()
        {
            var writer = new BinaryWriter(new MemoryStream());
            LobbyPlayerInfo.WriteList(writer, new List<LobbyPlayerInfo>
            {
                new LobbyPlayerInfo { Id = "a", Name = "Ann", Color = new Color(255, 128, 0, 10) },
            });
            writer.Flush();

            var players = LobbyPlayerInfo.ReadList(Reader(((MemoryStream)writer.BaseStream).ToArray()));

            Assert.Equal(new Color(255, 128, 0, 255), players[0].Color);
        }

        [Fact]
        public void A_truncated_lobby_sync_throws_rather_than_producing_half_a_lobby()
        {
            byte[] bytes = Write(new LobbySyncPacket { Rules = SomeRules(), Players = SomePlayers() });
            byte[] cut = new byte[bytes.Length - 3];
            System.Array.Copy(bytes, cut, cut.Length);

            Assert.ThrowsAny<System.Exception>(() => LobbySyncPacket.Deserialize(Reader(cut)));
        }
    }
}
