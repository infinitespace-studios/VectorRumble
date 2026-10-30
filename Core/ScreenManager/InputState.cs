#region File Description
//-----------------------------------------------------------------------------
// InputState.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;
#endregion

namespace VectorRumble
{
    /// <summary>
    /// Helper for reading input from keyboard and gamepad. This class tracks both
    /// the current and previous state of both input devices, and implements query
    /// properties for high level input actions such as "move up through the menu"
    /// or "pause the game".
    /// </summary>
    /// <remarks>Based on a class in the Game State Management sample.</remarks>
    public class InputState
    {
        #region Fields

        public const int MaxInputs = 4;

        public readonly KeyboardState[] CurrentKeyboardStates;
        public readonly GamePadState[] CurrentGamePadStates;

        public readonly KeyboardState[] LastKeyboardStates;
        public readonly GamePadState[] LastGamePadStates;

        /// <summary>What triggers each <see cref="GameAction"/>.</summary>
        public readonly ActionBindings Bindings = new ActionBindings();

        private MouseState currentMouseState;

        /// <summary>
        /// Where the touch or mouse pointer is. Touch wins while a finger is down.
        /// </summary>
        public Vector2 PointerPosition { get; private set; }

        /// <summary>Is a finger or the left mouse button down?</summary>
        public bool PointerDown { get; private set; }

        /// <summary>Did a finger or the left mouse button go down during this update?</summary>
        public bool PointerPressed { get; private set; }

        /// <summary>Did a finger or the left mouse button come up during this update?</summary>
        public bool PointerReleased { get; private set; }

        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new input state.
        /// </summary>
        public InputState()
        {
            CurrentKeyboardStates = new KeyboardState[MaxInputs];
            CurrentGamePadStates = new GamePadState[MaxInputs];

            LastKeyboardStates = new KeyboardState[MaxInputs];
            LastGamePadStates = new GamePadState[MaxInputs];

            if (Helper.IsMobile)
            {
                TouchPanel.EnabledGestures = GestureType.Tap;
            }
        }


        #endregion

        #region Properties


        /// <summary>Checks for a "menu up" input action, from any player.</summary>
        public bool MenuUp => IsPressed(GameAction.Up);

        /// <summary>Checks for a "menu down" input action, from any player.</summary>
        public bool MenuDown => IsPressed(GameAction.Down);

        /// <summary>Checks for a "menu select" input action, from any player.</summary>
        public bool MenuSelect => IsPressed(GameAction.Select);

        /// <summary>Checks for a "menu cancel" input action, from any player.</summary>
        public bool MenuCancel => IsPressed(GameAction.Cancel);

        /// <summary>Checks for a "pause the game" input action, from any player.</summary>
        public bool PauseGame => IsPressed(GameAction.Pause);


        #endregion

        #region Methods


        /// <summary>
        /// Reads the latest state of the keyboard and gamepad.
        /// </summary>
        public void Update()
        {
            for (int i = 0; i < MaxInputs; i++)
            {
                LastKeyboardStates[i] = CurrentKeyboardStates[i];
                LastGamePadStates[i] = CurrentGamePadStates[i];

                CurrentKeyboardStates[i] = Keyboard.GetState();
                CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);
            }

            UpdatePointer();
        }


        /// <summary>
        /// Helper for checking if a key was newly pressed during this update,
        /// by any player.
        /// </summary>
        public bool IsNewKeyPress(Keys key)
        {
            for (int i = 0; i < MaxInputs; i++)
            {
                if (IsNewKeyPress(key, (PlayerIndex)i))
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Helper for checking if a key was newly pressed during this update,
        /// by the specified player.
        /// </summary>
        public bool IsNewKeyPress(Keys key, PlayerIndex playerIndex)
        {
            return (CurrentKeyboardStates[(int)playerIndex].IsKeyDown(key) &&
                    LastKeyboardStates[(int)playerIndex].IsKeyUp(key));
        }


        /// <summary>
        /// Helper for checking if a button was newly pressed during this update,
        /// by any player.
        /// </summary>
        public bool IsNewButtonPress(Buttons button)
        {
            for (int i = 0; i < MaxInputs; i++)
            {
                if (IsNewButtonPress(button, (PlayerIndex)i))
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Helper for checking if a button was newly pressed during this update,
        /// by the specified player.
        /// </summary>
        public bool IsNewButtonPress(Buttons button, PlayerIndex playerIndex)
        {
            return (CurrentGamePadStates[(int)playerIndex].IsButtonDown(button) &&
                    LastGamePadStates[(int)playerIndex].IsButtonUp(button));
        }


        /// <summary>
        /// Checks whether any key or button bound to the action was newly pressed
        /// during this update, by any player.
        /// </summary>
        public bool IsPressed(GameAction action)
        {
            var binding = Bindings[action];
            foreach (var key in binding.Keys)
            {
                if (IsNewKeyPress(key))
                    return true;
            }
            foreach (var button in binding.Buttons)
            {
                if (IsNewButtonPress(button))
                    return true;
            }
            return false;
        }


        /// <summary>
        /// Folds touch and mouse into the one pointer the menus read.
        /// </summary>
        private void UpdatePointer()
        {
            currentMouseState = Mouse.GetState();

            bool wasDown = PointerDown;
            bool down = false;
            Vector2 position = PointerPosition;

            if (Helper.IsMobile)
            {
                foreach (TouchLocation touch in TouchPanel.GetState())
                {
                    if (touch.State == TouchLocationState.Pressed || touch.State == TouchLocationState.Moved)
                    {
                        down = true;
                        position = touch.Position;
                    }
                }
            }
            else
            {
                down = currentMouseState.LeftButton == ButtonState.Pressed;
                position = new Vector2(currentMouseState.X, currentMouseState.Y);
            }

            PointerDown = down;
            PointerPressed = down && !wasDown;
            PointerReleased = !down && wasDown;
            PointerPosition = position;
        }


        #endregion
    }
}
