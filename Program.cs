using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pul
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Random random = new Random();
            const int timesRunGame = 1000;
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


            PulFunctions game = new PulFunctions(random, players.ToArray());
            for (int i = 1; i <= timesRunGame; i++)
            {
                List<Player> WonPlayers = game.StartGame();
                foreach (Player player in WonPlayers)
                {
                    Console.WriteLine($"Player {player} won round {i}");
                }
                Console.WriteLine();
            }

            Console.ReadKey();
        }
    }
}
