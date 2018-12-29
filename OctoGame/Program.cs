﻿using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using OctoGame.DiscordFramework;
using OctoGame.DiscordFramework.Language;
using OctoGame.Helpers;
using OctoGame.LocalPersistentData.GameSpellsAccounts;
using OctoGame.LocalPersistentData.LoggingSystemJson;
using OctoGame.LocalPersistentData.ServerAccounts;
using OctoGame.LocalPersistentData.UsersAccounts;
using OctoGame.OctoGame.GamePlayFramework;
using OctoGame.OctoGame.ReactionHandling;
using OctoGame.OctoGame.SpellHandling.ActiveSkills;
using OctoGame.OctoGame.SpellHandling.BonusDmgHandling;
using OctoGame.OctoGame.SpellHandling.Buffs;
using OctoGame.OctoGame.SpellHandling.DmgReductionHandling;
using OctoGame.OctoGame.SpellHandling.PassiveSkills;
using OctoGame.OctoGame.UpdateMessages;

namespace OctoGame
{
    public class ProgramOctoGame
    {
        private DiscordShardedClient _client;
        private IServiceProvider _services;
        
        private readonly int[] _shardIds = {0, 1};

        private static void Main()
        {
            new ProgramOctoGame().RunBotAsync().GetAwaiter().GetResult();
        }

        public async Task RunBotAsync()
        {
            if (string.IsNullOrEmpty(Config.Bot.Token)) return;
            _client = new DiscordShardedClient(_shardIds,new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                DefaultRetryMode = RetryMode.AlwaysRetry,
                MessageCacheSize = 50,
                TotalShards = 2
            });

            _services = ConfigureServices();
            _services.GetRequiredService<DiscordEventHandler>().InitDiscordEvents();
            await _services.GetRequiredService<CommandHandeling>().InitializeAsync();
            
            var botToken = Config.Bot.Token;
            await _client.SetGameAsync("Boole~");

            await _client.LoginAsync(TokenType.Bot, botToken);
            await _client.StartAsync();
           

            SendMessagesUsingConsole.ConsoleInput(_client);
            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {            
            return new ServiceCollection()
                .AddSingleton(_client)

                .AddSingleton<OctoPicPull>()
                .AddSingleton<OctoNamePull>()
                .AddSingleton<Global>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandeling>()
                .AddSingleton<DiscordEventHandler>()
                .AddSingleton<MagicReduction>()
                .AddSingleton<ArmorReduction>()
                .AddSingleton<Dodge>()
                .AddSingleton<Crit>()

                .AddSingleton<AgilityActiveTree>()
                .AddSingleton<AgilityPassiveTree>()
                .AddSingleton<AttackDamageActiveTree>()
                .AddSingleton<AttackDamagePassiveTree>()
                .AddSingleton<DefenceActiveTree>()
                .AddSingleton<DefencePassiveTree>()
                .AddSingleton<MagicActiveTree>()
                .AddSingleton<MagicPassiveTree>()
                .AddSingleton<AllBuffs>()
                //.AddTransient<666666666>()
                       .AddSingleton<AudioService>()
            

                .AddSingleton<OctoGameReaction>()
                .AddSingleton<OctoGameUpdateMess>()
                .AddSingleton<AttackDamageActiveTree>()          
                .AddSingleton<CustomCalculator>()
                .AddSingleton<HelperFunctions>()
                .AddTransient<SecureRandom>()
                .AddTransient<AwaitForUserMessage>()
                .AddSingleton<GameFramework>()

                .AddTransient<IDataStorage, JsonLocalStorage>()
                .AddTransient<ILocalization, JsonLocalization>()
                .AddTransient<IUserAccounts, UserAccounts>()
                .AddTransient<IServerAccounts, ServerAccounts>()
                .AddTransient<ILoggingSystem, LoggingSystem>()
                .AddTransient<ISpellAccounts, SpellUserAccounts>()
                .BuildServiceProvider();
        }
    }
}