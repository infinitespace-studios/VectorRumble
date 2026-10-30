using VectorRumble.Networking;
using Xunit;

namespace VectorRumble.Tests
{
    public class ShipAssignmentTests
    {
        [Fact]
        public void Players_who_chose_different_ships_get_them()
        {
            Assert.Equal(new[] { 2, 0, 4 }, ShipAssignment.Assign(new[] { 2, 0, 4 }, 5));
        }

        [Fact]
        public void The_later_of_two_players_who_chose_the_same_ship_gets_the_next_one()
        {
            Assert.Equal(new[] { 1, 2 }, ShipAssignment.Assign(new[] { 1, 1 }, 5));
        }

        [Fact]
        public void The_search_for_a_free_ship_wraps_round_and_skips_taken_ones()
        {
            // everybody wants the last ship; the others take the following free ones in turn
            Assert.Equal(new[] { 4, 0, 1, 2 }, ShipAssignment.Assign(new[] { 4, 4, 4, 4 }, 5));
            Assert.Equal(new[] { 1, 0, 2 }, ShipAssignment.Assign(new[] { 1, 0, 0 }, 5));
        }

        [Fact]
        public void A_choice_outside_the_list_is_wrapped_into_it()
        {
            Assert.Equal(new[] { 1, 4 }, ShipAssignment.Assign(new[] { 6, -1 }, 5));
        }

        [Fact]
        public void Every_machine_gets_the_same_answer_from_the_same_choices()
        {
            int[] choices = { 3, 3, 0, 3 };
            Assert.Equal(ShipAssignment.Assign(choices, 5), ShipAssignment.Assign((int[])choices.Clone(), 5));
        }

        [Fact]
        public void No_ship_flies_for_two_players_while_there_are_ships_to_spare()
        {
            int[] result = ShipAssignment.Assign(new[] { 2, 2, 2, 2 }, 5);
            Assert.Equal(result.Length, new System.Collections.Generic.HashSet<int>(result).Count);
        }
    }
}
