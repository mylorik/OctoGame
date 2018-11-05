﻿namespace OctoGame.Accounts.GameSpells
{
    public interface ISpellAccounts
    {
        void SaveAccounts();
        SpellSetting GetAccount(ulong spellId);
        SpellSetting GetOrCreateAccount(ulong spellId);
        SpellSetting CreateUserAccount(ulong id);
    }
}
