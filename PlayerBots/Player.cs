using System.Collections.Generic;

namespace Pul
{
    /// <summary>
    /// Represents an outline of a player class that it should follow.
    /// </summary>
    abstract class Player
    {
        /// <summary>
        /// Represents the name of the <see cref="Player"/>
        /// </summary>
        public string Name;
        /// <summary>
        /// Represents the player's hand of cards.
        /// </summary>
        /// <remarks>
        /// The hand is stored as a <see cref="List{T}"/> of <see cref="Card"/> objects.
        /// Modifying this local reference to <see cref="Hand"/> will not change the actual hand in the game.
        /// </remarks>
        public List<Card> Hand;
        /// <summary>
        /// Represents the current trumf card.
        /// </summary>
        /// <remarks>
        /// The current trumf card is stored as a <see cref="Card"/> struct externally in the <see cref="PulFunctions"/> class.
        /// <br></br>
        /// Modifying the value of this field will not change the <see cref="PulFunctions.Trumfen"/> property in the game.
        /// </remarks>
        public Card CurrentTrumf;
        /// <summary>
        /// Represents the top card in the current stack, which is the main suit.
        /// </summary>
        /// <remarks>
        /// The top card is stored as a <see cref="Card"/> struct externally in the <see cref="PulFunctions"/> class.
        /// <br></br>
        /// Modifying the value of this field will not change the top card in the stack.
        /// </remarks>
        public Card CurrentSuitCard;

        /// <summary>
        /// Constructor for creating a player instance, which sets <see cref="Name"/>.
        /// </summary>
        /// <param name="name">The name of the player in the game.</param>
        public Player(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Asks the player to bid on the number of stacks they expect to win.
        /// </summary>
        /// <returns>The player's bid indicating the number of stacks they predict to win.</returns>
        public abstract int StickBidAmount();
        /// <summary>
        /// Asks the player which card it wants to put in the stack.
        /// </summary>
        /// <remarks>
        /// The <paramref name="currentStack"/> parameter holds a copy of the cards played in the current stack.
        /// <br></br>
        /// Modifying the elements of this list will not affect the game's actual stack.
        /// </remarks>
        /// <param name="currentStack">A list containing all the cards played in the current stack.</param>
        /// <returns>The <see cref="Card"/> from <see cref="Hand"/> that the player wants to put in the stack.</returns>
        public abstract Card CardToStack(List<Card> currentStack);

        /// <summary>
        /// Returns the name of the player.
        /// </summary>
        /// <returns>The <see cref="Name"/> as a string.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
