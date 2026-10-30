using Microsoft.Xna.Framework;

namespace VectorRumble.Networking
{
    /// <summary>
    /// Smooths the ~20Hz states received for a remote ship into per-frame motion.
    /// 
    /// States are shown a short delay behind real time so there is nearly always a
    /// newer state to blend towards. If the next state is late, the ship carries on
    /// along its last velocity for a short while instead of freezing.
    /// </summary>
    public sealed class RemoteShipInterpolator
    {
        // how far behind the newest state the ship is shown, in seconds
        public const double Delay = 0.1;
        // longest the ship is extrapolated past its newest state, in seconds
        public const double MaxExtrapolation = 0.25;
        // a jump bigger than this is a respawn, not movement: snap to it (a ship tops out at 320 units a second)
        public const float TeleportDistance = 200f;

        const int Capacity = 16;

        struct Snapshot
        {
            public double Time;             // local time the state was received
            public Vector2 Position;
            public float Rotation;
            public Vector2 Velocity;
            public byte Shield;
        }

        readonly Snapshot[] buffer = new Snapshot[Capacity];
        int count;                          // snapshots held, oldest first
        ushort lastSequence;
        bool hasSequence;

        /// <summary>
        /// True once at least one state has arrived
        /// </summary>
        public bool HasState
        {
            get { return count > 0; }
        }

        /// <summary>
        /// True if sequence a is newer than b (handles wraparound)
        /// </summary>
        public static bool IsNewer(ushort a, ushort b)
        {
            return a != b && (ushort)(a - b) < 0x8000;
        }

        /// <summary>
        /// Add a received state. Old or duplicate packets are ignored.
        /// </summary>
        public bool Add(ShipStatePacket state, double now)
        {
            if (hasSequence && !IsNewer(state.Sequence, lastSequence))
                return false;

            hasSequence = true;
            lastSequence = state.Sequence;

            // a respawn or a long stall: forget the old path so the ship does not glide across the arena
            if (count > 0 &&
                Vector2.DistanceSquared(buffer[count - 1].Position, state.Position) >
                TeleportDistance * TeleportDistance)
                count = 0;

            if (count == Capacity)
            {
                for (int i = 1; i < Capacity; i++)
                    buffer[i - 1] = buffer[i];
                count--;
            }

            buffer[count++] = new Snapshot
            {
                Time = now,
                Position = state.Position,
                Rotation = state.Rotation,
                Velocity = state.Velocity,
                Shield = state.Shield,
            };
            return true;
        }

        /// <summary>
        /// Where the ship should be drawn at the given local time
        /// </summary>
        public void Sample(double now, out Vector2 position, out float rotation,
            out Vector2 velocity, out byte shield)
        {
            if (count == 0)
            {
                position = Vector2.Zero;
                rotation = 0f;
                velocity = Vector2.Zero;
                shield = 0;
                return;
            }

            double t = now - Delay;
            Snapshot newest = buffer[count - 1];

            // older than everything we have: hold the oldest state
            if (t <= buffer[0].Time)
            {
                Snapshot first = buffer[0];
                position = first.Position;
                rotation = first.Rotation;
                velocity = first.Velocity;
                shield = first.Shield;
                return;
            }

            // newer than everything we have: extrapolate along the last velocity
            if (t >= newest.Time)
            {
                float ahead = (float)System.Math.Min(t - newest.Time, MaxExtrapolation);
                position = newest.Position + newest.Velocity * ahead;
                rotation = newest.Rotation;
                velocity = newest.Velocity;
                shield = newest.Shield;
                return;
            }

            // between two states: blend
            int i = count - 2;
            while (i > 0 && buffer[i].Time > t)
                i--;

            Snapshot a = buffer[i];
            Snapshot b = buffer[i + 1];
            float span = (float)(b.Time - a.Time);
            float amount = span > 1e-6f ? (float)((t - a.Time) / span) : 1f;

            position = Vector2.Lerp(a.Position, b.Position, amount);
            // turn the short way round: a ship's rotation is not kept within one turn
            rotation = a.Rotation + MathHelper.WrapAngle(b.Rotation - a.Rotation) * amount;
            velocity = Vector2.Lerp(a.Velocity, b.Velocity, amount);
            shield = b.Shield;
        }
    }
}
