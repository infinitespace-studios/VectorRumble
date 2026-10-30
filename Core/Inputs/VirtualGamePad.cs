using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace VectorRumble
{
    /// <summary>
    /// On-screen twin-stick controls for touch devices. Player one's gamepad state is
    /// the real gamepad's, overlaid with: a floating left stick (move) under a thumb in
    /// the left of the screen, a floating right stick (aim and fire) in the middle and
    /// right, the A (join, select) and B (mine, hold to leave) buttons at the bottom right,
    /// X (fire ahead, change ship) beside them and Start (pause) at the top right.
    /// </summary>
    class VirtualGamePad
    {
        // The sprites in Textures/gamepad.png
        private static readonly Rectangle LeftStickRect = new Rectangle(2, 2, 68, 68);
        private static readonly Rectangle RightStickRect = new Rectangle(3, 77, 67, 67);
        private static readonly Rectangle ButtonYRect = new Rectangle(73, 4, 36, 36);
        private static readonly Rectangle ButtonXRect = new Rectangle(73, 41, 36, 35);
        private static readonly Rectangle ButtonBRect = new Rectangle(73, 77, 36, 36);
        private static readonly Rectangle ButtonARect = new Rectangle(73, 114, 36, 35);

        // Layout is designed for a 480 high screen and scaled to the real one
        private const float BaseHeight = 480f;
        private const float StickTravel = 60f;
        private const float DeadZone = 0.15f;
        private const float LeftZoneFraction = 0.4f;
        private const float ButtonSpacing = 90f;
        private const float ButtonHitRadius = 48f;
        private const float ButtonScale = 1.5f;
        private const float DirectionThreshold = 0.5f;

        private static readonly Buttons[] RealButtons =
        {
            Buttons.A, Buttons.B, Buttons.X, Buttons.Y, Buttons.Start, Buttons.Back,
            Buttons.LeftShoulder, Buttons.RightShoulder, Buttons.LeftStick, Buttons.RightStick,
            Buttons.DPadUp, Buttons.DPadDown, Buttons.DPadLeft, Buttons.DPadRight, Buttons.BigButton,
            Buttons.LeftThumbstickUp, Buttons.LeftThumbstickDown, Buttons.LeftThumbstickLeft, Buttons.LeftThumbstickRight,
            Buttons.RightThumbstickUp, Buttons.RightThumbstickDown, Buttons.RightThumbstickLeft, Buttons.RightThumbstickRight,
        };

        /// <summary>The pad for the gameplay screen in play, or null when there is none (desktop).</summary>
        public static VirtualGamePad Instance { get; set; }

        private readonly Texture2D texture;
        private readonly Vector2 screenSize;
        private readonly float scale;

        private int leftId = -1, rightId = -1, aId = -1, bId = -1, xId = -1, startId = -1;
        private Vector2 leftOrigin, leftPosition, rightOrigin, rightPosition;

        public VirtualGamePad(Texture2D texture, Viewport viewport)
        {
            this.texture = texture;
            screenSize = new Vector2(viewport.Width, viewport.Height);
            scale = screenSize.Y / BaseHeight;
        }

        private Vector2 ButtonAPosition => screenSize - new Vector2(70f, 80f) * scale;
        private Vector2 ButtonBPosition => ButtonAPosition - new Vector2(0f, ButtonSpacing * scale);
        private Vector2 ButtonXPosition => ButtonAPosition - new Vector2(ButtonSpacing * scale, 0f);
        private Vector2 StartPosition => new Vector2(screenSize.X - 50f * scale, 50f * scale);
        private Vector2 DefaultLeftStick => new Vector2(110f * scale, screenSize.Y - 110f * scale);
        private Vector2 DefaultRightStick => new Vector2(screenSize.X - 250f * scale, screenSize.Y - 110f * scale);

        private bool InButton(Vector2 button, Vector2 position) =>
            Vector2.DistanceSquared(button, position) <= ButtonHitRadius * scale * (ButtonHitRadius * scale);

        /// <summary>Lets go of everything, for when the screen stops reading the controls.</summary>
        public void Reset()
        {
            leftId = rightId = aId = bId = xId = startId = -1;
        }

        /// <summary>Tracks the fingers down on the controls. Call once a frame, before the ships read it.</summary>
        public void Update()
        {
            foreach (TouchLocation touch in TouchPanel.GetState())
            {
                switch (touch.State)
                {
                    case TouchLocationState.Pressed:
                        Press(touch.Id, touch.Position);
                        break;
                    case TouchLocationState.Moved:
                        if (touch.Id == leftId) leftPosition = touch.Position;
                        if (touch.Id == rightId) rightPosition = touch.Position;
                        break;
                    case TouchLocationState.Released:
                    case TouchLocationState.Invalid:
                        Release(touch.Id);
                        break;
                }
            }
        }

        private void Press(int id, Vector2 position)
        {
            if (InButton(ButtonAPosition, position))
            {
                aId = id;
            }
            else if (InButton(ButtonBPosition, position))
            {
                bId = id;
            }
            else if (InButton(ButtonXPosition, position))
            {
                xId = id;
            }
            else if (InButton(StartPosition, position))
            {
                startId = id;
            }
            else if (position.X < screenSize.X * LeftZoneFraction)
            {
                if (leftId < 0)
                {
                    leftId = id;
                    leftOrigin = leftPosition = position;
                }
            }
            else if (rightId < 0)
            {
                rightId = id;
                rightOrigin = rightPosition = position;
            }
        }

        private void Release(int id)
        {
            if (id == leftId) leftId = -1;
            if (id == rightId) rightId = -1;
            if (id == aId) aId = -1;
            if (id == bId) bId = -1;
            if (id == xId) xId = -1;
            if (id == startId) startId = -1;
        }

        private Vector2 StickValue(int id, Vector2 origin, Vector2 position)
        {
            if (id < 0)
                return Vector2.Zero;

            // Screen Y runs down, a gamepad stick's runs up
            Vector2 delta = (position - origin) / (StickTravel * scale);
            delta.Y = -delta.Y;
            if (delta.LengthSquared() > 1f)
                delta.Normalize();
            return delta.LengthSquared() < DeadZone * DeadZone ? Vector2.Zero : delta;
        }

        /// <summary>
        /// Overlays the touch controls on a real gamepad state. Only player one is touch controlled.
        /// </summary>
        public GamePadState GetState(GamePadState real)
        {
            Vector2 left = StickValue(leftId, leftOrigin, leftPosition);
            Vector2 right = StickValue(rightId, rightOrigin, rightPosition);

            if (left == Vector2.Zero) left = real.ThumbSticks.Left;
            if (right == Vector2.Zero) right = real.ThumbSticks.Right;

            Buttons pressed = 0;
            foreach (Buttons button in RealButtons)
            {
                if (real.IsButtonDown(button))
                    pressed |= button;
            }
            if (aId >= 0) pressed |= Buttons.A;
            if (bId >= 0) pressed |= Buttons.B;
            if (xId >= 0) pressed |= Buttons.RightShoulder;
            if (startId >= 0) pressed |= Buttons.Start;

            // The sticks also read as buttons, which is how the menus move through their entries
            if (StickValue(leftId, leftOrigin, leftPosition).Y > DirectionThreshold) pressed |= Buttons.LeftThumbstickUp;
            if (StickValue(leftId, leftOrigin, leftPosition).Y < -DirectionThreshold) pressed |= Buttons.LeftThumbstickDown;
            if (StickValue(leftId, leftOrigin, leftPosition).X < -DirectionThreshold) pressed |= Buttons.LeftThumbstickLeft;
            if (StickValue(leftId, leftOrigin, leftPosition).X > DirectionThreshold) pressed |= Buttons.LeftThumbstickRight;

            return new GamePadState(new GamePadThumbSticks(left, right), real.Triggers,
                new GamePadButtons(pressed), real.DPad);
        }

        /// <summary>The state player one's ship should read: the touch controls over the real pad.</summary>
        public static GamePadState Apply(PlayerIndex player, GamePadState real) =>
            Instance != null && player == PlayerIndex.One ? Instance.GetState(real) : real;

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawStick(spriteBatch, LeftStickRect, leftId, leftOrigin, leftPosition, DefaultLeftStick);
            DrawStick(spriteBatch, RightStickRect, rightId, rightOrigin, rightPosition, DefaultRightStick);
            DrawButton(spriteBatch, ButtonARect, ButtonAPosition, aId >= 0);
            DrawButton(spriteBatch, ButtonBRect, ButtonBPosition, bId >= 0);
            DrawButton(spriteBatch, ButtonXRect, ButtonXPosition, xId >= 0);
            DrawButton(spriteBatch, ButtonYRect, StartPosition, startId >= 0);
        }

        private void DrawStick(SpriteBatch spriteBatch, Rectangle source, int id, Vector2 origin, Vector2 position, Vector2 idle)
        {
            bool active = id >= 0;
            Vector2 center = active ? origin : idle;
            Vector2 offset = active ? position - origin : Vector2.Zero;
            if (offset.Length() > StickTravel * scale)
                offset = Vector2.Normalize(offset) * StickTravel * scale;

            var half = new Vector2(source.Width, source.Height) / 2f;
            Color color = Color.White * (active ? 0.7f : 0.35f);
            spriteBatch.Draw(texture, center, source, color * 0.6f, 0f, half, scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, center + offset, source, color, 0f, half, scale * 0.6f, SpriteEffects.None, 0f);
        }

        private void DrawButton(SpriteBatch spriteBatch, Rectangle source, Vector2 center, bool down)
        {
            var half = new Vector2(source.Width, source.Height) / 2f;
            Color color = Color.White * (down ? 0.9f : 0.4f);
            spriteBatch.Draw(texture, center, source, color, 0f, half, scale * ButtonScale, SpriteEffects.None, 0f);
        }
    }
}
