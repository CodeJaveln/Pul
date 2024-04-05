using System;
using System.Collections.Generic;
using System.Linq;

namespace Pul
{
    /// <summary>
    /// Contains the main game logic for Pul.
    /// </summary>
    class PulFunctions
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
        private Deck Deck;
        /// <summary>
        /// Is the index of which player that is dealer of the round and gets to choose the current suit with their <see cref="Card"/>.
        /// </summary>
        public int CurrentDealerIndex { get; private set; }

        /// <summary>
        /// Prepares the game by initialising new lists and dictionaries and puts <paramref name="players"/> into <see cref="Players"/>.
        /// </summary>
        /// <param name="players">Are the <see cref="Player"/> objects that are going to play.</param>
        public PulFunctions(Random random, params Player[] players)
        {
            Deck = new Deck(random);

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
        /// <exception cref="ArgumentException"></exception>
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
                        throw new ArgumentException($"Player: {currentPlayersTurn.Name}, tried to cheat by {exceptionCause}.");
                    }

                    else
                    {
                        PlayerHands[currentPlayersTurn.Name].Remove(nextStackCard);
                        FindPlayer(currentPlayersTurn.Name).Hand.Remove(nextStackCard);
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

        /// <summary>
        /// Updates the player scores based on the amount of stacks they won.
        /// </summary>
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

        /// <summary>
        /// Checks the stack and returns the best card played.
        /// </summary>
        /// <param name="currentStack">Is the list of cards played during the current stack.</param>
        /// <returns>The best card in the stack.</returns>
        private Card BestCard(List<Card> currentStack)
        {
            Card bestCard = currentStack[0];
            Suit currentSuit = bestCard.Suit;
            foreach (Card card in currentStack)
            {
                if (card.Suit == Suit.Joker)
                {
                    bestCard = card; // The latest joker is always best.
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

        /// <summary>
        /// Takes in a player name and returns the player object with the same name.
        /// </summary>
        /// <param name="name">Name of a <see cref="Player"/> instance in <see cref="Players"/></param>
        /// <returns>A player instance with the same name from <see cref="Players"/>.</returns>
        private Player FindPlayer(string name)
        {
            return Players.Find(player => player.Name == name);
        }

        /// <summary>
        /// Gets what each <see cref="Player"/> instance in <see cref="Players"/> bets.
        /// </summary>
        private void GetPlayerBets()
        {
            PlayerBets = new Dictionary<string, int>(Players.Count);
            foreach (Player player in Players)
            {
                PlayerBets[player.Name] = player.StickBidAmount();
            }
        }

        /// <summary>
        /// Checks all scores of <see cref="Players"/> and returns the winners.
        /// </summary>
        /// <returns>A list of players who won the game.</returns>
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
        /// Gets how many cards should be dealt, how many can be dealt, and deals it to all <see cref="Players"/>.
        /// </summary>
        /// <param name="round">The current round in the game.</param>
        /// <returns>How many stacks that can be played in the current round.</returns>
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
}