#region File Description
//-----------------------------------------------------------------------------
// RocketPowerUp.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace VectorRumble
{
    /// <summary>
    /// A power-up that gives a player a rocket-launching weapon.
    /// </summary>
    class RocketPowerUp : PowerUp
    {
        #region Initialization
        /// <summary>
        /// Constructs a new rocket-launcher power-up.
        /// </summary>
        /// <param name="world">The world that this power-up belongs to.</param>
        public RocketPowerUp(World world)
            : base(world) 
        { 
            this.color = Color.Orange;
        }
        #endregion

        #region Interaction
        public override PowerUpKind Kind => PowerUpKind.Rocket;

        /// <summary>
        /// The weapon this power-up gives to a ship.
        /// </summary>
        protected override Weapon CreateWeapon(Ship ship)
        {
            return new RocketWeapon(ship);
        }
        #endregion
    }
}
