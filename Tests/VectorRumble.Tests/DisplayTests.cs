using Microsoft.Xna.Framework;
using Xunit;

namespace VectorRumble.Tests
{
    public class DisplayTests
    {
        [Fact]
        public void A_screen_of_the_same_size_needs_no_scaling()
        {
            Display.Fit(1280, 720, out float scale, out Vector2 offset);

            Assert.Equal(1f, scale);
            Assert.Equal(Vector2.Zero, offset);
        }

        [Fact]
        public void A_wider_screen_scales_by_height_and_gets_bars_at_the_sides()
        {
            Display.Fit(2400, 1080, out float scale, out Vector2 offset);

            Assert.Equal(1.5f, scale);
            // the picture is 1920 wide, so 240 on each side
            Assert.Equal(new Vector2(240f, 0f), offset);
        }

        [Fact]
        public void A_taller_screen_scales_by_width_and_gets_bars_above_and_below()
        {
            Display.Fit(1280, 1000, out float scale, out Vector2 offset);

            Assert.Equal(1f, scale);
            Assert.Equal(new Vector2(0f, 140f), offset);
        }

        [Fact]
        public void A_touch_on_the_screen_lands_in_the_same_place_on_the_picture()
        {
            Display.Update(2400, 1080);

            // the middle of the screen is the middle of the picture
            Vector2 middle = Display.ToVirtual(new Vector2(1200, 540));
            Assert.Equal(640f, middle.X, 3);
            Assert.Equal(360f, middle.Y, 3);

            // the picture's top left corner
            Vector2 corner = Display.ToVirtual(new Vector2(240, 0));
            Assert.Equal(0f, corner.X, 3);
            Assert.Equal(0f, corner.Y, 3);
        }

        [Fact]
        public void A_touch_on_a_bar_is_outside_the_picture()
        {
            Display.Update(2400, 1080);

            Assert.True(Display.ToVirtual(new Vector2(100, 500)).X < 0f);
            Assert.True(Display.ToVirtual(new Vector2(2300, 500)).X > Display.BaseWidth);
        }

        [Fact]
        public void The_destination_is_where_the_picture_is_drawn()
        {
            Display.Update(2400, 1080);

            Assert.Equal(new Rectangle(240, 0, 1920, 1080), Display.Destination);
        }

        [Fact]
        public void A_screen_with_no_size_changes_nothing()
        {
            Display.Update(1280, 720);
            Display.Update(0, 0);

            Assert.Equal(1f, Display.Scale);
        }
    }
}
