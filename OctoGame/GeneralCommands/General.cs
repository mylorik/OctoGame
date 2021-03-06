﻿using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordBotsList.Api;
using OctoGame.DiscordFramework;
using OctoGame.DiscordFramework.Extensions;
using OctoGame.DiscordFramework.Language;
using OctoGame.Helpers;
using OctoGame.LocalPersistentData.UsersAccounts;

namespace OctoGame.GeneralCommands
{
    public class General : ModuleBaseCustom
    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.


        readonly AuthDiscordBotListApi _dblApi = new AuthDiscordBotListApi(423593006436712458, "...");

        private readonly ILocalization _lang;
        private readonly UserAccounts _accounts;
        private readonly SecureRandom _secureRandom;
        private readonly OctoPicPull _octoPicPull;
        private readonly OctoNamePull _octoNmaNamePull;
        private readonly HelperFunctions _helperFunctions;
        private readonly AudioService _service;
        private readonly CommandsInMemory _commandsInMemory;
        private readonly Global _global;

  
        public General(ILocalization lang, UserAccounts accounts, SecureRandom secureRandom, OctoPicPull octoPicPull, OctoNamePull octoNmaNamePull, HelperFunctions helperFunctions, AudioService service, CommandsInMemory commandsInMemory, Global global) 
        {
            _lang = lang;
            _accounts = accounts;

            _secureRandom = secureRandom;
            _octoPicPull = octoPicPull;
            _octoNmaNamePull = octoNmaNamePull;
            _helperFunctions = helperFunctions;
            _service = service;
            _commandsInMemory = commandsInMemory;
            _global = global;
        }

        [Command("ddw")]
        [Summary("Download all Octo Pictures.")]
        [RequireOwner]
        public async Task Ddw()
        {
            using var client = new WebClient();
            foreach (var v in _octoPicPull.OctoPics)
            {
                try
                {
                    var s = v.Split('/');
                    client.DownloadFile(v, $"Octopuses/{s[^1]}");

                }
                catch
                {
                    SendMessAsync($"download it manually `{v}`");
                }

            }

            foreach (var v in _octoPicPull.OctoPicsPull)
            {
                try
                {
                    var s = v.Split('/');
                client.DownloadFile(v, $"Octopuses/{s[^1]}");
                }
                catch
                {
                    SendMessAsync($"download it manually `{v}`");
                }
            }
            SendMessAsync($"done");
        }

        [Command("tt")]
        [Summary("doing absolutely nothing. That's right - NOTHING")]
        public async Task Ttest([Remainder]string st = null)
        {
            var acc = _accounts.GetAccount(Context.User);
            acc.Attack_Tree = st; 
            _accounts.SaveAccounts(Context.User);

            SendMessAsync($"updated");
            // Context.User.GetAvatarUrl()
        }


        [Command("updMaxRam")]
        [RequireOwner]
        [Summary("updates maximum number of commands bot will save in memory (default 1000 every time you launch this app)")]
        public async Task ChangeMaxNumberOfCommandsInRam(uint number)
        {
            _commandsInMemory.MaximumCommandsInRam = number;
            SendMessAsync($"now I will store {number} of commands");
        }

        [Command("clearMaxRam")]
        [RequireOwner]
        [Summary("CAREFUL! This will delete ALL commands in ram")]
        public async Task ClearCommandsInRam()
        {
            var toBeDeleted = _commandsInMemory.CommandList.Count;
            _commandsInMemory.CommandList.Clear();
            SendMessAsync($"I have deleted {toBeDeleted} commands");
        }

      

        [Command("uptime")]
        [Summary("showing general info about the bot")]
        public async Task UpTime()
        {
            _global.TimeSpendOnLastMessage.TryGetValue(Context.User.Id, out var watch);

            var time = DateTime.Now -_global.TimeBotStarted;
            var ramUsage = Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024);

            var embed = new EmbedBuilder()
               // .WithAuthor(Context.Client.CurrentUser)
               // .WithTitle("My internal statistics")
                .WithColor(Color.DarkGreen)
                .WithCurrentTimestamp()
                .WithFooter("Version: 0.0a | Thank you for using me!")
                .AddField("Numbers:", "" +
                                      $"Working for: {time.Days}d {time.Hours}h {time.Minutes}m and {time:ss\\.fff}s\n" +
                                      $"Total Games Started: {_global.OctoGamePlaying}\n" +
                                      $"Total Commands issued while running: {_global.TotalCommandsIssued}\n" +
                                      $"Total Commands changed: {_global.TotalCommandsChanged}\n" +
                                      $"Total Commands deleted: {_global.TotalCommandsDeleted}\n" +
                                      $"Total Commands in memory: {_commandsInMemory.CommandList.Count} (max {_commandsInMemory.MaximumCommandsInRam})\n" +
                                      $"Client Latency: {_global.Client.Latency}\n" +
                                      $"Time I spend processing your command: {watch?.Elapsed:m\\:ss\\.ffff}s\n" +
                                      $"This time counts from from the moment he receives this command.\n" +
                                      $"Memory Used: {ramUsage}\n" +
                                      $"Total Servers: {_global.Client.Guilds.Count}");


            SendMessAsync(embed);

            // Context.User.GetAvatarUrl()
        }


       
        [Command("upd")]
        [RequireOwner]
        [Summary("UpdateDiscordBotListGuildCount")]
        public async Task UpdateDiscordBotListGuildCount(int num)
        {
            _dblApi.UpdateStats(num);
            SendMessAsync( $"updated");
        }
        

        [Command("join", RunMode = RunMode.Async)]
        [Summary("Joins a voice channel")]
        public async Task JoinCmd()
        {
            await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState)?.VoiceChannel);
        }

        [Command("leave", RunMode = RunMode.Async)]
        [Summary("leaves a voice channel")]
        public async Task LeaveCmd()
        {
           await _service.LeaveAudio(Context.Guild);
        }
    
        [Command("play", RunMode = RunMode.Async)]
        [Summary("plays only 1 song. for now. Just because.")]
        public async Task PlayCmd([Remainder] string song = "nothing")
        {
            await _service.SendAudioAsync(Context.Guild, Context.Channel, song);
        }



        [Command("octo")]
        [Alias("окто", "octopus", "Осьминог", "Осьминожка", "Осьминога", "o", "oct", "о")]
        [Summary("Show a random octo. The pull contains only my own pictures.")]
        public async Task OctopusPicture()
        {
            var octoIndex = _secureRandom.Random(0, _octoPicPull.OctoPics.Length-1);
            var octoToPost = _octoPicPull.OctoPics[octoIndex];


            var color1Index = _secureRandom.Random(0, 255);
            var color2Index = _secureRandom.Random(0, 255);
            var color3Index = _secureRandom.Random(0, 255);

            var randomIndex = _secureRandom.Random(0, _octoNmaNamePull.OctoNameRu.Length-1);
            var randomOcto = _octoNmaNamePull.OctoNameRu[randomIndex];

            var embed = new EmbedBuilder();
            embed.WithDescription($"{randomOcto} found:");
            embed.WithFooter("lil octo notebook");
            embed.WithColor(color1Index, color2Index, color3Index);
            embed.WithAuthor(Context.User);
            embed.WithImageUrl("" + octoToPost);

            await SendMessAsync( embed);

            switch (octoIndex)
            {
                case 19:
                    {
                        var lll = await Context.Channel.SendMessageAsync("Ooooo, it was I who just passed Dark Souls!");
                        _helperFunctions.DeleteMessOverTime(lll, 6);
                        break;
                    }
                case 9:
                    {
                        var lll = await Context.Channel.SendMessageAsync("I'm drawing an octopus :3");
                        _helperFunctions.DeleteMessOverTime(lll, 6);
                        break;
                    }
                case 26:
                    {
                        var lll = await Context.Channel.SendMessageAsync(
                            "Oh, this is New Year! time to gift turtles!!");
                        _helperFunctions.DeleteMessOverTime(lll, 6);
                        break;
                    }
            }
        }


        [Command("test")]
        [Summary("just a test of a multiple language shit, does not work")]
        public async Task TestLanguges([Remainder] string rem)
        {
            var kek = _lang.Resolve($"{Context.User.Mention}\n[PRIVACY_DATA_REPORT_TEMPLATE(@DATA)]");
            SendMessAsync( $"{kek}");
        }
    }
}