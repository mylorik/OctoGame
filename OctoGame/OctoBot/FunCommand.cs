﻿using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using OctoGame.DiscordFramework.Extensions;
using OctoGame.Helpers;
using OctoGame.LocalPersistentData.ServerAccounts;
using OctoGame.LocalPersistentData.UsersAccounts;

namespace OctoGame.OctoBot
{
    public class Fun : ModuleBaseCustom
    {
        private readonly UserAccounts _accounts;
        private readonly ServerAccounts _serverAccounts;
        private readonly AwaitForUserMessage _awaitForUserMessage;

        public Fun(UserAccounts accounts, ServerAccounts serverAccounts, AwaitForUserMessage awaitForUserMessage)
        {
            _accounts = accounts;
            _serverAccounts = serverAccounts;
            _awaitForUserMessage = awaitForUserMessage;
        }

        [Command("pick")]
        [Summary("Local Joke.")]
        public async Task Pick([Remainder] string message)
        {
            var option = message.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);

                var rand = new Random();
                var selection = option[rand.Next(0, option.Length)];

                var embed = new EmbedBuilder();
                embed.WithTitle("I chose instead " + Context.User.Username);
                embed.WithDescription(selection);

                embed.WithColor(new Color(255, 0, 94));
                embed.WithThumbnailUrl("https://i.imgur.com/I3o0bm4.jpg");


                await SendMessAsync( embed);
            }

        [Command("ping")]
        [Alias("пинг")]
        [Summary("Local Joke.")]
        public async Task DefaultPing()
        {
            await SendMessAsync( $"{Context.User.Mention} pong!");
        }


        [Command("DM")]
        [Summary("A bot will send you a DM")]
        public async Task DmMess()
        {
            try
            {
                var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
                await dmChannel.SendMessageAsync("Boole.");
            }
            catch
            {
             //   await ReplyAsync(
             //       "boo... An error just appear >_< \nTry to use this command properly: **DM**(sends you a DM)\n");
            }
        }

        [Command("guess", RunMode = RunMode.Async)]
        [Alias("Рулетка", "угадайка")]
        [Summary("Guess Game")]
        public async Task GuessGame(ulong enter)
        {

                var amount = (int) enter;

                var userAccount = _accounts.GetAccount(Context.User);
                var octoAcccount = _accounts.GetAccount(Context.Guild.CurrentUser);

                if (amount > userAccount.Points || amount <= 0)
                {
                    await SendMessAsync(
                        "You do not have enough OktoPoints! Or you just entered something wrong.");

                    return;
                }


                var randSlot = new Random();
                var slots = randSlot.Next(72);


                await SendMessAsync(
                    $"Number of slots **{slots}**. What is your choice?");

                var response = await _awaitForUserMessage.AwaitMessage(Context.User.Id, Context.Channel.Id, 10000);

                var result = int.TryParse(response.Content, out _);
                if (result)
                {
                    var choise = Convert.ToInt32(response.Content);
                    var bank = Math.Abs(amount * slots / 5);


                    var rand = new Random();
                    var random = rand.Next(slots);

                    if (choise == random)
                    {
                        userAccount.Points += bank;
                        

                        await SendMessAsync(
                            $"You won **{bank}** OctoPoints!\nNow you have **{userAccount.Points}** OctoPoints!");

                        userAccount.Points += bank;
                        
                    }
                    else
                    {
                        await SendMessAsync(
                            $"booole. Yuor **{amount}** OctoPoints stayed with us. Btw, number was **{random}**");


                        userAccount.Points -= amount;
                        octoAcccount.Points += amount;
                        
                    }
                }
                else


                {
                    await SendMessAsync(
                        $"The choice should be between 0 and {slots}, answer only with a number.");
                }
        }



    }
}