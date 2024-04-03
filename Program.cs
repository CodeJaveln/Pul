using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pul
{
    enum Suit 
    { 
        Hearts, Diamonds, Clubs, Spades, 
        Joker
    }

    public enum Rank
    {
        Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten,
        Jack, Queen, King, Ace
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            const int timesRunGame = 1000;
            List<Player> players = new List<Player>();
            List<Type> playerTypes = new List<Type>(Assembly.GetAssembly(typeof(Player)).GetTypes().Where((myType) => myType.IsClass && myType.IsSubclassOf(typeof(Player))));

            for (int i = 0; i < playerTypes.Count; i++)
            {
                Console.WriteLine(i + " " + playerTypes[i].Name);
            }
            string input = "";
            do
            {
                if (input == "")
                {
                    Console.WriteLine("Write any of the bots number (to the left of their name) to add them to the player list");
                }
                input = Console.ReadLine();

                try
                {
                    int i = Convert.ToInt32(input);
                    players.Add((Player)Activator.CreateInstance(playerTypes[i]));
                }
                catch (ArgumentOutOfRangeException)
                {
                    Console.WriteLine("That player isn't in the list, choose another player or enter without typing anything");
                }
                catch (FormatException)
                {
                    if (input != "")
                    {
                        Console.WriteLine("Incorrect input, input either a number or enter without typing anything");
                    }
                }
            } while (input != "" || players.Count < 2);

            PulFunctionality game = new PulFunctionality(players.ToArray());
            Console.WriteLine("Player: " + game.StartGame().ToString() + " won");

            for (int i = 1; i <= timesRunGame; i++)
            {

            }

            Console.ReadKey();
        }
    }

    class PulFunctionality
    {
        private const int TotalNumOfRounds = 20;

        public List<Player> Players { get; private set; }
        public Dictionary<string, List<Card>> PlayerHands { get; private set; }
        public Dictionary<string, int> Scores { get; private set; }
        public Dictionary<string, int> PlayerBets { get; private set; }
        public Dictionary<Card, string> PlayersCardInStack { get; private set; }
        public int[] PlayerWonStacks { get; private set; }
        public Card Trumfen { get; private set; }
        private List<Card> Deck;
        public int CurrentDealerIndex { get; private set; } = 0;

        Random Random { get; } = new Random();

        public PulFunctionality(params Player[] players)
        {
            Players = new List<Player>(players.Length);
            PlayerHands = new Dictionary<string, List<Card>>(players.Length);
            Scores = new Dictionary<string, int>(players.Length);
            PlayerBets = new Dictionary<string, int>(players.Length);


            // Have each player get different names if they have the same
            Dictionary<string, int> numberOfRecurringNames = new Dictionary<string, int>();
            foreach (Player player in players)
            {
                string originalName = player.Name;
                if (numberOfRecurringNames.ContainsKey(originalName))
                {
                    player.Name += numberOfRecurringNames[originalName];
                    numberOfRecurringNames[originalName]++;
                }
                else
                {
                    numberOfRecurringNames.Add(player.Name, 1);
                }

                Players.Add(player);
                Scores[player.Name] = 0;
            }
        }

        // Plays a whole game of Pul, returns the winning player
        public Player StartGame()
        {
            // Hold who should start the round
            CurrentDealerIndex = 0;

            // Plays a number of rounds equal to how big the constant TotalNumOfRounds is
            for (int round = 1; round < TotalNumOfRounds; round++)
            {
                CreateDeck();
                ShuffleDeck();

                // numOfStacks is how many stacks should be played this round, equal to how many cards each player has
                int numOfStacks = DealCards(round);
                // Gets the bets of every player of how many stacks they think they will win
                GetPlayerBets();

                // Returns how many stacks each player won
                PlayRound(numOfStacks);

                UpdateScores();

                CurrentDealerIndex = (CurrentDealerIndex + 1) % Players.Count;
            }

            return DetermineWinner();
        }

        // Handles a single round of the game
        private void PlayRound(int numOfStacks)
        {
            PlayerWonStacks = new int[Players.Count];
            for (int i = 0; i < numOfStacks; i++)
            {
                PlayersCardInStack = new Dictionary<Card, string>(Players.Count);
                List<Card> currentStack = new List<Card>(Players.Count);
                Suit currentSuit = Suit.Joker;

                // Let all players put one card into the currentStack, currentDealerIndex shifts which player is starting
                for (int j = CurrentDealerIndex; j < Players.Count + CurrentDealerIndex; j++)
                {
                    int tempCurrentIndex;

                    // Maybe fix this later,
                    // Nah don't fix what isn't broken
                    if (j >= Players.Count) tempCurrentIndex = j - Players.Count;
                    else tempCurrentIndex = j;

                    Player currentPlayersTurn = Players[tempCurrentIndex];
                    Players[tempCurrentIndex].CurrentSuit = currentSuit;

                    // Get a card from the current player and check if it tried to cheat
                    Card nextStackCard = currentPlayersTurn.CardToStack(currentStack);
                    if (!IsCardEligible(nextStackCard, currentSuit, Trumfen.Suit, PlayerHands[currentPlayersTurn.Name]))
                    {
                        throw new Exception("Player: " + currentPlayersTurn.Name + ", tried to cheat");
                    }

                    // If they didn't cheat, add it on the pile and remove it from their hand
                    else
                    {
                        PlayerHands[currentPlayersTurn.Name].Remove(nextStackCard);
                        FindPlayer(currentPlayersTurn.Name).Hand.Remove(nextStackCard);
                        currentStack.Add(nextStackCard);
                        PlayersCardInStack.Add(nextStackCard, currentPlayersTurn.Name);

                        // If they are the first player to throw a card into the pile
                        // Then make the currentSuit their cards suit
                        if (Players[CurrentDealerIndex].Name == currentPlayersTurn.Name)
                        {
                            currentSuit = nextStackCard.Suit;
                        }
                    }
                }

                // Decide the most valuable card based on the rules
                Card bestCard = BestCard(currentStack);
                PlayerWonStacks[Players.IndexOf(FindPlayer(PlayersCardInStack[bestCard]))]++;
            }
        }

        // Updates each players score by 10 + how many stacks they thought they were going to win if that was true
        private void UpdateScores()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (PlayerWonStacks[i] == PlayerBets[Players[i].Name])
                {
                    Scores[Players[i].Name] += PlayerWonStacks[i] + 10;
                }
            }
        }

        // Goes through the currentStack to find the best card based on the rule set
        private Card BestCard(List<Card> currentStack)
        {
            // Rules:
            // The Actual most valuable card is Joker (the last in the pile)
            // The next most valuable card is of Trumf suit
            // Third most valuable card is of currentSuit (AKA the suit of the first card in the pile)
            // If all other conditions fail then the card of the highest rank is the most valuable if they aren't of any of the other suits
            Suit currentSuit = currentStack[0].Suit;
            Card bestCard = currentStack[0];
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

        // Gets how many sticks the players think they are going to win by calling the BidAmount in each player
        private void GetPlayerBets()
        {
            PlayerBets = new Dictionary<string, int>(Players.Count);
            foreach (Player player in Players)
            {
                PlayerBets[player.Name] = player.BidAmount();
            }
        }

        // Returns the winning player by picking the player with the highest score
        private Player DetermineWinner()
        {
            int bestScore = 0;
            string bestPlayer = "";
            foreach (var score in Scores)
            {
                if (score.Value > bestScore)
                {
                    bestScore = score.Value;
                    bestPlayer = score.Key;
                }
            }

            return FindPlayer(bestPlayer);
        }

        // Creates the deck for use in the game
        private void CreateDeck()
        {
            const int NumOfJokers = 3;

            Deck = new List<Card>(52 + NumOfJokers);
            int currentId = 0;

            // Create all 52 cards in a deck
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                if (suit != Suit.Joker)
                {
                    foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                    {
                        Deck.Add(new Card(suit, rank, currentId));
                        currentId++;
                    }
                }
            }
            
            // Adds the amount of jokers equal to NumOfJokers
            for (int i = 0; i < NumOfJokers; i++)
            {
                Deck.Add(new Card(Suit.Joker, Rank.Ace, currentId));
                currentId++;
            }
        }

        // Randomizes the deck by picking two random cards in the pile and switching their places
        // Doing this TimesToShuffle constant amount of times
        private void ShuffleDeck()
        {
            const int TimesToShuffle = 1000;

            for (int i = 0; i < TimesToShuffle; i++)
            {
                int card1 = Random.Next(Deck.Count);
                int card2 = Random.Next(Deck.Count);
                Card temp = Deck[card1];
                Deck[card1] = Deck[card2];
                Deck[card2] = temp;
            }
        }

        // Deals cards to all players and returns how many sticks that are gonna be played during the current round
        private int DealCards(int round)
        {
            // Decide how many cards this round should have
            int maxCardsThisRound = NumberOfStacks(round);
            if (Players.Count * maxCardsThisRound > Deck.Count)
            {
                for (int i = maxCardsThisRound; i > 0; i--)
                {
                    if (Players.Count * i < Deck.Count)
                    {
                        maxCardsThisRound = i;
                    }
                }
            }

            // Reset the hand in each player
            foreach (Player player in Players)
            {
                PlayerHands[player.Name] = new List<Card>(maxCardsThisRound);
                player.Hand = new List<Card>(maxCardsThisRound);
            }

            // Add a card equal to maxCardsThisRound to each player
            for (int i = 0; i < maxCardsThisRound; i++)
            {
                foreach (Player player in Players)
                {
                    Card topCard = TakeTopCard();
                    PlayerHands[player.Name].Add(topCard);
                    player.Hand.Add(topCard);
                }
            }

            // Trumfen is the last top card after giving all the players cards
            Trumfen = TakeTopCard();

            // Add the Trumfen card to all players
            foreach (Player player in Players)
            {
                player.Trumfen = Trumfen;
            }

            // Returns how many sticks that are gonna be played this round, which is  how many cards each player has on their hand
            return maxCardsThisRound;
        }

        // Takes the card from the top of the pile
        private Card TakeTopCard()
        {
            Card topCard = Deck[0];
            Deck.RemoveAt(0);

            return topCard;
        }

        // Checks how many cards each player should have this round, or how many stacks will be played this round
        private int NumberOfStacks(int round)
        {
            if (round > 10)
                return 1 + TotalNumOfRounds - round;
            else
                return round;
        }

        // For use of finding player by name, uses Lamba
        private Player FindPlayer(string playerName)
        {
            return Players.Find(player => player.Name == playerName);
        }

        // Method for checking a card if it is allowed in the pile
        public static bool IsCardEligible(Card nextStackCard, Suit currentSuit, Suit trumfSuit, List<Card> hand)
        {
            // First, check if it has the card in it's hand
            // Second, check if the card is of the current suit, if it isn't then check if it has another card of the current suit
            // Third, if the card isn't of the current suit, and that card isn't also of the trumfSuit then check if it has another card of trumf suit
            Card cardOnHandCurrentSuit = hand.Find(card => card.Suit == currentSuit);
            Card cardOnHandTrumfSuit = hand.Find(card => card.Suit == trumfSuit);

            // First check
            if (nextStackCard != hand.Find(card => card == nextStackCard))
            {
                return false;
            }

            // Second check
            else if (currentSuit != Suit.Joker && nextStackCard.Suit != currentSuit)
            {
                if (cardOnHandCurrentSuit != null && currentSuit == cardOnHandCurrentSuit.Suit)
                {
                    return false;
                }

                // Third check
                else if (trumfSuit != Suit.Joker && nextStackCard.Suit != trumfSuit)
                {
                    if (cardOnHandTrumfSuit != null && trumfSuit == cardOnHandTrumfSuit.Suit)
                    {
                        return false;
                    }
                }
            }

            // Else it's eligible
            return true;
        }
    }

    class Card
    {
        public Suit Suit { get; private set; }
        public Rank Rank { get; private set; }
        public int Id { get; private set; }

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
    }

    abstract class Player
    {
        public string Name;
        
        // It doesn't matter if the hand changes, the "real" hand is privatly in a dictionary in the Game class
        public List<Card> Hand;
        public Card Trumfen;
        public Suit CurrentSuit;

        public abstract int BidAmount();
        public abstract Card CardToStack(List<Card> currentStack);
        public override string ToString()
        {
            return Name;
        }
    }

    class SmallBidder : Player
    {
        public SmallBidder()
        {
            Name = "SmallBidder";
        }

        public override int BidAmount()
        {
            return 0;
        }

        public override Card CardToStack(List<Card> currentStack)
        {
            foreach(Card card in Hand)
            {
                if (PulFunctionality.IsCardEligible(card, CurrentSuit, Trumfen.Suit, Hand)) return card;
            }

            return Hand[0];
        }
    }
}
