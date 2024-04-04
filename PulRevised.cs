using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NewPul
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
        private readonly Random Random = new Random();
        /// <summary>
        /// Holds all the cards in the deck
        /// </summary>
        private List<Card> Cards;
        /// <summary>
        /// Gets the amount of cards in <see cref="Cards"/>.
        /// </summary>
        public int CardsAmount => Cards.Count;

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

    /// <summary>
    /// Contains the main game logic for Pul.
    /// </summary>
    class PulRevised
    {
        const int TotalNumberOfRounds = 20;

        /// <summary>
        /// Holds all the reference objects of the players in the game.
        /// </summary>
        public List<Player> Players { get; private set; }
        /// <summary>
        /// Holds the hands of the players, the key is the <see cref="Player.Name"/>.
        /// </summary>
        public Dictionary<string, List<Card>> PlayerHands { get; private set; }
        /// <summary>
        /// Holds the individual scores of the players, the key is the <see cref="Player.Name"/>.
        /// </summary>
        public Dictionary<string, int> PlayerScores { get; private set; }
        /// <summary>
        /// Holds each players bet, the key is the <see cref="Player.Name"/>.
        /// </summary>
        public Dictionary<string, int> PlayerBets { get; private set; }
        /// <summary>
        /// Holds which player instance put which card, the key is the <see cref="Card"/> from the stack and the value is the <see cref="Player.Name"/>.
        /// </summary>
        public Dictionary<Card, string> PlayersCardInStack { get; private set; }
        /// <summary>
        /// Holds how many stacks in the current round each player won.
        /// </summary>
        public int[] PlayerWonStacks { get; private set; }
        /// <summary>
        /// Is which <see cref="Card"/> is trumfen this round.
        /// </summary>
        public Card Trumfen { get; private set; }
        /// <summary>
        /// The deck of cards.
        /// </summary>
        private Deck Deck = new Deck();
        /// <summary>
        /// Is the index of which player that is dealer of the round and gets to choose the current suit with their <see cref="Card"/>.
        /// </summary>
        public int CurrentDealerIndex { get; private set; }

        /// <summary>
        /// Prepares the game by initialising new lists and dictionaries and puts <paramref name="players"/> into <see cref="Players"/>.
        /// </summary>
        /// <param name="players">Are the <see cref="Player"/> objects that are going to play.</param>
        public PulRevised(params Player[] players)
        {
            int playersAmount = players.Length;
            Players = new List<Player>(playersAmount);
            PlayerHands = new Dictionary<string, List<Card>>(playersAmount);
            PlayerScores = new Dictionary<string, int>(playersAmount);
            PlayerBets = new Dictionary<string, int>(playersAmount);

            Dictionary<string, int> numberOfRecurringNames = new Dictionary<string, int>();
            foreach (Player player in players)
            {
                string originalName = player.Name;
                if (numberOfRecurringNames.ContainsKey(originalName))
                {
                    player.Name += $" {numberOfRecurringNames[originalName]}";
                    numberOfRecurringNames[originalName]++;
                }
                else
                {
                    numberOfRecurringNames.Add(originalName, 1);
                }

                Players.Add(player);
                PlayerScores[player.Name] = 0;
            }
        }

        /// <summary>
        /// Goes through one game of Pul and returns all <see cref="Player"/> objects that won (in case it was a tie).
        /// </summary>
        /// <returns>The list of <see cref="Player"/> objects that won the game.</returns>
        // Returns a list of players of everyone who won in a tie or a list of one player
        public List<Player> StartGame()
        {
            CurrentDealerIndex = 0;

            for (int round = 1; round < TotalNumberOfRounds; round++)
            {
                Deck.Init();
                Deck.Shuffle();

                int numOfStacks = DealCards(round);

                GetPlayerBets();

                PlayRound(numOfStacks);

                UpdateScores();

                CurrentDealerIndex = (CurrentDealerIndex + 1) % Players.Count;
            }

            return DetermineWinner();
        }

        /// <summary>
        /// Plays through one round of Pul which plays through the same number of stacks as each the number of player's cards.
        /// </summary>
        /// <param name="numOfStacks">Is how many stacks that should be played.</param>
        /// <exception cref="Exception"></exception>
        private void PlayRound(int numOfStacks)
        {
            PlayerWonStacks = new int[Players.Count];
            for (int i = 0; i < numOfStacks; i++)
            {
                PlayersCardInStack = new Dictionary<Card, string>(Players.Count);
                List<Card> currentStack = new List<Card>(Players.Count);
                Card currentSuit = new Card(Suit.Joker, Rank.Ace, -1);

                for (int j = CurrentDealerIndex; j < Players.Count + CurrentDealerIndex; j++)
                {
                    int tempCurrentIndex;

                    if (j >= Players.Count) tempCurrentIndex = j - Players.Count;
                    else tempCurrentIndex = j;

                    Player currentPlayersTurn = Players[tempCurrentIndex];
                    currentPlayersTurn.CurrentSuitCard = currentSuit;

                    Card nextStackCard = currentPlayersTurn.CardToStack(currentStack.ToList());

                    if (!IsCardEligible(nextStackCard, currentSuit.Suit, Trumfen.Suit, PlayerHands[currentPlayersTurn.Name], out IneligibleReason exceptionCause))
                    {
                        throw new Exception($"Player: {currentPlayersTurn.Name}, tried to cheat by {exceptionCause}.");
                    }

                    else
                    {
                        PlayerHands[currentPlayersTurn.Name].Remove(nextStackCard);
                        FindPlayer(currentPlayersTurn.Name)
                            .Hand
                            .Remove(nextStackCard);
                        currentStack.Add(nextStackCard);
                        PlayersCardInStack.Add(nextStackCard, currentPlayersTurn.Name);

                        if (Players[CurrentDealerIndex].Name == currentPlayersTurn.Name)
                        {
                            currentSuit = nextStackCard;
                        }
                    }
                }

                Card bestCard = BestCard(currentStack);
                PlayerWonStacks[Players.IndexOf(FindPlayer(PlayersCardInStack[bestCard]))]++;
            }
        }

        private void UpdateScores()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (PlayerWonStacks[i] == PlayerBets[Players[i].Name])
                {
                    PlayerScores[Players[i].Name] += PlayerWonStacks[i] + 10;
                }
            }
        }

        private Card BestCard(List<Card> currentStack)
        {
            Card bestCard = currentStack[0];
            Suit currentSuit = bestCard.Suit;
            foreach (Card card in currentStack)
            {
                if (card.Suit == Suit.Joker)
                {
                    bestCard = card;
                }
                else if ((card.Suit == Trumfen.Suit && bestCard.Suit != Trumfen.Suit) || (card.Suit == Trumfen.Suit && bestCard.Suit == Trumfen.Suit && card.Rank > bestCard.Rank))
                {
                    bestCard = card; // Trumf suit is the most valuable
                }
                else if ((card.Suit == currentSuit && bestCard.Suit == currentSuit && card.Rank > bestCard.Rank) || (card.Suit == currentSuit && (bestCard.Suit != Trumfen.Suit && bestCard.Suit != currentSuit)))
                {
                    bestCard = card; // Same suit card is better
                }
                else if (bestCard.Suit != Trumfen.Suit && bestCard.Suit != currentSuit && card.Rank > bestCard.Rank)
                {
                    bestCard = card; // Any other card is better
                }
            }

            return bestCard;
        }

        private Player FindPlayer(string name)
        {
            return Players.Find(player => player.Name == name);
        }

        private void GetPlayerBets()
        {
            PlayerBets = new Dictionary<string, int>(Players.Count);
            foreach (Player player in Players)
            {
                PlayerBets[player.Name] = player.StickBidAmount();
            }
        }

        private List<Player> DetermineWinner()
        {
            int bestScore = 0;
            string bestPlayer = "";
            List<string> bestPlayersName = new List<string>(Players.Count);
            foreach (var score in PlayerScores)
            {
                if (score.Value > bestScore)
                {
                    bestScore = score.Value;
                    bestPlayer = score.Key;
                    bestPlayersName = new List<string>();
                }
                else if (score.Value == bestScore)
                {
                    bestPlayersName.Add(bestPlayer);
                    bestPlayersName.Add(score.Key);
                    bestPlayer = "";
                }
            }

            List<Player> bestPlayers = new List<Player>();
            if (bestPlayer != "")
            {
                bestPlayers.Add(FindPlayer(bestPlayer));
                return bestPlayers;
            }
            else
            {
                foreach (string playerName in bestPlayersName)
                {
                    bestPlayers.Add(FindPlayer(playerName));
                }

                return bestPlayers;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="round"></param>
        /// <returns></returns>
        private int DealCards(int round)
        {
            int amountOfCards = NumberOfStacks(round);
            if (Players.Count * amountOfCards > Deck.CardsAmount)
            {
                for (int i = amountOfCards; i > 0; i--)
                {
                    if (Players.Count * i < Deck.CardsAmount)
                    {
                        amountOfCards = i;
                    }
                }
            }

            foreach (Player player in Players)
            {
                PlayerHands[player.Name] = new List<Card>(amountOfCards);
                player.Hand = new List<Card>(amountOfCards);
            }

            for (int i = 0; i < amountOfCards; i++)
            {
                foreach (Player player in Players)
                {
                    Card topCard = Deck.TakeTopCard();

                    PlayerHands[player.Name].Add(topCard);
                    player.Hand.Add(topCard);
                }
            }

            Trumfen = Deck.TakeTopCard();

            foreach (Player player in Players)
            {
                player.CurrentTrumf = Trumfen;
            }

            return amountOfCards;
        }


        /// <summary>
        /// Takes in the <paramref name="round"/> and checks which amount of stacks should be played this round.
        /// </summary>
        /// <param name="round">The current round, from 1 to <see cref="TotalNumberOfRounds"/>.</param>
        /// <returns>The amount of stacks that will be played and how many cards each player should have during the current round.</returns>
        private int NumberOfStacks(int round)
        {
            if (round > 10)
                return 1 + TotalNumberOfRounds - round;
            else
                return round;
        }

        /// <summary>
        /// Specifies the reason a <see cref="Card"/> may not be available for the current stack.
        /// <br></br>
        /// Used in debugging and error handling.
        /// </summary>
        public enum IneligibleReason
        {
            None,
            CardNotOnHand,
            CardNotCurrentSuit,
            CardNotTrumfSuit,
        }

        /// <summary>
        /// Determines wether the <paramref name="card"/> is eligible to be played in the current stack based on the game state.
        /// <br></br>
        /// Checks if the <paramref name="card"/> is in the player's <paramref name="hand"/>, if it matches the <paramref name="currentSuit"/>, and if it matches the <paramref name="trumfSuit"/>.
        /// </summary>
        /// <param name="card">The card being evaluated for eligibility.</param>
        /// <param name="currentSuit">The current suit that the card should be of, if it has one in their <paramref name="hand"/>.</param>
        /// <param name="trumfSuit">The trumf suit that the card should be of, if it has one in their <paramref name="hand"/> and did not have a card of the <paramref name="currentSuit"/>.</param>
        /// <param name="hand">The player's hand of cards, used to compare if it had another eligible card that should have been played instead.</param>
        /// <param name="ineligibleReason">When the <paramref name="card"/> is ineligible, <paramref name="ineligibleReason"/> specifies the reason why it will not be played.</param>
        /// <returns>
        /// True if the <paramref name="card"/> is eligible to be played; otherwise, false.
        /// <br></br>
        /// If false, the <paramref name="ineligibleReason"/> specifies why the <paramref name="card"/> cannot be played.
        /// </returns>
        public static bool IsCardEligible(Card card, Suit currentSuit, Suit trumfSuit, List<Card> hand, out IneligibleReason ineligibleReason)
        {
            // First, check if the player had nextStackCard in their hand
            // Second, check if nextStackCard's suit is of currentSuit, and if not, check if it had any other card of currentSuit on Player hand
            // Third, check if nextStackCard's suit is of trumfSuit, and if not, check if it had any other card of trumfSuit on Player hand

            // First check: Verify if the player has the nextStackCard in their hand
            if (!hand.Contains(card))
            {
                ineligibleReason = IneligibleReason.CardNotOnHand;
                return false;
            }

            // Second check: If nextStackCard's suit is not the currentSuit,
            // verify if there are any cards of currentSuit in the player's hand
            else if (card.Suit != currentSuit && currentSuit != Suit.Joker)
            {
                if (hand.Any(handCard => handCard.Suit == currentSuit))
                {
                    ineligibleReason = IneligibleReason.CardNotCurrentSuit;
                    return false;
                }
                else
                {
                    // Third check: If nextStackCard's suit is not the trumfSuit,
                    // verify if there are any cards of trumfSuit in the player's hand
                    if (card.Suit != trumfSuit && trumfSuit != Suit.Joker && hand.Any(handCard => handCard.Suit == trumfSuit))
                    {
                        ineligibleReason = IneligibleReason.CardNotTrumfSuit;
                        return false;
                    }
                }
            }

            // Card is eligible
            ineligibleReason = IneligibleReason.None;
            return true;
        }
    }

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
        /// The current trumf card is stored as a <see cref="Card"/> struct externally in the <see cref="PulRevised"/> class.
        /// <br></br>
        /// Modifying the value of this field will not change the <see cref="PulRevised.Trumfen"/> property in the game.
        /// </remarks>
        public Card CurrentTrumf;
        /// <summary>
        /// Represents the top card in the current stack, which is the main suit.
        /// </summary>
        /// <remarks>
        /// The top card is stored as a <see cref="Card"/> struct externally in the <see cref="PulRevised"/> class.
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