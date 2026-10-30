using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

namespace VectorRumble.Networking
{
    /// <summary>
    /// One LAN match, from the simulation's point of view. It keeps the machines' games in step:
    /// 
    /// * Every machine flies its own ship and sends its state (see <see cref="ShipStateSync"/>).
    /// * Every machine makes the shots any ship fires, but only the host decides what they hit.
    ///   It tells the owner of a ship that the ship was hit; the owner decides when it explodes
    ///   and tells the host, who keeps the score, hands out power-ups and decides who has won.
    /// * Every machine makes the same asteroids from the host's seed, and the host sends where they
    ///   are a few times a second, so that a bump from a ship on one machine does not make them drift.
    /// 
    /// The simulation asks the match through <c>World.Match</c>, which is null in a game on one machine.
    /// </summary>
    internal sealed class NetworkMatch : IDisposable
    {
        // seconds between asteroid snapshots
        const double AsteroidSyncInterval = 0.2;

        readonly World world;
        readonly NetworkManager network;
        readonly Ship[] ships;                      // by player slot
        readonly string[] slotIds;                  // network gamer id for each player slot
        readonly int localSlot;
        readonly ShipStateSync stateSync;
        readonly Dictionary<ushort, PowerUp> powerUps = new Dictionary<ushort, PowerUp>();
        readonly short[] scores;

        ushort nextPowerUpId;
        ushort asteroidSequence;
        bool hasAsteroidSequence;
        double asteroidTimer;
        bool disposed;

        /// <param name="network">The session of the match. The match takes it over and ends it when it is disposed.</param>
        /// <param name="ships">The match's ships by player slot, already chosen</param>
        /// <param name="localSlot">The slot of the player on this machine</param>
        public NetworkMatch(World world, NetworkManager network, MatchStartPacket start, int localSlot, Ship[] ships)
        {
            this.world = world;
            this.network = network;
            this.ships = ships;
            this.localSlot = localSlot;
            Seed = start.Seed;

            slotIds = start.Players.Select(p => p.Id).ToArray();
            scores = new short[slotIds.Length];

            for (int slot = 0; slot < ships.Length; slot++)
            {
                ships[slot].Slot = slot;
                ships[slot].IsRemote = slot != localSlot;
            }
            // whichever slot a player holds, they fly with their own controls: the first player's
            ships[localSlot].Controls = ShipControls.ForSlot(PlayerIndex.One);

            stateSync = new ShipStateSync(network, slotIds, localSlot);

            PacketDispatcher d = network.Dispatcher;
            d.Register(PacketType.ProjectileFired, PacketAudience.Any, OnProjectileFired);
            d.Register(PacketType.Damage, PacketAudience.FromHost, OnDamage);
            d.Register(PacketType.ShipDestroyed, PacketAudience.Any, OnShipDestroyed);
            d.Register(PacketType.ScoreSync, PacketAudience.FromHost, OnScoreSync);
            d.Register(PacketType.PowerupSpawned, PacketAudience.FromHost, OnPowerUpSpawned);
            d.Register(PacketType.PowerupTaken, PacketAudience.FromHost, OnPowerUpTaken);
            d.Register(PacketType.MatchEnd, PacketAudience.FromHost, OnMatchEnd);
            d.Register(PacketType.PlayerLeft, PacketAudience.FromHost, OnPlayerLeft);
            d.Register(PacketType.AsteroidSync, PacketAudience.FromHost, OnAsteroidSync);

            network.GamerJoined += OnGamerJoined;
            network.GamerLeft += OnGamerLeft;

            world.Match = this;
        }

        /// <summary>The host's seed, which every machine makes the asteroids from.</summary>
        public int Seed { get; }

        public bool IsHost
        {
            get { return network.IsHost; }
        }

        public int LocalSlot
        {
            get { return localSlot; }
        }

        /// <summary>The slot of the winner, or -1 while the match goes on.</summary>
        public int Winner { get; private set; } = -1;

        /// <summary>Packets from a client that claimed to be someone else.</summary>
        public int SpoofedCount { get; private set; }

        /// <summary>
        /// Pump the network and keep the ships and asteroids in step. Call once a frame, before the
        /// world is updated.
        /// </summary>
        public void Update(float elapsedTime)
        {
            network.Update();

            stateSync.ApplyRemoteStates(elapsedTime, ships);
            stateSync.SendLocalState(elapsedTime, ships[localSlot]);

            if (network.IsHost)
            {
                asteroidTimer += elapsedTime;
                if (asteroidTimer >= AsteroidSyncInterval)
                {
                    asteroidTimer = 0;
                    SendAsteroids();
                }
            }
        }

        int SlotOf(NetworkGamer gamer)
        {
            return Array.IndexOf(slotIds, gamer.GamerId);
        }

        // the slot of whoever is responsible for a ship or a shot, or NoAttacker
        static byte AttackerSlot(Actor source)
        {
            Ship ship = source as Ship ?? (source as Projectile)?.Owner;
            return ship != null && ship.Slot >= 0 ? (byte)ship.Slot : DamagePacket.NoAttacker;
        }

        // true if the sender really owns the slot it claims (checked by the host)
        bool OwnsSlot(NetworkGamer sender, int slot)
        {
            if (slot < 0 || slot >= slotIds.Length || slotIds[slot] != sender.GamerId)
            {
                SpoofedCount++;
                return false;
            }
            return true;
        }

        #region Asked by the simulation

        /// <summary>
        /// A ship was hit. The host decides what a shot hits: it hurts its own ship at once and tells the
        /// owner of any other. A client never hurts a ship by itself; it waits to be told.
        /// </summary>
        public bool RouteDamage(Ship victim, Actor source, float amount)
        {
            if (victim.Slot < 0)
                return victim.ApplyDamage(source, amount);

            // a client waits to be told; the shot still counts as a hit on this machine
            if (!network.IsHost)
                return true;

            if (victim.Dead)
                return false;

            if (victim.Slot == localSlot)
                return victim.ApplyDamage(source, amount);

            NetworkGamer owner = network.FindRemote(slotIds[victim.Slot]);
            if (owner != null)
            {
                network.Send(owner, new DamagePacket
                {
                    Attacker = AttackerSlot(source),
                    Defender = (byte)victim.Slot,
                    Amount = amount,
                }, true);
            }
            return true;
        }

        /// <summary>
        /// The ship on this machine fired
        /// </summary>
        public void OnShotFired(Ship owner, WeaponKind kind, Vector2 direction)
        {
            if (owner.Slot != localSlot)
                return;

            network.Broadcast(new ProjectileFiredPacket
            {
                Slot = (byte)owner.Slot,
                Weapon = (byte)kind,
                Direction = direction,
            }, true);
        }

        /// <summary>
        /// The ship on this machine exploded: tell the host, or when this is the host score it and tell everybody
        /// </summary>
        public void OnShipDestroyed(Ship ship, Actor source)
        {
            var packet = new ShipDestroyedPacket { Attacker = AttackerSlot(source), Defender = (byte)ship.Slot };

            if (network.IsHost)
                Score(packet.Attacker, packet.Defender);

            network.Broadcast(packet, true);
        }

        /// <summary>
        /// Host: a power-up appeared. Name it and tell everybody.
        /// </summary>
        public void OnPowerUpSpawned(PowerUp powerUp)
        {
            if (!network.IsHost)
                return;

            powerUp.Id = nextPowerUpId++;
            powerUps[powerUp.Id] = powerUp;
            network.Broadcast(new PowerupSpawnedPacket
            {
                Id = powerUp.Id,
                Kind = (byte)powerUp.Kind,
                Position = powerUp.Position,
            }, true);
        }

        /// <summary>
        /// Host: a ship touched a power-up. Tell everybody who took it.
        /// </summary>
        public void OnPowerUpTaken(PowerUp powerUp, Ship ship)
        {
            if (!network.IsHost || ship.Slot < 0)
                return;

            powerUps.Remove(powerUp.Id);
            network.Broadcast(new PowerupTakenPacket { Id = powerUp.Id, Player = (byte)ship.Slot }, true);
        }

        #endregion

        #region Host decisions

        // a ship was destroyed: change the scores, tell everybody and see if somebody won
        void Score(int attacker, int defender)
        {
            MatchScoring.ApplyKill(scores, attacker, defender);
            ApplyScores();
            network.Broadcast(new ScoreSyncPacket { Scores = scores }, true);

            int winner = MatchScoring.PlayerAtLimit(scores, MatchRules.Current.ScoreLimit);
            if (winner >= 0 && Winner < 0)
            {
                Winner = winner;
                network.Broadcast(new MatchEndPacket { Winner = (byte)winner }, true);
            }
        }

        void ApplyScores()
        {
            for (int i = 0; i < scores.Length && i < ships.Length; i++)
                ships[i].Score = scores[i];
        }

        void SendAsteroids()
        {
            Asteroid[] asteroids = world.Actors.OfType<Asteroid>().Take(AsteroidSyncPacket.MaxAsteroids).ToArray();
            if (asteroids.Length == 0)
                return;

            network.Broadcast(new AsteroidSyncPacket
            {
                Sequence = asteroidSequence++,
                Positions = asteroids.Select(a => a.Position).ToArray(),
                Velocities = asteroids.Select(a => a.Velocity).ToArray(),
            }, false);
        }

        // somebody connected after the match started: tell them they are too late
        void OnGamerJoined(NetworkGamer gamer)
        {
            if (network.IsHost && SlotOf(gamer) < 0)
                network.Send(gamer, new JoinRefusedPacket(), true);
        }

        // the host sees who leaves, and tells the others (clients cannot see each other)
        void OnGamerLeft(NetworkGamer gamer)
        {
            int slot = SlotOf(gamer);
            if (!network.IsHost || slot < 0)
                return;

            ships[slot].Depart();
            network.Broadcast(new PlayerLeftPacket { Slot = (byte)slot }, true);
        }

        #endregion

        #region Packets

        void OnProjectileFired(NetworkGamer sender, BinaryReader reader)
        {
            ProjectileFiredPacket p = ProjectileFiredPacket.Deserialize(reader);

            if (p.Slot >= ships.Length || p.Slot == localSlot || !Enum.IsDefined(typeof(WeaponKind), p.Weapon))
                return;

            if (network.IsHost)
            {
                if (!OwnsSlot(sender, p.Slot))
                    return;
                network.Relay(p, true, sender);
            }

            ships[p.Slot].FireRemote((WeaponKind)p.Weapon, p.Direction);
        }

        void OnDamage(NetworkGamer sender, BinaryReader reader)
        {
            DamagePacket p = DamagePacket.Deserialize(reader);

            if (p.Defender != localSlot)
                return;

            Ship attacker = p.Attacker < ships.Length ? ships[p.Attacker] : null;
            ships[localSlot].ApplyDamage(attacker, p.Amount);
        }

        void OnShipDestroyed(NetworkGamer sender, BinaryReader reader)
        {
            ShipDestroyedPacket p = ShipDestroyedPacket.Deserialize(reader);

            if (p.Defender >= ships.Length)
                return;

            if (network.IsHost)
            {
                // a client reports its own ship
                if (!OwnsSlot(sender, p.Defender))
                    return;
                Score(p.Attacker, p.Defender);
                network.Relay(p, true, sender);
            }

            // the ship exploded on the machine that flies it; show it here
            if (p.Defender != localSlot)
                ships[p.Defender].Die(null);
        }

        void OnScoreSync(NetworkGamer sender, BinaryReader reader)
        {
            short[] received = ScoreSyncPacket.Deserialize(reader).Scores;
            Array.Copy(received, scores, Math.Min(received.Length, scores.Length));
            ApplyScores();
        }

        void OnPowerUpSpawned(NetworkGamer sender, BinaryReader reader)
        {
            PowerupSpawnedPacket p = PowerupSpawnedPacket.Deserialize(reader);
            if (!Enum.IsDefined(typeof(PowerUpKind), p.Kind))
                return;

            PowerUp powerUp = PowerUp.Create(world, (PowerUpKind)p.Kind);
            powerUp.Id = p.Id;
            powerUp.Position = p.Position;
            powerUp.Spawn(false);
            powerUps[p.Id] = powerUp;
        }

        void OnPowerUpTaken(NetworkGamer sender, BinaryReader reader)
        {
            PowerupTakenPacket p = PowerupTakenPacket.Deserialize(reader);
            if (p.Player >= ships.Length || !powerUps.TryGetValue(p.Id, out PowerUp powerUp))
                return;

            powerUps.Remove(p.Id);
            powerUp.GiveTo(ships[p.Player]);
        }

        void OnMatchEnd(NetworkGamer sender, BinaryReader reader)
        {
            MatchEndPacket p = MatchEndPacket.Deserialize(reader);
            if (p.Winner < ships.Length)
                Winner = p.Winner;
        }

        void OnPlayerLeft(NetworkGamer sender, BinaryReader reader)
        {
            PlayerLeftPacket p = PlayerLeftPacket.Deserialize(reader);
            if (p.Slot < ships.Length && p.Slot != localSlot)
                ships[p.Slot].Depart();
        }

        void OnAsteroidSync(NetworkGamer sender, BinaryReader reader)
        {
            AsteroidSyncPacket p = AsteroidSyncPacket.Deserialize(reader);

            if (hasAsteroidSequence && !RemoteShipInterpolator.IsNewer(p.Sequence, asteroidSequence))
                return;
            hasAsteroidSequence = true;
            asteroidSequence = p.Sequence;

            int index = 0;
            foreach (Asteroid asteroid in world.Actors.OfType<Asteroid>())
            {
                if (index >= p.Positions.Length)
                    break;
                asteroid.Position = p.Positions[index];
                asteroid.Velocity = p.Velocities[index];
                index++;
            }
        }

        #endregion

        /// <summary>True once the session is over (the host left, or the network failed).</summary>
        public bool HasEnded
        {
            get { return network.HasEnded; }
        }

        /// <summary>
        /// Ends the match and the session with it.
        /// </summary>
        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            stateSync.Dispose();

            PacketDispatcher d = network.Dispatcher;
            d.Unregister(PacketType.ProjectileFired);
            d.Unregister(PacketType.Damage);
            d.Unregister(PacketType.ShipDestroyed);
            d.Unregister(PacketType.ScoreSync);
            d.Unregister(PacketType.PowerupSpawned);
            d.Unregister(PacketType.PowerupTaken);
            d.Unregister(PacketType.MatchEnd);
            d.Unregister(PacketType.PlayerLeft);
            d.Unregister(PacketType.AsteroidSync);

            network.GamerJoined -= OnGamerJoined;
            network.GamerLeft -= OnGamerLeft;

            if (world.Match == this)
                world.Match = null;

            network.Dispose();
        }
    }
}
