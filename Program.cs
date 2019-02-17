using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ProperDiceBot
{
    class Program
    {
        static TelegramBotClient ProperDiceBot;
        static Random rnd;

        static string helpMessage = "Roll the dice!" + Environment.NewLine + Environment.NewLine + "For example:" + Environment.NewLine + "/roll 3d12" + Environment.NewLine + "/roll (for 1d20)" + Environment.NewLine + "/roll 5 (for 5d20)" + Environment.NewLine + "/roll d12 (for 1d12)" + Environment.NewLine + Environment.NewLine + "Maximum eyes (dice size) is 100, maximum dice (count) is 50." + Environment.NewLine + Environment.NewLine + "Questions? Poke @NoxiousPluK!";

        static void Main(string[] args)
        {
            ProperDiceBot = new TelegramBotClient("--YOUR API KEY HERE--");
            rnd = new Random();
            Console.Write("Starting bot... ");
            ProperDiceBot.OnMessage += ProperDiceBot_OnMessage;
            var me = ProperDiceBot.GetMeAsync().Result;
            ProperDiceBot.StartReceiving();

            Console.WriteLine($"Connected as userID {me.Id} and name {me.FirstName}.");
            Console.WriteLine("Press Q to quit!");

            bool Running = true;
            while (Running)
            {
                var X = Console.ReadKey();
                if (X.Key.ToString() == "q")
                {
                    Console.WriteLine("Bye!");
                    Running = false;
                }
            }

        }

        static async void ProperDiceBot_OnMessage(object sender, MessageEventArgs e)
        {
            int dice = 1;
            int size = 20;

            if (e.Message.Text != null)
            {
                if (e.Message.Text.StartsWith("/roll"))
                {
                    var match = Regex.Matches(e.Message.Text, @"\/roll[ ]?(\d+)?[d]?(\d+)?$");
                    if (match.Count > 0)
                    {
                        foreach (Match M in match)
                        {
                            dice = (M.Groups[1].Value.ToString() == "" ? 1 : int.Parse(M.Groups[1].Value));
                            size = (M.Groups[2].Value.ToString() == "" ? 20 : int.Parse(M.Groups[2].Value));
                            int lowest = size;
                            int highest = 0;
                            if (dice < 51 && size < 101 && dice > 0 && size > 1)
                            {
                                string returnMessage = "🎲 <b>Dice rolls (" + dice.ToString() + "d" + size.ToString() + "):</b>" + Environment.NewLine;
                                int totalValue = 0;
                                for (int i = 0; i < dice; i++)
                                {
                                    int diceResult = DiceRoll(size);
                                    if (diceResult < lowest)
                                    {
                                        lowest = diceResult;
                                    }
                                    if (diceResult > highest)
                                    {
                                        highest = diceResult;
                                    }
                                    totalValue += diceResult;
                                    returnMessage += "<b>#" + (i + 1).ToString() + ":</b> " + diceResult.ToString() + Environment.NewLine;
                                    Console.WriteLine(FormName(e.Message.From) + " rolled " + dice.ToString() + "d" + size.ToString() + ".");
                                }
                                if (dice != 1)
                                {
                                    returnMessage += Environment.NewLine + "#️⃣ <b>Total: </b>" + totalValue.ToString();
                                    returnMessage += Environment.NewLine + "🔼 <b>Highest: </b>" + highest.ToString();
                                    returnMessage += Environment.NewLine + "🔽 <b>Lowest: </b>" + lowest.ToString();
                                    returnMessage += Environment.NewLine + "↕️ <b>Average: </b>" + String.Format("{0:0.##}", Decimal.Divide(totalValue, dice));
                                }

                                await ProperDiceBot.SendTextMessageAsync(e.Message.Chat.Id, returnMessage, ParseMode.Html, false, false, e.Message.MessageId);
                            }
                            else
                            {
                                await ProperDiceBot.SendTextMessageAsync(e.Message.Chat.Id, helpMessage, ParseMode.Html, false, false, e.Message.MessageId);
                            }
                        }
                    }
                    else
                    {
                        await ProperDiceBot.SendTextMessageAsync(e.Message.Chat.Id, helpMessage, ParseMode.Html, false, false, e.Message.MessageId);
                    }
                }
            }
        }

        static int DiceRoll(int eyes)
        {
            return rnd.Next(1, eyes + 1);
        }

        static String FormName(User tgUser)
        {
            String nameString = "";
            if (tgUser.FirstName != null && tgUser.FirstName.Length > 0)
            {
                nameString += tgUser.FirstName;
            }

            if (tgUser.LastName != null && tgUser.LastName.Length > 0)
            {
                nameString += " " + tgUser.LastName;
            }

            if (tgUser.Username != null && tgUser.Username.Length > 0)
            {
                nameString += " (@" + tgUser.Username + ")";
            }
            return nameString;
        }
    }
}