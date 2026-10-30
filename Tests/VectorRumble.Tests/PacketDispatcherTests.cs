using System.IO;
using VectorRumble.Networking;
using Xunit;

namespace VectorRumble.Tests
{
    public class PacketDispatcherTests
    {
        static byte[] Payload(INetworkPacket packet)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((byte)packet.Type);
                packet.Serialize(writer);
                writer.Flush();
                return stream.ToArray();
            }
        }

        [Fact]
        public void A_packet_goes_to_its_handler_with_its_body()
        {
            PacketDispatcher dispatcher = new PacketDispatcher();
            uint token = 0;
            dispatcher.Register(PacketType.Ping, PacketAudience.Any,
                (sender, reader) => token = PingPacket.Deserialize(reader).Token);

            dispatcher.Dispatch(null, Payload(new PingPacket { Token = 42 }), false);

            Assert.Equal(42u, token);
        }

        [Fact]
        public void Unknown_packet_types_are_counted_not_fatal()
        {
            PacketDispatcher dispatcher = new PacketDispatcher();

            dispatcher.Dispatch(null, new byte[] { 200, 1, 2 }, false);
            dispatcher.Dispatch(null, new byte[0], false);

            Assert.Equal(1, dispatcher.UnknownCount);
        }

        [Fact]
        public void Packets_meant_for_the_host_are_rejected_by_a_client()
        {
            PacketDispatcher dispatcher = new PacketDispatcher();
            int handled = 0;
            dispatcher.Register(PacketType.LobbyUpdate, PacketAudience.FromClient, (s, r) => handled++);
            byte[] payload = Payload(new LobbyUpdatePacket { Ship = 1, Ready = true });

            dispatcher.Dispatch(null, payload, false);     // this machine is a client
            Assert.Equal(0, handled);
            Assert.Equal(1, dispatcher.RejectedCount);

            dispatcher.Dispatch(null, payload, true);      // this machine is the host
            Assert.Equal(1, handled);
        }

        [Fact]
        public void Packets_meant_for_clients_are_rejected_when_the_sender_is_not_known_to_be_the_host()
        {
            PacketDispatcher dispatcher = new PacketDispatcher();
            int handled = 0;
            dispatcher.Register(PacketType.JoinRefused, PacketAudience.FromHost, (s, r) => handled++);

            dispatcher.Dispatch(null, Payload(new JoinRefusedPacket()), false);
            dispatcher.Dispatch(null, Payload(new JoinRefusedPacket()), true);

            Assert.Equal(0, handled);
            Assert.Equal(2, dispatcher.RejectedCount);
        }

        [Fact]
        public void A_handler_that_throws_is_counted_and_the_game_carries_on()
        {
            PacketDispatcher dispatcher = new PacketDispatcher();
            dispatcher.Register(PacketType.Ping, PacketAudience.Any,
                (s, r) => throw new System.InvalidOperationException("boom"));

            dispatcher.Dispatch(null, Payload(new PingPacket()), false);

            Assert.Equal(1, dispatcher.ErrorCount);
        }

        [Fact]
        public void A_truncated_packet_is_counted_as_an_error()
        {
            PacketDispatcher dispatcher = new PacketDispatcher();
            dispatcher.Register(PacketType.Ping, PacketAudience.Any,
                (s, r) => PingPacket.Deserialize(r));

            dispatcher.Dispatch(null, new byte[] { (byte)PacketType.Ping, 1 }, false);

            Assert.Equal(1, dispatcher.ErrorCount);
        }

        [Fact]
        public void An_unregistered_type_is_unknown_again()
        {
            PacketDispatcher dispatcher = new PacketDispatcher();
            dispatcher.Register(PacketType.Ping, PacketAudience.Any, (s, r) => { });
            dispatcher.Unregister(PacketType.Ping);

            dispatcher.Dispatch(null, Payload(new PingPacket()), false);

            Assert.Equal(1, dispatcher.UnknownCount);
        }
    }
}
