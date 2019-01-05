﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using OctoGame.Helpers;
using OctoGame.LocalPersistentData.GameSpellsAccounts;
using OctoGame.LocalPersistentData.UsersAccounts;
using OctoGame.OctoGame.SpellHandling.ActiveSkills;
using OctoGame.OctoGame.SpellHandling.BonusDmgHandling;
using OctoGame.OctoGame.SpellHandling.Buffs;
using OctoGame.OctoGame.SpellHandling.DmgReductionHandling;
using OctoGame.OctoGame.SpellHandling.PassiveSkills;
using OctoGame.OctoGame.UpdateMessages;

namespace OctoGame.OctoGame.GamePlayFramework
{
    public class GameFramework
    {
        private readonly IUserAccounts _accounts;
        private readonly ISpellAccounts _spellAccounts;
        private readonly OctoGameUpdateMess _octoGameUpdateMess;

        private readonly AttackDamageActiveTree _attackDamageActiveTree;
        private readonly AttackDamagePassiveTree _attackDamagePassiveTree;
        private readonly DefenceActiveTree _defenceActiveTree;
        private readonly DefencePassiveTree _defencePassiveTree;
        private readonly AgilityActiveTree _agilityActiveTree;
        private readonly AgilityPassiveTree _agilityPassiveTree;
        private readonly MagicActiveTree _magicActiveTree;
        private readonly MagicPassiveTree _magicPassiveTree;

        private readonly Global _global;
        private readonly AwaitForUserMessage _awaitForUserMessage;
        private readonly AllBuffs _allDebuffs;

        private readonly Crit _crit;
        private readonly Dodge _dodge;
        private readonly ArmorReduction _armorReduction;
        private readonly MagicReduction _magicReduction;


        public GameFramework(IUserAccounts accounts, OctoGameUpdateMess octoGameUpdateMess,
            AttackDamageActiveTree attackDamageActiveTree, ISpellAccounts spellAccounts, Global global,
            AwaitForUserMessage awaitForUserMessage, MagicReduction magicReduction, ArmorReduction armorReduction,
            Crit crit, Dodge dodge, AttackDamagePassiveTree attackDamagePassiveTree, AllBuffs allDebuffs, DefencePassiveTree defencePassiveTree, DefenceActiveTree defenceActiveTree, AgilityActiveTree agilityActiveTree, AgilityPassiveTree agilityPassiveTree, MagicActiveTree magicActiveTree, MagicPassiveTree magicPassiveTree)
        {
            _accounts = accounts;
            _octoGameUpdateMess = octoGameUpdateMess;
            _attackDamageActiveTree = attackDamageActiveTree;
            _spellAccounts = spellAccounts;
            _global = global;
            _awaitForUserMessage = awaitForUserMessage;
            _magicReduction = magicReduction;
            _armorReduction = armorReduction;
            _crit = crit;
            _dodge = dodge;
            _attackDamagePassiveTree = attackDamagePassiveTree;
            _allDebuffs = allDebuffs;
            _defencePassiveTree = defencePassiveTree;
            _defenceActiveTree = defenceActiveTree;
            _agilityActiveTree = agilityActiveTree;
            _agilityPassiveTree = agilityPassiveTree;
            _magicActiveTree = magicActiveTree;
            _magicPassiveTree = magicPassiveTree;
        }


        public async Task GetSkillDependingOnMoveList(AccountSettings account, AccountSettings enemy,
            SocketReaction reaction, int i)
        {
            var skills = GetSkillListFromTree(account);
            var ski = Convert.ToUInt64(skills[GetSkillNum(reaction) - 1]);
            var skill = _spellAccounts.GetAccount(ski);

            if (account.SkillCooldowns.Any(x => x.skillId == skill.SpellId))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                _awaitForUserMessage.ReplyAndDeleteOvertime("this skill is on cooldown, use another one", 6,
                    reaction);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                return;
            }

            Console.WriteLine($"{skill.SpellNameEn} + {skill.SpellId}");

            double dmg = 0;
            switch (account.MoveListPage)
            {
                    case 1:
                        dmg =   _attackDamageActiveTree.AttackDamageActiveSkills(skill.SpellId, account, enemy, false);
                        break;
                    case 2:
                        dmg = _defenceActiveTree.DefSkills(skill.SpellId, account, enemy, false);
                        break;
                    case 3:
                        dmg = _agilityActiveTree.AgiActiveSkills(skill.SpellId, account, enemy, false);
                        break;
                    case 4:
                        dmg = _magicActiveTree.ApSkills(skill.SpellId, account, enemy, false);
                        break;
            }


            
            await DmgHealthHandeling(skill.WhereDmg, dmg, skill.SpellDmgType, account, enemy);
            await UpdateTurn(account, enemy);


            foreach (var u in _global.OctopusGameMessIdList[account.MessageIdInList])
            {
                if(u.PlayerDiscordAccount != null && u.GamingWindowFromBot != null)
                await _octoGameUpdateMess.MainPage(u.PlayerDiscordAccount.Id, u.GamingWindowFromBot);
            }
        }

        public async Task UpdateTurn(AccountSettings account, AccountSettings enemy)
        {

            //TODO figure out who to update and when 
            await _allDebuffs.CheckForBuffs(account);
            await _allDebuffs.CheckForDeBuffs(enemy);
            await _allDebuffs.CheckForBuffsToBeActivatedLater(account);
            await _allDebuffs.CheckForDeBuffsToBeActivatedLater(enemy);


            if (account.SkillCooldowns != null)
                for (var i = 0; i < account.SkillCooldowns.Count; i++)
                {
                    account.SkillCooldowns[i].cooldown--;
                    if (account.SkillCooldowns[i].cooldown <= 0)
                        account.SkillCooldowns.RemoveAt(i);
                }

            account.Turn = 1;
            enemy.Turn = 0;
            _accounts.SaveAccounts(account.DiscordId);

            await CheckDmgWithTimer(account, enemy);
            await CheckDmgWithTimer(enemy, account);
            await CheckStatsForTime(account, enemy);

            await CheckForPassivesAndUpdateStats(account, enemy);
        }


       
        public async Task DmgHealthHandeling(int dmgWhere, double dmg, int dmgType, AccountSettings myAccount,
            AccountSettings enemyAccount)
        {
            //TODO implemnt
            // >on hit dmg

            /*
             0 = Regular
             1 = To health
             2 = only to stamina
             */
            // type 0 = physic, 1 = magic

            //yes, this is a passive skill, have no idea how to impement it better
            //////////////////////////////////////////////////////////////////////////////////////////
            if (myAccount.InstantBuff.Any(x => x.skillId == 1000) && myAccount.IsFirstHit)
                dmg = dmg * (1 + myAccount.PrecentBonusDmg);
            if (dmg >= 1) myAccount.IsFirstHit = false;
            //////////////////////////////////////////////////////////////////////////////////////////

            if (dmg > 0)
                if (myAccount.IsCrit)
                dmg = _crit.CritHandling(myAccount.Agility_Stats,
                    dmg, myAccount); // check crit


            dmg = _dodge.DodgeHandling(myAccount.Agility_Stats, dmg,
                myAccount, enemyAccount); // check block


            if(dmg > 0)
            dmg = CheckForBlock(dmgType, dmg, myAccount); // check for block, if dmg <= 0 than dont waste block

            if (dmg > 0) // armor resist handling
                switch (dmgType)
            {
                case 0:
                    dmg = _armorReduction.ArmorHandling(myAccount.PhysicalPenetration, enemyAccount.PhysicalResistance, dmg);
                    break;
                case 1:
                    dmg = _magicReduction.ResistHandling(myAccount.MagicalPenetration, enemyAccount.MagicalResistance, dmg);   
                    break;
                //TODO implement mix dmg
            }

            if (dmg > 0)
                myAccount.Health += dmg * myAccount.LifeStealPrec; //lifesteal

            if (myAccount.Health > myAccount.MaxHealth) myAccount.Health = myAccount.MaxHealth; // hp cant be more than MAX hp

            var status = 0;
            var userId = myAccount.DiscordId;
            switch (dmgWhere)
            {
                case 0 when enemyAccount.Stamina > 0:

                    if (dmgType == 0)
                    {
                        dmg = dmg - myAccount.PhysShield;

                        if (dmg < 0)
                            dmg = 0;
                    }

                    enemyAccount.Stamina -= Math.Ceiling(dmg);
                    _accounts.SaveAccounts(userId);

                    if (enemyAccount.Stamina < 0)
                    {
                        enemyAccount.Health += enemyAccount.Stamina;
                        enemyAccount.Stamina = 0;
                        _accounts.SaveAccounts(userId);
                    }

                    break;
                case 0:

                    if (dmgType == 0)
                    {
                        dmg = dmg - myAccount.PhysShield;

                        if (dmg < 0)
                            dmg = 0;
                    }

                    if (enemyAccount.Stamina <= 0)
                    {
                        enemyAccount.Health -= Math.Ceiling(dmg);
                        _accounts.SaveAccounts(userId);
                    }

                    break;
                case 1:
                    enemyAccount.Health -= Math.Ceiling(dmg);
                    _accounts.SaveAccounts(userId);
                    break;
                case 2:
                    enemyAccount.Stamina -= Math.Ceiling(dmg);
                    if (enemyAccount.Stamina < 0)
                        enemyAccount.Stamina = 0;
                    _accounts.SaveAccounts(userId);
                    break;
            }

            if (enemyAccount.Health <= 0)
            {
                enemyAccount.Health = 0;
                status = 1;
            }

            _accounts.SaveAccounts(myAccount.DiscordId);
            _accounts.SaveAccounts(enemyAccount.DiscordId);

            await UpdateIfWinOrContinue(status, myAccount.DiscordId, myAccount.MessageIdInList);
        }

        public double CheckForBlock(int dmgType, double dmg, AccountSettings myAccount)
        {

            foreach (var shield in myAccount.BlockShield)
            {
                shield.howManyHits--;
                shield.howManyTurn--;
                _accounts.SaveAccounts(myAccount.DiscordId);

                if (shield.howManyHits <= -1 || shield.howManyTurn <= -1)
                    return dmg;
            }

            foreach (var shield in myAccount.BlockShield)
            {
                switch (dmgType)
                {
                    case 0 when shield.shieldType == 0:
                        return 0;
                    case 1 when shield.shieldType == 1:
                        return 0;

                }
            }

            return dmg;
        }

        public async Task CheckDmgWithTimer(AccountSettings account, AccountSettings enemy)
        {
            for (var index = 0; index < account.DamageOnTimer.Count; index++)
            {
                var t = account.DamageOnTimer[index];
                t.timer--;
                if (t.timer <= 0)
                {
                    await DmgHealthHandeling(0, t.dmg, t.dmgType, enemy, account);
                    account.DamageOnTimer.RemoveAt(index);
                    _accounts.SaveAccounts(account.DiscordId);
                }
            }

            await Task.CompletedTask;
        }

        public async Task CheckStatsForTime(AccountSettings account, AccountSettings enemy)
        {
            for (var i = 0; i < account.StatsForTime.Count; i++)
            {
                var t = account.StatsForTime[i];
                t.timer--;

                if (t.timer <= 0) account.AttackPower_Stats -= t.AD_STATS;
            }

            await Task.CompletedTask;
        }

        //move debufs somewhere else


        public async Task CheckForPassivesAndUpdateStats(AccountSettings account, AccountSettings enemy)
        {
            for (var i = 0; i < account.PassiveList.Count; i++)
            {
                if (account.SkillCooldowns.Any(x => x.skillId == account.PassiveList[i].skillId))
                    continue;

                _attackDamagePassiveTree.AttackDamagePassiveSkills(account.PassiveList[i].skillId, account, enemy);
                _defencePassiveTree.DefPassiveSkills(account.PassiveList[i].skillId, account, enemy);
                _agilityPassiveTree.AgiPassiveSkills(account.PassiveList[i].skillId, account, enemy);
                _magicPassiveTree.ApPassiveSkills(account.PassiveList[i].skillId, account, enemy);

            }

           // account.Base_AD_Stats = Math.Ceiling(account.Strength * (0.2 * account.OctoLvL)); // + ITEMS + SKILLS

        //    account.AttackPower_Stats = account.Base_AD_Stats + account.Bonus_AD_Stats;
       //     account.Bonus_AD_Stats = 0;

            account.IsCrit = false;

            _accounts.SaveAccounts(account.DiscordId);
            _accounts.SaveAccounts(enemy.DiscordId);
            await Task.CompletedTask;
        }

        public async Task UpdateIfWinOrContinue(int status, ulong userId, int i)
        {
            if (status == 1)
                foreach (var v in _global.OctopusGameMessIdList[i])
                {
                    if (v.PlayerDiscordAccount.Id == userId)
                        await _octoGameUpdateMess.VictoryPage(v.PlayerDiscordAccount.Id,
                            v.GamingWindowFromBot);
                }

        }

        public string[] GetSkillListFromTree(AccountSettings account)
        {
            string[] skills = { };

            switch (account.MoveListPage)
            {
                case 1:
                    skills = account.Attack_Tree.Split(new[] {'|'},
                        StringSplitOptions.RemoveEmptyEntries);
                    break;
                case 2:
                    skills = account.Defensive_Tree.Split(new[] {'|'},
                        StringSplitOptions.RemoveEmptyEntries);
                    break;
                case 3:
                    skills = account.Agility_Tree.Split(new[] {'|'},
                        StringSplitOptions.RemoveEmptyEntries);
                    break;
                case 4:
                    skills = account.Magic_Tree.Split(new[] {'|'},
                        StringSplitOptions.RemoveEmptyEntries);
                    break;
            }

            return skills;
        }

        public int GetSkillNum(SocketReaction reaction)
        {
            var value = 0;
            switch (reaction.Emote.Name)
            {
                case "1⃣":
                {
                    value = 1;
                    break;
                }

                case "2⃣":
                {
                    value = 2;
                    break;
                }

                case "3⃣":
                {
                    value = 3;
                    break;
                }

                case "4⃣":
                {
                    value = 4;
                    break;
                }

                case "5⃣":
                {
                    value = 5;
                    break;
                }

                case "6⃣":
                {
                    value = 6;
                    break;
                }

                case "7⃣":
                {
                    value = 6;
                    break;
                }

                case "8⃣":
                {
                    value = 7;
                    break;
                }
            }

            return value;
        }
    }
}