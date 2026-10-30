#region File Description
//-----------------------------------------------------------------------------
// DoubleLaserPowerUp.cs
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
    /// A power-up that gives a player a triple-laser-shooting weapon.
    /// </summary>
    class DoubleLaserPowerUp : PowerUp
    {
        #region Fields
        #endregion

        #region Initialization
        /// <summary>
        /// Constructs a new DoubleLaserPowerUp.
        /// </summary>
        /// <param name="world">The world that this power-up belongs to.</param>
        public DoubleLaserPowerUp(World world) 
            : base(world) 
        { 
            this.color = Color.Lime;
        }
        #endregion

        #region Interaction
        public override PowerUpKind Kind => PowerUpKind.DoubleLaser;

        /// <summary>
        /// The weapon this power-up gives to a ship.
        /// </summary>
        protected override Weapon CreateWeapon(Ship ship)
        {
            return new DoubleLaserWeapon(ship);
        }
        #endregion
    }
}
