using System;

namespace VectorRumble
{
    /// <summary>
    /// How visible something is, fading it out when nobody has touched the screen for a while and back in at the
    /// next touch. Used for the on-screen controls, so a player with a gamepad is not left looking at them.
    /// </summary>
    public class IdleFade
    {
        /// <summary>Seconds without a touch before the fade-out starts.</summary>
        public const float DefaultDelay = 4f;

        private readonly float delay;
        private readonly float fadeOutSeconds;
        private readonly float fadeInSeconds;
        private float idle;

        /// <summary>How visible, from 0 (gone) to 1 (fully shown).</summary>
        public float Opacity { get; private set; }

        /// <param name="startHidden">Starts gone and stays so until the first touch</param>
        public IdleFade(bool startHidden, float delay = DefaultDelay, float fadeOutSeconds = 1f, float fadeInSeconds = 0.25f)
        {
            this.delay = delay;
            this.fadeOutSeconds = fadeOutSeconds;
            this.fadeInSeconds = fadeInSeconds;

            Opacity = startHidden ? 0f : 1f;
            idle = startHidden ? delay : 0f;
        }

        /// <summary>Counts as a touch, for when the screen is shown again after something else had it.</summary>
        public void Wake()
        {
            idle = 0f;
        }

        /// <param name="elapsedSeconds">Time since the last call</param>
        /// <param name="touched">A finger is on the screen (down or moving)</param>
        /// <param name="canFade">False where the controls must stay in view, such as the screens that say "Hold A to join"</param>
        public void Update(float elapsedSeconds, bool touched, bool canFade)
        {
            idle = touched ? 0f : idle + elapsedSeconds;

            bool show = !canFade || idle < delay;
            float seconds = show ? fadeInSeconds : fadeOutSeconds;
            float step = seconds > 0f ? elapsedSeconds / seconds : 1f;
            Opacity = show ? Math.Min(1f, Opacity + step) : Math.Max(0f, Opacity - step);
        }
    }
}
