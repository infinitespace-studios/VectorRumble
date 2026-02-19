#region File Description
//-----------------------------------------------------------------------------
// AIController.cs
//
// AI decision-making logic for AI-controlled ships
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Linq;
using Microsoft.Xna.Framework;
#endregion

namespace VectorRumble
{
    /// <summary>
    /// AI controller that handles decision-making for AI ships
    /// </summary>
    internal class AIController
    {
        #region Constants
        /// <summary>
        /// Distance at which AI will try to collect power-ups
        /// </summary>
        const float powerUpCollectionDistance = 300f;

        /// <summary>
        /// Distance at which AI considers a target to be close
        /// </summary>
        const float closeRangeDistance = 200f;

        /// <summary>
        /// Angle tolerance for firing (in radians)
        /// </summary>
        const float fireAngleTolerance = 0.3f;

        /// <summary>
        /// Distance to maintain from walls
        /// </summary>
        const float wallAvoidanceDistance = 100f;

        /// <summary>
        /// Distance to maintain from asteroids
        /// </summary>
        const float asteroidAvoidanceDistance = 80f;

        /// <summary>
        /// How much to weight evasion vs aggression (0-1, higher = more evasive)
        /// </summary>
        const float evasionWeight = 0.3f;
        #endregion

        #region Fields
        private readonly Ship ship;
        private readonly World world;
        private Random random;
        
        private Actor currentTarget;
        private float targetUpdateTimer;
        private const float targetUpdateInterval = 0.5f; // Update target every 0.5 seconds
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor for AI Controller
        /// </summary>
        /// <param name="ship">The ship this AI controls</param>
        /// <param name="world">The world the ship exists in</param>
        public AIController(Ship ship, World world)
        {
            this.ship = ship;
            this.world = world;
            this.random = new Random(ship.GetHashCode());
            this.targetUpdateTimer = 0f;
        }
        #endregion

        #region AI Decision Making
        /// <summary>
        /// Main AI update method - makes decisions and returns desired actions
        /// </summary>
        /// <param name="elapsedTime">Time elapsed since last update</param>
        /// <returns>AI input state</returns>
        public AIInput GetInput(float elapsedTime)
        {
            var input = new AIInput();

            // Don't do anything if ship is dead
            if (ship.Dead)
                return input;

            // Update target selection periodically
            targetUpdateTimer += elapsedTime;
            if (targetUpdateTimer >= targetUpdateInterval || currentTarget == null || currentTarget.Dead)
            {
                currentTarget = SelectTarget();
                targetUpdateTimer = 0f;
            }

            // Calculate desired movement direction
            Vector2 desiredDirection = CalculateDesiredDirection();
            
            // Set movement input
            if (desiredDirection.LengthSquared() > 0.01f)
            {
                input.MovementDirection = Vector2.Normalize(desiredDirection);
            }

            // Determine if we should fire
            if (currentTarget != null && !currentTarget.Dead)
            {
                Vector2 toTarget = currentTarget.Position - ship.Position;
                float distanceToTarget = toTarget.Length();
                
                if (distanceToTarget > 0)
                {
                    Vector2 targetDirection = toTarget / distanceToTarget;
                    
                    // Calculate predicted target position
                    Vector2 predictedPosition = PredictTargetPosition(currentTarget, distanceToTarget);
                    Vector2 toPredicted = predictedPosition - ship.Position;
                    
                    if (toPredicted.LengthSquared() > 0)
                    {
                        Vector2 aimDirection = Vector2.Normalize(toPredicted);
                        
                        // Calculate ship's forward direction
                        Vector2 shipForward = new Vector2((float)Math.Sin(ship.Rotation), -(float)Math.Cos(ship.Rotation));
                        
                        // Check if target is roughly in front of us
                        float dotProduct = Vector2.Dot(shipForward, aimDirection);
                        float angle = (float)Math.Acos(MathHelper.Clamp(dotProduct, -1f, 1f));
                        
                        if (angle < fireAngleTolerance && distanceToTarget < 500f)
                        {
                            input.FireDirection = aimDirection;
                        }
                    }
                }
            }

            // Consider dropping mines if enemies are chasing us
            if (ShouldDropMine())
            {
                input.DropMine = true;
            }

            return input;
        }

        /// <summary>
        /// Selects the best target for the AI
        /// </summary>
        private Actor SelectTarget()
        {
            // Get all enemy ships (ships that are playing and not this ship)
            var enemyShips = world.Actors
                .OfType<Ship>()
                .Where(s => s.Playing && !s.Dead && s != ship)
                .ToList();

            if (!enemyShips.Any())
                return null;

            // Find nearest enemy
            Ship nearestEnemy = null;
            float nearestDistance = float.MaxValue;

            foreach (var enemy in enemyShips)
            {
                float distance = Vector2.Distance(ship.Position, enemy.Position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy;
                }
            }

            return nearestEnemy;
        }

        /// <summary>
        /// Calculates the desired movement direction based on AI goals
        /// </summary>
        private Vector2 CalculateDesiredDirection()
        {
            Vector2 direction = Vector2.Zero;

            // Avoid walls and boundaries
            Vector2 wallAvoidance = CalculateWallAvoidance();
            direction += wallAvoidance * 2.0f; // High priority

            // Avoid asteroids
            Vector2 asteroidAvoidance = CalculateAsteroidAvoidance();
            direction += asteroidAvoidance * 1.5f;

            // Move toward target
            if (currentTarget != null && !currentTarget.Dead)
            {
                Vector2 toTarget = currentTarget.Position - ship.Position;
                if (toTarget.LengthSquared() > 0)
                {
                    direction += Vector2.Normalize(toTarget) * 1.0f;
                }
            }

            // Check for nearby power-ups if safe
            if (wallAvoidance.LengthSquared() < 0.1f && asteroidAvoidance.LengthSquared() < 0.1f)
            {
                Vector2 powerUpDirection = SeekPowerUp();
                direction += powerUpDirection * 0.5f;
            }

            return direction;
        }

        /// <summary>
        /// Calculates avoidance vector for walls
        /// </summary>
        private Vector2 CalculateWallAvoidance()
        {
            Vector2 avoidance = Vector2.Zero;
            Rectangle safeDimensions = world.SafeDimensions;

            // Check proximity to each wall
            float leftDist = ship.Position.X - safeDimensions.Left;
            float rightDist = safeDimensions.Right - ship.Position.X;
            float topDist = ship.Position.Y - safeDimensions.Top;
            float bottomDist = safeDimensions.Bottom - ship.Position.Y;

            // Add avoidance force if too close to walls
            if (leftDist < wallAvoidanceDistance)
            {
                float strength = 1.0f - (leftDist / wallAvoidanceDistance);
                avoidance.X += strength;
            }
            if (rightDist < wallAvoidanceDistance)
            {
                float strength = 1.0f - (rightDist / wallAvoidanceDistance);
                avoidance.X -= strength;
            }
            if (topDist < wallAvoidanceDistance)
            {
                float strength = 1.0f - (topDist / wallAvoidanceDistance);
                avoidance.Y += strength;
            }
            if (bottomDist < wallAvoidanceDistance)
            {
                float strength = 1.0f - (bottomDist / wallAvoidanceDistance);
                avoidance.Y -= strength;
            }

            return avoidance;
        }

        /// <summary>
        /// Calculates avoidance vector for asteroids
        /// </summary>
        private Vector2 CalculateAsteroidAvoidance()
        {
            Vector2 avoidance = Vector2.Zero;

            var asteroids = world.Actors.OfType<Asteroid>();
            
            foreach (var asteroid in asteroids)
            {
                if (asteroid.Dead)
                    continue;

                Vector2 toAsteroid = asteroid.Position - ship.Position;
                float distance = toAsteroid.Length();

                if (distance < asteroidAvoidanceDistance && distance > 0)
                {
                    float strength = 1.0f - (distance / asteroidAvoidanceDistance);
                    Vector2 awayFromAsteroid = -toAsteroid / distance;
                    avoidance += awayFromAsteroid * strength;
                }
            }

            return avoidance;
        }

        /// <summary>
        /// Seeks nearby power-ups
        /// </summary>
        private Vector2 SeekPowerUp()
        {
            var powerUps = world.Actors
                .OfType<PowerUp>()
                .Where(p => !p.Dead);

            PowerUp nearestPowerUp = null;
            float nearestDistance = powerUpCollectionDistance;

            foreach (var powerUp in powerUps)
            {
                float distance = Vector2.Distance(ship.Position, powerUp.Position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPowerUp = powerUp;
                }
            }

            if (nearestPowerUp != null)
            {
                Vector2 toPowerUp = nearestPowerUp.Position - ship.Position;
                if (toPowerUp.LengthSquared() > 0)
                {
                    return Vector2.Normalize(toPowerUp);
                }
            }

            return Vector2.Zero;
        }

        /// <summary>
        /// Predicts where a target will be based on its velocity
        /// </summary>
        private Vector2 PredictTargetPosition(Actor target, float distance)
        {
            // Estimate projectile speed (typical laser speed)
            const float projectileSpeed = 640f;
            
            float timeToTarget = distance / projectileSpeed;
            
            // Predict where target will be
            return target.Position + target.Velocity * timeToTarget;
        }

        /// <summary>
        /// Determines if the AI should drop a mine
        /// </summary>
        private bool ShouldDropMine()
        {
            // Drop mines occasionally when enemies are behind us
            if (currentTarget == null || currentTarget.Dead)
                return false;

            Vector2 toTarget = currentTarget.Position - ship.Position;
            Vector2 shipForward = new Vector2((float)Math.Sin(ship.Rotation), -(float)Math.Cos(ship.Rotation));
            
            // Check if target is behind us
            float dotProduct = Vector2.Dot(Vector2.Normalize(toTarget), shipForward);
            
            // If enemy is behind us and close, drop a mine
            if (dotProduct < -0.5f && toTarget.Length() < closeRangeDistance)
            {
                // Random chance to avoid dropping mines too frequently
                return random.NextDouble() < 0.1; // 10% chance per frame when conditions are met
            }

            return false;
        }
        #endregion
    }

    /// <summary>
    /// Represents the input state calculated by the AI
    /// </summary>
    internal struct AIInput
    {
        public Vector2 MovementDirection;
        public Vector2 FireDirection;
        public bool DropMine;
    }
}
