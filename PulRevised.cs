using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NewPul
{
    enum Suit
    {
        Hearts, Diamonds, Clubs, Spades,
        Joker
    }

    enum Rank
    {
        Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten,
        Jack, Queen, King, Ace
    }

    struct Card
    {
        public Suit Suit { get; }
        public Rank Rank { get; }
        private int Id;

        public Card(Suit suit, Rank rank, int id)
        {
            Suit = suit;
            Rank = rank;
            Id = id;
        }

        public override string ToString()
        {
            if (Suit == Suit.Joker) return "Joker";

            return $"{Rank} of {Suit}";
        }

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

        public bool Equals(Card card)
        {
            return card.Suit == Suit && card.Rank == Rank && card.Id == Id;
        }
    }

    class PulRevised
    {
        const int TotalNumberOfRounds = 20;

        public List<Player> Players { get; private set; }
        public Dictionary<string, List<Card>> PlayerHands { get; private set; }
        public Dictionary<string, int> PlayerScores { get; private set; }
        public Dictionary<string, int> PlayerBets { get; private set; }
        public Dictionary<Card, string> PlayersCardInStack { get; private set; }
        public int[] PlayerWonStacks { get; private set; }
        public Card Trumfen { get; private set; }
        private List<Card> Deck;
        public int CurrentDealerIndex { get; private set; }

        static Random Random = new Random();

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

        // Returns a list of players of everyone who won in a tie or a list of one player
        public List<Player> StartGame()
        {
            CurrentDealerIndex = 0;

            for (int round = 1; round < TotalNumberOfRounds; round++)
            {
                Deck = ShuffleDeck(CreateDeck());

                int numOfStacks = DealCards(round);

                GetPlayerBets();

                PlayRound(numOfStacks);

                UpdateScores();

                CurrentDealerIndex = (CurrentDealerIndex + 1) % Players.Count;
            }

            return DetermineWinner();
        }

        private void PlayRound(int numOfStacks)
        {
            PlayerWonStacks = new int[Players.Count];
            for (int i = 0; i < numOfStacks; i++)
            {
                PlayersCardInStack = new Dictionary<Card, string>(Players.Count);
                List<Card> currentStack = new List<Card>(Players.Count);
                Suit currentSuit = Suit.Joker;

                for (int j = CurrentDealerIndex; j < Players.Count + CurrentDealerIndex; j++)
                {
                    int tempCurrentIndex;

                    if (j >= Players.Count) tempCurrentIndex = j - Players.Count;
                    else tempCurrentIndex = j;

                    Player currentPlayersTurn = Players[tempCurrentIndex];

                    Card nextStackCard = currentPlayersTurn.CardToStack(currentStack.ToList());
                    if (!IsCardEligible(nextStackCard, currentSuit, Trumfen.Suit, PlayerHands[currentPlayersTurn.Name]))
                    {
                        throw new Exception($"Player: {currentPlayersTurn.Name}, tried to cheat.");
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
                            currentSuit = nextStackCard.Suit;
                            foreach (Player player in Players)
                            {
                                player.CurrentSuitCard = nextStackCard;
                            }
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

        private int DealCards(int round)
        {
            int amountOfCards = NumberOfStacks(round);
            if (Players.Count * amountOfCards > Deck.Count)
            {
                for (int i = amountOfCards; i > 0; i--)
                {
                    if (Players.Count * i < Deck.Count)
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
                    Card topCard = TakeTopCard();

                    PlayerHands[player.Name].Add(topCard);
                    player.Hand.Add(topCard);
                }
            }

            Trumfen = TakeTopCard();

            foreach (Player player in Players)
            {
                player.CurrentTrumf = Trumfen;
            }

            return amountOfCards;
        }

        private Card TakeTopCard()
        {
            Card topCard = Deck[0];
            Deck.RemoveAt(0);
            return topCard;
        }

        private int NumberOfStacks(int round)
        {
            if (round > 10)
                return 1 + TotalNumberOfRounds - round;
            else
                return round;
        }

        private List<Card> CreateDeck()
        {
            const int NumOfJokers = 3;

            List<Card> deck = new List<Card>(52 + NumOfJokers);
            int currentId = 0;

            // Create all 52 cards in a deck
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                if (suit != Suit.Joker)
                {
                    foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                    {
                        deck.Add(new Card(suit, rank, currentId));
                        currentId++;
                    }
                }
            }

            // Adds the amount of jokers equal to NumOfJokers
            for (int i = 0; i < NumOfJokers; i++)
            {
                deck.Add(new Card(Suit.Joker, Rank.Ace, currentId));
                currentId++;
            }

            return deck;
        }

        private List<Card> ShuffleDeck(List<Card> deck)
        {
            for (int n = deck.Count; n > 1; n--)
            {
                int k = Random.Next(n + 1);
                (deck[n], deck[k]) = (deck[k], deck[n]);
            }

            return deck;
        }

        public static bool IsCardEligible(Card nextStackCard, Suit currentSuit, Suit trumfSuit, List<Card> hand)
        {
            // First, check if the player had nextStackCard in their hand
            // Second, check if nextStackCard's suit is of currentSuit, and if not, check if it had any other card of currentSuit on Player hand
            // Third, check if nextStackCard's suit is of trumfSuit, and if not, check if it had any other card of trumfSuit on Player hand

            // First check: Verify if the player has the nextStackCard in their hand
            if (!hand.Contains(nextStackCard))
            {
                return false;
            }

            // Second check: If nextStackCard's suit is not the currentSuit,
            // verify if there are any cards of currentSuit in the player's hand
            else if (nextStackCard.Suit != currentSuit && hand.Any(card => card.Suit == currentSuit))
            {
                return false;
            }

            // Third check: If nextStackCard's suit is not the trumfSuit,
            // verify if there are any cards of trumfSuit in the player's hand
            else if (nextStackCard.Suit != trumfSuit && hand.Any(card => card.Suit == trumfSuit))
            {
                return false;
            }

            // Card is eligible
            return true;
        }
    }

    abstract class Player
    {
        public string Name;
        public List<Card> Hand;
        public Card CurrentTrumf;
        public Card CurrentSuitCard = new Card(Suit.Joker, Rank.Ace, -1);

        public Player(string name)
        {
            Name = name;
        }

        public abstract int StickBidAmount();
        public abstract Card CardToStack(List<Card> currentStack);
        public override string ToString()
        {
            return Name;
        }
    }
}