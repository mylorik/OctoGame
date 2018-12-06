﻿using System.Collections.Generic;
using System.Threading.Tasks;
using OctoGame.LocalPersistentData.UsersAccounts;

namespace OctoGame.OctoGame.SpellHandling.Buffs
{
    public class AllBuffs
    {
        private readonly IUserAccounts _accounts;
        static Dictionary<List<>>
        public AllBuffs(IUserAccounts accounts)
        {
            _accounts = accounts;
        }

        public async Task CheckForDeBuffs(AccountSettings account)
        {
            if (account.InstantDeBuff.Count > 0)
                for (var i = 0; i < account.InstantDeBuff.Count; i++)
                {
                    account.InstantDeBuff[i].forHowManyTurns--;


                    switch (account.InstantDeBuff[i].skillId)
                    {
                        case 1003:
                            if (!account.InstantDeBuff[i].activated)
                                account.Armor -= 2;
                            if (account.InstantDeBuff[i].forHowManyTurns <= 0)
                                account.Armor += 2;
                            break;
                    }

                    account.InstantDeBuff[i].activated = true;
                    if (account.InstantDeBuff[i].forHowManyTurns <= 0)
                    {
                        account.InstantDeBuff.RemoveAt(i);
                        _accounts.SaveAccounts(account.DiscordId);
                    }
                }

            _accounts.SaveAccounts(account.DiscordId);
           await CheckForBuffsToBeActivatedLater(account);
            await Task.CompletedTask;
        }

        public async Task CheckForBuffsToBeActivatedLater(AccountSettings account)
        {
            if (account.BuffToBeActivatedLater.Count > 0)
                for (var i = 0; i < account.BuffToBeActivatedLater.Count; i++)
                {
                    account.BuffToBeActivatedLater[i].afterHowManyTurns--;

                    switch (account.BuffToBeActivatedLater[i].skillId)
                    {
                        //1079 (танк ветка - ульта) Get cancer - после 20и ходов отхиливает фул выносливость +1% за каждые 20 уровней, снимает все дебафы 
                        case 1079:
                            if (account.BuffToBeActivatedLater[i].afterHowManyTurns <= 0)
                            {
                                account.Stamina = account.MaxStamina + (account.OctoLvL / 20) * account.MaxStamina / 100;
                                account.InstantDeBuff = new List<AccountSettings.InstantBuffClass>();
                                account.DeBuffToBeActivatedLater = new List<AccountSettings.OnTimeBuffClass>();
                            }
                            break;
                    }
                
                }
            _accounts.SaveAccounts(account.DiscordId);
            await Task.CompletedTask;
        }

        public async Task CheckForDeBuffsToBeActivatedLater(AccountSettings account)
        {
            await Task.CompletedTask;
        }

        public async Task CheckForBuffs(AccountSettings account)
        {
            if(account.InstantBuff.Count > 0)
                for (var i = 0; i < account.InstantBuff.Count; i++)
                {
                    account.InstantBuff[i].forHowManyTurns--;

                    switch (account.InstantBuff[i].skillId)
                    {
                        case 4856757499:
                            break;
                        //1089 (танк ветка) Истинный воин - повышает силу в 2 раза на следующие 2 хода (но уменьшает скейл ад от силы в 2 раза) 
                        case 1089:
                            if (!account.InstantBuff[i].activated)
                            {
                                account.Strength = account.Strength * 2;
                                account.InstantBuff[i].activated = true;
                            }

                            if (account.InstantBuff[i].activated)
                            {

                            }
                            break;

                    }
                }
            _accounts.SaveAccounts(account.DiscordId);
            await Task.CompletedTask;
        }

    }
}