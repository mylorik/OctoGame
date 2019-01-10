﻿using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace OctoGame.DiscordFramework.Extensions
{
   public class ModuleBaseCustom : ModuleBase<SocketCommandContextCustom>
   {

       protected virtual async Task SendMessAsync(EmbedBuilder embed)
       {
           if (Context.MessageContentForEdit == null)
           {
               var message = await Context.Channel.SendMessageAsync("", false, embed.Build());
               var kek = new Global.CommandRam(Context.User, Context.Message, message);
               Context.Global.CommandList.Add(kek);
           }
           else if (Context.MessageContentForEdit == "edit")
           {
               foreach (var t in Context.Global.CommandList)
                   if (t.UserSocketMsg.Id == Context.Message.Id)
                       await t.BotSocketMsg.ModifyAsync(message =>
                       {
                           message.Content = "";
                           message.Embed = null;
                           message.Embed = embed.Build();
                       });
           }
       }


       protected virtual async Task SendMessAsync([Remainder] string regularMess = null)
       {
           if (Context.MessageContentForEdit == null)
           {
               var message = await Context.Channel.SendMessageAsync($"{regularMess}");
               var kek = new Global.CommandRam(Context.User, Context.Message, message);

               Context.Global.CommandList.Add(kek);
           }
           else if (Context.MessageContentForEdit == "edit")
           {
               foreach (var t in Context.Global.CommandList)
                   if (t.UserSocketMsg.Id == Context.Message.Id)
                       await t.BotSocketMsg.ModifyAsync(message =>
                       {
                           message.Content = "";
                           message.Embed = null;
                           if (regularMess != null) message.Content = regularMess.ToString();
                       });
           }
       }


       protected virtual async Task SendMessAsync([Remainder] string regularMess, SocketCommandContextCustom context)
       {
           if (context.MessageContentForEdit == null )
           {
               var message = await context.Channel.SendMessageAsync($"{regularMess}");
               var kek = new Global.CommandRam(context.User, context.Message, message);

               context.Global.CommandList.Add(kek);
           }
           else if (context.MessageContentForEdit == "edit")
           {
               foreach (var t in context.Global.CommandList)
                   if (t.UserSocketMsg.Id == context.Message.Id)
                       await t.BotSocketMsg.ModifyAsync(message =>
                       {
                           message.Content = "";
                           message.Embed = null;
                           if (regularMess != null) message.Content = regularMess.ToString();
                       });
           }
       }

    }
}