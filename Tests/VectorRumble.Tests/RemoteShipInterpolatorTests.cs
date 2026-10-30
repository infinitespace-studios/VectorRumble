using System.IO;
using Microsoft.Xna.Framework;
using VectorRumble.Networking;
using Xunit;

namespace VectorRumble.Tests
{
    public class RemoteShipInterpolatorTests
    {
        // a ship flying along x at 100 units a second, with a state every 0.05s: state i is at x = 5 * i
        static ShipStatePacket State(int sequence, float x, float rotation = 0f, byte shield = 100)
        {
            return new ShipStatePacket
            {
                Sequence = (ushort)sequence,
                Position = new Vector2(x, 0),
                Rotation = rotation,
                Velocity = new Vector2(100, 0),
                Shield = shield,
            };
        }

        static RemoteShipInterpolator Straight()
        {
            RemoteShipInterpolator interpolator = new RemoteShipInterpolator();
            for (int i = 0; i < 10; i++)
                interpolator.Add(State(i, i * 5f), i * 0.05);
            return interpolator;
        }

        static void Sample(RemoteShipInterpolator interpolator, double now,
            out Vector2 position, out float rotation, out byte shield)
        {
            interpolator.Sample(now, out position, out rotation, out _, out shield);
        }

        static Vector2 PositionAt(RemoteShipInterpolator interpolator, double now)
        {
            Sample(interpolator, now, out var position, out _, out _);
            return position;
        }

        [Fact]
        public void Nothing_to_show_before_the_first_state()
        {
            RemoteShipInterpolator interpolator = new RemoteShipInterpolator();
            Assert.False(interpolator.HasState);
            Assert.True(Straight().HasState);
        }

        [Fact]
        public void Blends_between_two_states_a_short_delay_behind_real_time()
        {
            // shown RemoteShipInterpolator.Delay behind, so at 0.35 + delay it shows the state of time 0.35
            Assert.Equal(35f, PositionAt(Straight(), 0.35 + RemoteShipInterpolator.Delay).X, 3);
        }

        [Fact]
        public void Carries_on_along_its_velocity_when_the_next_state_is_late()
        {
            // 0.1 s past the newest state (x = 45)
            Assert.Equal(55f, PositionAt(Straight(), 0.45 + RemoteShipInterpolator.Delay + 0.1).X, 3);
        }

        [Fact]
        public void Extrapolation_stops_after_a_short_while()
        {
            Assert.Equal(45f + 100f * (float)RemoteShipInterpolator.MaxExtrapolation,
                PositionAt(Straight(), 0.45 + RemoteShipInterpolator.Delay + 10).X, 3);
        }

        [Fact]
        public void Holds_the_oldest_state_if_asked_for_a_time_before_it()
        {
            Assert.Equal(0f, PositionAt(Straight(), 0).X, 3);
        }

        [Fact]
        public void Ignores_old_and_duplicate_packets()
        {
            RemoteShipInterpolator interpolator = Straight();
            Assert.False(interpolator.Add(State(3, 999), 1.0));     // older
            Assert.False(interpolator.Add(State(9, 999), 1.0));     // same as the newest
            Assert.True(interpolator.Add(State(10, 50), 0.5));
        }

        [Fact]
        public void Sequence_numbers_wrap_around()
        {
            Assert.True(RemoteShipInterpolator.IsNewer(2, 65535));
            Assert.False(RemoteShipInterpolator.IsNewer(65535, 2));
            Assert.False(RemoteShipInterpolator.IsNewer(7, 7));
        }

        [Fact]
        public void A_big_jump_is_a_respawn_so_the_ship_does_not_glide_across_the_arena()
        {
            RemoteShipInterpolator interpolator = Straight();     // newest at x = 45

            interpolator.Add(State(10, 45f + RemoteShipInterpolator.TeleportDistance + 1f), 0.5);

            // the old path is forgotten, so even a time before the new state shows it
            Assert.Equal(45f + RemoteShipInterpolator.TeleportDistance + 1f, PositionAt(interpolator, 0.1).X, 3);
        }

        [Fact]
        public void Rotation_turns_the_short_way_round()
        {
            RemoteShipInterpolator interpolator = new RemoteShipInterpolator();
            interpolator.Add(State(0, 0, rotation: MathHelper.TwoPi - 0.1f), 0.0);
            interpolator.Add(State(1, 5, rotation: MathHelper.TwoPi + 0.1f), 0.05);

            Sample(interpolator, 0.025 + RemoteShipInterpolator.Delay, out _, out float rotation, out _);

            // half way between -0.1 and 0.1 (a full turn apart in the numbers) is 0, not a spin through pi
            Assert.Equal(MathHelper.TwoPi, rotation, 3);
        }

        [Fact]
        public void The_shield_shown_is_the_newer_states()
        {
            RemoteShipInterpolator interpolator = new RemoteShipInterpolator();
            interpolator.Add(State(0, 0, shield: 100), 0.0);
            interpolator.Add(State(1, 5, shield: 40), 0.05);

            Sample(interpolator, 0.025 + RemoteShipInterpolator.Delay, out _, out _, out byte shield);

            Assert.Equal(40, shield);
        }
    }

    public class ShipStatePacketTests
    {
        [Fact]
        public void A_ship_state_round_trips()
        {
            var state = new ShipStatePacket
            {
                Slot = 2,
                Sequence = 60000,
                Position = new Vector2(1.5f, -2f),
                Rotation = 7.25f,
                Velocity = new Vector2(-3f, 4f),
                Shield = 80,
            };

            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            state.Serialize(writer);
            writer.Flush();

            var read = ShipStatePacket.Deserialize(new BinaryReader(new MemoryStream(stream.ToArray())));

            Assert.Equal(2, read.Slot);
            Assert.Equal(60000, read.Sequence);
            Assert.Equal(new Vector2(1.5f, -2f), read.Position);
            Assert.Equal(7.25f, read.Rotation);
            Assert.Equal(new Vector2(-3f, 4f), read.Velocity);
            Assert.Equal(80, read.Shield);
            Assert.Equal(PacketType.ShipState, read.Type);
        }
    }
}
