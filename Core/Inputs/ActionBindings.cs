using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace VectorRumble
{
    /// <summary>
    /// The logical actions the menus ask for. What triggers each one lives in
    /// <see cref="ActionBindings"/>, so screens never name a key or a button.
    /// </summary>
    public enum GameAction
    {
        Select,
        Cancel,
        Pause,
        Up,
        Down,
        Previous,
        Next,
    }

    /// <summary>
    /// Everything bound to one action: any of its keys or gamepad buttons triggers it.
    /// </summary>
    public class ActionBinding
    {
        public readonly List<Keys> Keys = new List<Keys>();
        public readonly List<Buttons> Buttons = new List<Buttons>();
    }

    /// <summary>
    /// The action table, built in with the defaults. Rebinding goes through
    /// <see cref="RebindKey"/> and <see cref="RebindButton"/>.
    /// </summary>
    public class ActionBindings
    {
        private readonly Dictionary<GameAction, ActionBinding> table = new Dictionary<GameAction, ActionBinding>();

        public ActionBindings()
        {
            ResetAll();
        }

        public ActionBinding this[GameAction action] => table[action];

        /// <summary>Put every action back to its default keys and buttons.</summary>
        public void ResetAll()
        {
            table.Clear();
            Set(GameAction.Select, new[] { Keys.Space, Keys.Enter }, new[] { Buttons.A, Buttons.Start });
            Set(GameAction.Cancel, new[] { Keys.Escape }, new[] { Buttons.B, Buttons.Back });
            Set(GameAction.Pause, new[] { Keys.Escape }, new[] { Buttons.Back, Buttons.Start });
            Set(GameAction.Up, new[] { Keys.Up }, new[] { Buttons.DPadUp, Buttons.LeftThumbstickUp });
            Set(GameAction.Down, new[] { Keys.Down }, new[] { Buttons.DPadDown, Buttons.LeftThumbstickDown });
            Set(GameAction.Previous, new[] { Keys.Left }, new[] { Buttons.DPadLeft, Buttons.LeftThumbstickLeft });
            Set(GameAction.Next, new[] { Keys.Right }, new[] { Buttons.DPadRight, Buttons.LeftThumbstickRight });
        }

        /// <summary>Replace an action's keyboard keys with one key.</summary>
        public void RebindKey(GameAction action, Keys key)
        {
            var binding = table[action];
            binding.Keys.Clear();
            binding.Keys.Add(key);
        }

        /// <summary>Replace an action's gamepad buttons with one button.</summary>
        public void RebindButton(GameAction action, Buttons button)
        {
            var binding = table[action];
            binding.Buttons.Clear();
            binding.Buttons.Add(button);
        }

        private void Set(GameAction action, Keys[] keys, Buttons[] buttons)
        {
            var binding = new ActionBinding();
            binding.Keys.AddRange(keys);
            binding.Buttons.AddRange(buttons);
            table[action] = binding;
        }
    }
}
