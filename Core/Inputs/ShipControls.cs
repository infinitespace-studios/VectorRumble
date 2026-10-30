using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace VectorRumble
{
    /// <summary>
    /// What one player is asking their ship to do this update, whatever device it came from.
    /// </summary>
    public struct ShipInput
    {
        /// <summary>Where the player wants to go, as a stick: Y runs down the screen.</summary>
        public Vector2 Move;

        /// <summary>Where the player wants to fire, as a stick: Y runs down the screen.</summary>
        public Vector2 Aim;

        /// <summary>Keyboard steering: -1 turns left, 1 turns right.</summary>
        public float Turn;

        /// <summary>Keyboard thrust along the way the ship faces.</summary>
        public bool Thrust;

        /// <summary>Fire straight ahead.</summary>
        public bool FireAhead;

        /// <summary>Lay a mine behind the ship.</summary>
        public bool Mine;

        /// <summary>Held to join the game.</summary>
        public bool JoinHeld;

        /// <summary>Held to leave the game.</summary>
        public bool LeaveHeld;

        /// <summary>Newly pressed this update, on the ship selection screen.</summary>
        public bool ChangeColour;

        /// <summary>Newly pressed this update, on the ship selection screen.</summary>
        public bool ChangeShip;
    }

    /// <summary>
    /// The keys one keyboard player uses. Any of an action's keys triggers it.
    /// </summary>
    public class KeyboardLayout
    {
        public readonly List<Keys> Left = new List<Keys>();
        public readonly List<Keys> Right = new List<Keys>();
        public readonly List<Keys> Thrust = new List<Keys>();
        public readonly List<Keys> Fire = new List<Keys>();
        public readonly List<Keys> Mine = new List<Keys>();
        public readonly List<Keys> Leave = new List<Keys>();
        public readonly List<Keys> ChangeColour = new List<Keys>();
        public readonly List<Keys> ChangeShip = new List<Keys>();

        /// <summary>Joining is done by holding the thrust key.</summary>
        public List<Keys> Join => Thrust;

        public static KeyboardLayout Wasd()
        {
            var layout = new KeyboardLayout();
            layout.Left.Add(Keys.A);
            layout.Right.Add(Keys.D);
            layout.Thrust.Add(Keys.W);
            layout.Fire.Add(Keys.Tab);
            layout.Mine.Add(Keys.S);
            layout.Leave.Add(Keys.X);
            layout.ChangeColour.Add(Keys.W);
            layout.ChangeShip.Add(Keys.Tab);
            return layout;
        }

        public static KeyboardLayout Arrows()
        {
            var layout = new KeyboardLayout();
            layout.Left.Add(Keys.Left);
            layout.Right.Add(Keys.Right);
            layout.Thrust.Add(Keys.Up);
            layout.Fire.Add(Keys.RightControl);
            layout.Fire.Add(Keys.RightAlt);
            layout.Mine.Add(Keys.Down);
            layout.Leave.Add(Keys.N);
            layout.ChangeColour.Add(Keys.Up);
            layout.ChangeShip.Add(Keys.RightAlt);
            return layout;
        }
    }

    /// <summary>
    /// One player's controls: a gamepad and, optionally, a keyboard layout. A ship reads
    /// its player's <see cref="ShipControls"/> and never names a key or a button, so a
    /// player can drive whichever ship they hold, in whichever slot.
    /// </summary>
    public class ShipControls
    {
        private const float FireThresholdSquared = 0.25f;

        private static readonly ShipControls[] slots =
        {
            new ShipControls(PlayerIndex.One, null, true),
            new ShipControls(PlayerIndex.Two, null, true),
            new ShipControls(PlayerIndex.Three, KeyboardLayout.Wasd(), false),
            new ShipControls(PlayerIndex.Four, KeyboardLayout.Arrows(), false),
        };

        /// <summary>
        /// The controls that belong to a player slot on this machine: gamepads one and two,
        /// then the two keyboard layouts (which also read gamepads three and four).
        /// </summary>
        public static ShipControls ForSlot(PlayerIndex slot) => slots[(int)slot];

        private readonly PlayerIndex pad;
        private readonly bool padJoins;

        private GamePadState lastPad;
        private KeyboardState lastKeyboard;

        /// <summary>The gamepad this player uses (it also shakes when the ship is hit).</summary>
        public PlayerIndex Pad => pad;

        /// <summary>The keys, or null for a gamepad only player.</summary>
        public KeyboardLayout Keyboard { get; }

        public ShipControls(PlayerIndex pad, KeyboardLayout keyboard, bool padJoins)
        {
            this.pad = pad;
            this.padJoins = padJoins;
            Keyboard = keyboard;
        }

        /// <summary>Reads the devices. Call once an update: the "newly pressed" fields compare with the last call.</summary>
        public ShipInput Read()
        {
            GamePadState padState = VirtualGamePad.Apply(pad, GamePad.GetState(pad));
            KeyboardState keys = Microsoft.Xna.Framework.Input.Keyboard.GetState();

            var input = new ShipInput();
            input.Move = padState.ThumbSticks.Left;
            input.Move.Y = -input.Move.Y;

            input.Aim = padState.ThumbSticks.Right;
            input.Aim.Y = -input.Aim.Y;
            if (input.Aim.LengthSquared() <= FireThresholdSquared)
                input.Aim = Vector2.Zero;

            input.FireAhead = padState.Buttons.RightShoulder == ButtonState.Pressed;
            input.Mine = padState.Buttons.B == ButtonState.Pressed && lastPad.Buttons.B == ButtonState.Released;
            input.JoinHeld = padJoins && padState.Buttons.A == ButtonState.Pressed;
            input.LeaveHeld = padState.Buttons.B == ButtonState.Pressed;
            input.ChangeColour = IsNew(padState, Buttons.DPadUp) || IsNew(padState, Buttons.LeftThumbstickUp);
            input.ChangeShip = IsNew(padState, Buttons.RightShoulder);

            if (Keyboard != null)
            {
                input.JoinHeld |= IsDown(keys, Keyboard.Join);
                input.LeaveHeld |= IsDown(keys, Keyboard.Leave);
                input.ChangeColour |= IsNew(keys, Keyboard.ChangeColour);
                input.ChangeShip |= IsNew(keys, Keyboard.ChangeShip);

                // The keys steer the ship only while no stick is pushed
                if (input.Move == Vector2.Zero)
                {
                    if (IsDown(keys, Keyboard.Left)) input.Turn -= 1f;
                    if (IsDown(keys, Keyboard.Right)) input.Turn += 1f;
                    input.Thrust = IsDown(keys, Keyboard.Thrust);
                    input.FireAhead |= IsDown(keys, Keyboard.Fire);
                    input.Mine |= IsDown(keys, Keyboard.Mine);
                }
            }

            lastPad = padState;
            lastKeyboard = keys;
            return input;
        }

        private bool IsNew(GamePadState state, Buttons button) =>
            state.IsButtonDown(button) && lastPad.IsButtonUp(button);

        private static bool IsDown(KeyboardState state, List<Keys> keys)
        {
            foreach (Keys key in keys)
            {
                if (state.IsKeyDown(key))
                    return true;
            }
            return false;
        }

        private bool IsNew(KeyboardState state, List<Keys> keys)
        {
            foreach (Keys key in keys)
            {
                if (state.IsKeyDown(key) && lastKeyboard.IsKeyUp(key))
                    return true;
            }
            return false;
        }
    }
}
