using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Xunit;

namespace VectorRumble.Tests
{
    public class InputBindingsTests
    {
        [Fact]
        public void Every_menu_action_has_a_key_and_a_button()
        {
            var bindings = new ActionBindings();

            foreach (GameAction action in System.Enum.GetValues(typeof(GameAction)))
            {
                Assert.NotEmpty(bindings[action].Keys);
                Assert.NotEmpty(bindings[action].Buttons);
            }
        }

        [Fact]
        public void Rebinding_replaces_the_keys_and_reset_puts_them_back()
        {
            var bindings = new ActionBindings();

            bindings.RebindKey(GameAction.Select, Keys.F);
            Assert.Equal(new[] { Keys.F }, bindings[GameAction.Select].Keys.ToArray());

            bindings.ResetAll();
            Assert.Contains(Keys.Space, bindings[GameAction.Select].Keys);
            Assert.Contains(Keys.Enter, bindings[GameAction.Select].Keys);
        }

        [Fact]
        public void The_two_keyboard_players_share_no_keys()
        {
            var wasd = KeyboardLayout.Wasd();
            var arrows = KeyboardLayout.Arrows();

            Assert.Empty(AllKeys(wasd).Intersect(AllKeys(arrows)));
        }

        [Fact]
        public void Joining_is_done_with_the_thrust_key()
        {
            Assert.Equal(KeyboardLayout.Wasd().Thrust, KeyboardLayout.Wasd().Join);
        }

        [Fact]
        public void Slots_one_and_two_are_gamepad_players_and_three_and_four_use_the_keyboard()
        {
            Assert.Null(ShipControls.ForSlot(PlayerIndex.One).Keyboard);
            Assert.Null(ShipControls.ForSlot(PlayerIndex.Two).Keyboard);
            Assert.NotNull(ShipControls.ForSlot(PlayerIndex.Three).Keyboard);
            Assert.NotNull(ShipControls.ForSlot(PlayerIndex.Four).Keyboard);
        }

        [Fact]
        public void A_slot_always_gets_the_same_controls()
        {
            Assert.Same(ShipControls.ForSlot(PlayerIndex.Three), ShipControls.ForSlot(PlayerIndex.Three));
        }

        static System.Collections.Generic.IEnumerable<Keys> AllKeys(KeyboardLayout layout) =>
            layout.Left.Concat(layout.Right).Concat(layout.Thrust).Concat(layout.Fire).Concat(layout.Mine).Concat(layout.Leave);
    }
}
