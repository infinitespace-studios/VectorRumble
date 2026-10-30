using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
using Xunit;

namespace VectorRumble.Tests
{
    public class LocalizationTests
    {
        public static readonly string[] Cultures = { "de", "es", "fr", "ja", "ko", "zh-CN" };

        // the strings of the LAN screens
        public static readonly string[] LanKeys =
        {
            "Multiplayer_LAN", "Multiplayer_Local", "Back", "Lan_Host_A_Game", "Lan_Join_Game", "Lan_Searching",
            "Lan_No_Games", "Lan_Starting_Game", "Lan_Joining", "Lan_Could_Not_Connect", "Lan_Players", "Lan_Ship",
            "Lan_Change_Colour", "Lan_Ready", "Lan_Start_Game", "Lan_Leave", "Lan_Waiting_For_Players",
            "Lan_Waiting_For_Host", "Lan_Rules", "Lan_Host_Ended", "Lan_Already_Started", "Lan_Not_In_Match",
            "Lan_Connection_Lost",
        };

        static string Get(string key, CultureInfo culture) => Strings.ResourceManager.GetString(key, culture);

        static string[] Placeholders(string text) =>
            Regex.Matches(text, @"\{\d+\}").Cast<Match>().Select(m => m.Value).OrderBy(v => v).ToArray();

        [Theory]
        [MemberData(nameof(Pairs))]
        public void A_lan_string_is_translated(string culture, string key)
        {
            string english = Get(key, CultureInfo.InvariantCulture);
            string translated = Get(key, new CultureInfo(culture));

            Assert.False(string.IsNullOrWhiteSpace(translated));
            Assert.NotEqual(english, translated);
        }

        [Theory]
        [MemberData(nameof(Pairs))]
        public void A_translation_keeps_the_values_the_game_fills_in(string culture, string key)
        {
            string english = Get(key, CultureInfo.InvariantCulture);
            string translated = Get(key, new CultureInfo(culture));

            // {0}, {1}... are replaced with names and numbers: losing one, or adding one, breaks string.Format
            Assert.Equal(Placeholders(english), Placeholders(translated));
        }

        public static System.Collections.Generic.IEnumerable<object[]> Pairs()
        {
            foreach (string culture in Cultures)
                foreach (string key in LanKeys)
                    yield return new object[] { culture, key };
        }
    }
}
