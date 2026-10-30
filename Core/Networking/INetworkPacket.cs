using System.IO;

namespace VectorRumble.Networking
{
    /// <summary>
    /// A gameplay packet. The type byte is written by <see cref="NetworkManager"/>
    /// before <see cref="Serialize"/>; each packet class also has a static
    /// Deserialize(BinaryReader) that reads what Serialize wrote.
    /// </summary>
    public interface INetworkPacket
    {
        PacketType Type { get; }
        void Serialize(BinaryWriter writer);
    }
}
