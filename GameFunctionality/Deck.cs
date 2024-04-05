using System;
using System.Collections.Generic;

namespace Pul
{
    /// <summary>
    /// Defines the constants that represents the suits of a <see cref="Card"/>.
    /// </summary>
    enum Suit
    {
        Hearts, Diamonds, Clubs, Spades,
        Joker,
    }

    /// <summary>
    /// Defines the constants that represents the rank of a <see cref="Card"/>.
    /// </summary>
    enum Rank
    {
        Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten,
        Jack, Queen, King, Ace,
    }

    /// <summary>
    /// Represents a card with a <see cref="NewPul.Suit"/> and <see cref="NewPul.Rank"/>.
    /// <br></br>
    /// Every card also has an <see cref="int"/> to generate a unique hashcode for each card even if they have the same <see cref="NewPul.Suit"/> and <see cref="NewPul.Rank"/>.
    /// </summary>
    readonly struct Card
    {
        /// <summary>
        /// Represents the suit of this card as a <see cref="NewPul.Suit"/>.
        /// </summary>
        public Suit Suit { get; }
        /// <summary>
        /// Represents the value of this card.
        /// </summary>
        public Rank Rank { get; }
        /// <summary>
        /// Used to generate a unique hash code with <see cref="GetHashCode"/>.
        /// </summary>
        private readonly int Id;

        /// <summary>
        /// Initializes a new card with the specified <paramref name="suit"/>, <paramref name="rank"/> and <paramref name="id"/>.
        /// </summary>
        /// <param name="suit">Represents the suit of the card.</param>
        /// <param name="rank">Represents the rank of the card which is the value.</param>
        /// <param name="id">An <see cref="int"/> that is used to generate a unique hash code with <see cref="GetHashCode"/>.</param>
        public Card(Suit suit, Rank rank, int id)
        {
            Suit = suit;
            Rank = rank;
            Id = id;
        }

        /// <summary>
        /// Converts the <see cref="Suit"/> and <see cref="Rank"/> of this card into a readable <see cref="string"/>.
        /// </summary>
        /// <returns>The type name in the representation of a <see cref="string"/>.</returns>
        public override string ToString()
        {
            if (Suit == Suit.Joker) return "Joker";

            return $"{Rank} of {Suit}";
        }

        /// <summary>
        /// Returns a unique <see cref="int"/> to represent this card.
        /// </summary>
        /// <returns>The <see cref="Id"/> as a hash code for this card.</returns>
        public override int GetHashCode()
        {
            return Id;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Card))
                return false;

            return Equals((Card)obj);
        }

        private bool Equals(Card card)
        {
            return card.Suit == Suit && card.Rank == Rank && card.Id == Id;
        }
    }

    /// <summary>
    /// Represents a deck of cards.
    /// </summary>
    class Deck
    {
        /// <summary>
        /// Instance of the Random class for shuffling the deck in <see cref="Shuffle"/>.
        /// </summary>
        private readonly Random Random;
        /// <summary>
        /// Holds all the cards in the deck
        /// </summary>
        private List<Card> Cards;
        /// <summary>
        /// Gets the amount of cards in <see cref="Cards"/>.
        /// </summary>
        public int CardsAmount => Cards.Count;

        /// <summary>
        /// Constructor of <see cref="Deck"/> class that sets the random instance to another random object.
        /// </summary>
        /// <param name="random">Instance of the Random class.</param>
        public Deck(Random random)
        {
            Random = random;
        }

        /// <summary>
        /// Makes <see cref="Cards"/> consist of 52 cards and 3 jokers. 
        /// </summary>
        public void Init()
        {
            const int NumOfJokers = 3;

            Cards = new List<Card>(52 + NumOfJokers);
            int currentId = 0;

            // Create all 52 cards in a deck
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                if (suit != Suit.Joker)
                {
                    foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                    {
                        Cards.Add(new Card(suit, rank, currentId));
                        currentId++;
                    }
                }
            }

            // Adds the amount of jokers equal to NumOfJokers
            for (int i = 0; i < NumOfJokers; i++)
            {
                Cards.Add(new Card(Suit.Joker, Rank.Ace, currentId));
                currentId++;
            }
        }

        /// <summary>
        /// Shuffles the cards in this deck using the <see href="https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle">Fisher-Yates algorithm</see>.
        /// </summary>
        public void Shuffle()
        {
            int n = Cards.Count;
            while (1 < n--)
            {
                int k = Random.Next(n + 1);
                (Cards[n], Cards[k]) = (Cards[k], Cards[n]);
            }
        }

        /// <summary>
        /// Takes the top card of <see cref="Cards"/> and removes it from the deck.
        /// </summary>
        /// <returns>The top card in the deck.</returns>
        public Card TakeTopCard()
        {
            Card topCard = Cards[0];
            Cards.RemoveAt(0);
            return topCard;
        }
    }
}
