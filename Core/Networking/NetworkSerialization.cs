using System.IO;
using Microsoft.Xna.Framework;

namespace VectorRumble.Networking
{
    /// <summary>
    /// Helpers for writing XNA types with a BinaryWriter / BinaryReader.
    /// Packets use plain System.IO readers and writers so they can be built and
    /// tested without a network session.
    /// </summary>
    public static class NetworkSerialization
    {
        public static void Write(this BinaryWriter writer, Vector2 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
        }

        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        /// <summary>A colour is sent as its red, green and blue bytes (a ship's colour is never see-through).</summary>
        public static void Write(this BinaryWriter writer, Color value)
        {
            writer.Write(value.R);
            writer.Write(value.G);
            writer.Write(value.B);
        }

        public static Color ReadColor(this BinaryReader reader)
        {
            return new Color(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        }
    }
}
