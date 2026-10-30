using System;
using System.IO;
using Microsoft.Xna.Framework;
using VectorRumble.Networking;
using Xunit;

namespace VectorRumble.Tests
{
    public class MatchScoringTests
    {
        [Fact]
        public void A_kill_is_worth_a_point()
        {
            short[] scores = { 3, 0, 0 };
            MatchScoring.ApplyKill(scores, attacker: 0, defender: 1);
            Assert.Equal(new short[] { 4, 0, 0 }, scores);
        }

        [Fact]
        public void Blowing_yourself_up_costs_a_point()
        {
            short[] scores = { 3, 5 };
            MatchScoring.ApplyKill(scores, attacker: 1, defender: 1);
            Assert.Equal(new short[] { 3, 4 }, scores);
        }

        [Fact]
        public void Being_destroyed_by_nobody_costs_a_point()
        {
            short[] scores = { 3, 5 };
            MatchScoring.ApplyKill(scores, DamagePacket.NoAttacker, defender: 0);
            Assert.Equal(new short[] { 2, 5 }, scores);
        }

        [Fact]
        public void Scores_can_go_below_zero_as_they_always_could()
        {
            short[] scores = { 0 };
            MatchScoring.ApplyKill(scores, DamagePacket.NoAttacker, defender: 0);
            Assert.Equal(-1, scores[0]);
        }

        [Fact]
        public void A_defender_that_is_not_in_the_match_changes_nothing()
        {
            short[] scores = { 1, 2 };
            MatchScoring.ApplyKill(scores, attacker: 0, defender: 7);
            Assert.Equal(new short[] { 1, 2 }, scores);
        }

        [Fact]
        public void The_first_player_at_the_limit_wins()
        {
            Assert.Equal(-1, MatchScoring.PlayerAtLimit(new short[] { 3, 9, 0 }, 10));
            Assert.Equal(1, MatchScoring.PlayerAtLimit(new short[] { 3, 10, 0 }, 10));
            Assert.Equal(0, MatchScoring.PlayerAtLimit(new short[] { 10, 12, 0 }, 10));
        }
    }

    public class CombatPacketTests
    {
        static BinaryReader Reader(INetworkPacket packet)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            packet.Serialize(writer);
            writer.Flush();
            return new BinaryReader(new MemoryStream(stream.ToArray()));
        }

        [Fact]
        public void A_shot_keeps_who_fired_what_which_way()
        {
            var p = ProjectileFiredPacket.Deserialize(Reader(new ProjectileFiredPacket
            {
                Slot = 2,
                Weapon = (byte)WeaponKind.Rocket,
                Direction = new Vector2(0.6f, -0.8f),
            }));

            Assert.Equal(2, p.Slot);
            Assert.Equal((byte)WeaponKind.Rocket, p.Weapon);
            Assert.Equal(new Vector2(0.6f, -0.8f), p.Direction);
        }

        [Fact]
        public void Damage_keeps_attacker_defender_and_amount()
        {
            var p = DamagePacket.Deserialize(Reader(new DamagePacket
            {
                Attacker = DamagePacket.NoAttacker,
                Defender = 1,
                Amount = 12.5f,
            }));

            Assert.Equal(DamagePacket.NoAttacker, p.Attacker);
            Assert.Equal(1, p.Defender);
            Assert.Equal(12.5f, p.Amount);
        }

        [Fact]
        public void Ship_destroyed_scores_and_power_up_packets_round_trip()
        {
            var destroyed = ShipDestroyedPacket.Deserialize(Reader(new ShipDestroyedPacket { Attacker = 0, Defender = 3 }));
            Assert.Equal(0, destroyed.Attacker);
            Assert.Equal(3, destroyed.Defender);

            var scores = ScoreSyncPacket.Deserialize(Reader(new ScoreSyncPacket { Scores = new short[] { 5, -1, 0 } }));
            Assert.Equal(new short[] { 5, -1, 0 }, scores.Scores);

            var spawned = PowerupSpawnedPacket.Deserialize(Reader(new PowerupSpawnedPacket
            {
                Id = 513,
                Kind = (byte)PowerUpKind.Rocket,
                Position = new Vector2(10, 20),
            }));
            Assert.Equal(513, spawned.Id);
            Assert.Equal((byte)PowerUpKind.Rocket, spawned.Kind);
            Assert.Equal(new Vector2(10, 20), spawned.Position);

            var taken = PowerupTakenPacket.Deserialize(Reader(new PowerupTakenPacket { Id = 513, Player = 2 }));
            Assert.Equal(513, taken.Id);
            Assert.Equal(2, taken.Player);

            Assert.Equal(1, MatchEndPacket.Deserialize(Reader(new MatchEndPacket { Winner = 1 })).Winner);
            Assert.Equal(3, PlayerLeftPacket.Deserialize(Reader(new PlayerLeftPacket { Slot = 3 })).Slot);
        }

        [Fact]
        public void Scores_for_more_players_than_a_match_holds_are_rejected()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write((byte)(MultiplayerSessionService.MaxPlayers + 1));
            writer.Flush();

            Assert.Throws<InvalidDataException>(() =>
                ScoreSyncPacket.Deserialize(new BinaryReader(new MemoryStream(stream.ToArray()))));
        }

        [Fact]
        public void Asteroid_snapshots_round_trip_in_order()
        {
            var p = AsteroidSyncPacket.Deserialize(Reader(new AsteroidSyncPacket
            {
                Sequence = 9,
                Positions = new[] { new Vector2(1, 2), new Vector2(3, 4) },
                Velocities = new[] { new Vector2(-1, 0), new Vector2(0, 5) },
            }));

            Assert.Equal(9, p.Sequence);
            Assert.Equal(new Vector2(3, 4), p.Positions[1]);
            Assert.Equal(new Vector2(-1, 0), p.Velocities[0]);
        }

        [Fact]
        public void An_asteroid_snapshot_with_too_many_asteroids_is_rejected()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write((ushort)0);
            writer.Write((byte)(AsteroidSyncPacket.MaxAsteroids + 1));
            writer.Flush();

            Assert.Throws<InvalidDataException>(() =>
                AsteroidSyncPacket.Deserialize(new BinaryReader(new MemoryStream(stream.ToArray()))));
        }

        [Fact]
        public void Every_machine_makes_the_same_asteroid_from_the_same_seed()
        {
            var a = VectorPolygon.CreateAsteroid(40f, new Random(1234));
            var b = VectorPolygon.CreateAsteroid(40f, new Random(1234));
            var other = VectorPolygon.CreateAsteroid(40f, new Random(4321));

            Assert.Equal(a.Points, b.Points);
            Assert.NotEqual(a.Points, other.Points);
        }

        [Fact]
        public void The_weapon_and_power_up_numbers_match_what_is_sent()
        {
            // these go over the network as bytes: changing a number breaks older builds
            Assert.Equal(0, (byte)WeaponKind.Laser);
            Assert.Equal(3, (byte)WeaponKind.Rocket);
            Assert.Equal(4, (byte)WeaponKind.Mine);
            Assert.Equal(0, (byte)PowerUpKind.DoubleLaser);
            Assert.Equal(2, (byte)PowerUpKind.Rocket);
        }
    }
}
