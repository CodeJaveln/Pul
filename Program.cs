using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Services;

namespace Pul
{
    internal class Program
    {
        static void Main(string[] args)
        {
            List<Player> players = new List<Player>();
            List<Type> playerTypes = new List<Type>(Assembly.GetAssembly(typeof(Player)).GetTypes().Where((myType) => myType.IsClass && myType.IsSubclassOf(typeof(Player))));

            for (int i = 0; i < playerTypes.Count; i++)
            {
                Console.WriteLine($"{i} {playerTypes[i].Name}");
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

            Console.Write("Enter the amount of games that should be played: ");
            int numberOfGames;
            while (!int.TryParse(Console.ReadLine(), out numberOfGames))
            {
                Console.WriteLine("Incorrect input.");
                Console.Write("Enter an integer: ");
            }

            PulFunctions game = new PulFunctions(players.ToArray());
            Dictionary<Player, int> playerWins = new Dictionary<Player, int>();
            Dictionary<Player, int> playerY_Pos = new Dictionary<Player, int>();
            Dictionary<Player, ConsoleColor> playerColor = new Dictionary<Player, ConsoleColor>();
            
            Console.Clear();
            int yPos = 0;
            ConsoleColor consoleColor = ConsoleColor.Blue;
            int longestName = 0;
            foreach (Player player in players)
            {
                Console.SetCursorPosition(0, yPos);
                Console.WriteLine($"{player.Name}: ");
                playerWins.Add(player, 0);
                playerY_Pos.Add(player, yPos);
                playerColor.Add(player, consoleColor);
                yPos += 2;
                consoleColor++;
                if (player.Name.Length > longestName)
                    longestName = player.Name.Length;
            }

            for (int i = 1; i <= numberOfGames; i++)
            {
                List<Player> WonPlayers = game.PlayGame();
                foreach (Player player in WonPlayers)
                {
                    Console.ForegroundColor = playerColor[player];
                    playerWins[player]++;
                    Console.SetCursorPosition((playerWins[player] * 100 / numberOfGames) + longestName + 2, playerY_Pos[player]);
                    Console.Write("█");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"{playerWins[player]}");
                }
            }

            Console.ReadLine();
        }
    }
}
