using Xunit;

namespace VectorRumble.Tests
{
    public class IdleFadeTests
    {
        // run for a number of seconds in small steps
        static void Run(IdleFade fade, float seconds, bool touched, bool canFade = true)
        {
            for (float t = 0; t < seconds; t += 0.05f)
                fade.Update(0.05f, touched, canFade);
        }

        [Fact]
        public void It_starts_in_view_and_stays_while_the_screen_is_touched()
        {
            var fade = new IdleFade(startHidden: false);

            Assert.Equal(1f, fade.Opacity);
            Run(fade, 30f, touched: true);
            Assert.Equal(1f, fade.Opacity);
        }

        [Fact]
        public void It_stays_in_view_for_the_delay_then_fades_out()
        {
            var fade = new IdleFade(startHidden: false);

            Run(fade, 3.5f, touched: false);
            Assert.Equal(1f, fade.Opacity);       // not yet 4 seconds

            Run(fade, 0.9f, touched: false);      // a little past the delay: partly faded
            Assert.InRange(fade.Opacity, 0.01f, 0.99f);

            Run(fade, 2f, touched: false);
            Assert.Equal(0f, fade.Opacity);
        }

        [Fact]
        public void A_touch_brings_it_back()
        {
            var fade = new IdleFade(startHidden: false);
            Run(fade, 10f, touched: false);
            Assert.Equal(0f, fade.Opacity);

            Run(fade, 0.5f, touched: true);

            Assert.Equal(1f, fade.Opacity);
        }

        [Fact]
        public void A_held_finger_keeps_it_in_view()
        {
            var fade = new IdleFade(startHidden: false);
            Run(fade, 2f, touched: false);
            Run(fade, 20f, touched: true);

            Assert.Equal(1f, fade.Opacity);
        }

        [Fact]
        public void With_a_gamepad_it_starts_out_of_sight_and_waits_for_a_touch()
        {
            var fade = new IdleFade(startHidden: true);
            Assert.Equal(0f, fade.Opacity);

            Run(fade, 10f, touched: false);
            Assert.Equal(0f, fade.Opacity);

            fade.Update(0.05f, touched: true, canFade: true);
            Assert.True(fade.Opacity > 0f);
        }

        [Fact]
        public void It_stays_in_view_where_it_may_not_fade()
        {
            var fade = new IdleFade(startHidden: false);

            Run(fade, 30f, touched: false, canFade: false);

            Assert.Equal(1f, fade.Opacity);
        }

        [Fact]
        public void Waking_it_starts_the_wait_again()
        {
            var fade = new IdleFade(startHidden: false);
            Run(fade, 3.9f, touched: false);

            fade.Wake();
            Run(fade, 3.9f, touched: false);

            Assert.Equal(1f, fade.Opacity);
        }
    }
}
