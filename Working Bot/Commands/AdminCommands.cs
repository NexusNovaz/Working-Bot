using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Working_Bot.Commands
{
	public class AdminCommands : BaseCommandModule
	{
		[Command("purge"), Aliases("p"), Description("Delete an amount of messages from the current channel."), RequirePermissions(Permissions.Administrator)]
		public async Task Purge(CommandContext ctx, [Description("Amount of messages to remove (max 100)")] int limit = 50, [Description("Amount of messages to skip")] int skip = 0)
		{
			var i = 0;
			var ms = await ctx.Channel.GetMessagesBeforeAsync(ctx.Message.Id, limit);
			var deletThis = new List<DiscordMessage>();
			foreach (var m in ms)
			{
				if (i < skip)
					i++;
				else
					deletThis.Add(m);
			}
			if (deletThis.Any())
				await ctx.Channel.DeleteMessagesAsync(deletThis, "Purged messages.");
			var resp = await ctx.RespondAsync("Latest messages deleted.");
			await Task.Delay(2000);
			await resp.DeleteAsync("Purge command executed.");
			await ctx.Message.DeleteAsync("Purge command executed.");

			var last = await ctx.RespondAsync($"Purged messages.\nChannel: #{ctx.Channel.Name} ({ctx.Channel.Id})");
			await Task.Delay(5000);
			await last.DeleteAsync($"Purged messages.\nChannel: #{ctx.Channel.Name} ({ctx.Channel.Id})");
		}

		[Command("kick"), Aliases("k"), Description("Kick a user from the discord server"), RequirePermissions(Permissions.KickMembers)]
		public async Task Kick(CommandContext ctx, DiscordMember user = null, string reason = null)
		{
			if(!user.IsBot && user.Id != 153284216228937728 && !user.IsOwner)
			{
				if (reason == null)
				{
					await user.RemoveAsync($"{ctx.Member.Username} kicked {user.Username} without providing a reason").ConfigureAwait(false);
				}
				else if (reason != null)
				{
					await user.RemoveAsync($"{ctx.Member.Username} kicked {user.Username} with the reason: {reason}").ConfigureAwait(false);
				}
			} else if(user.IsBot || user.Id == 153284216228937728 || user.IsOwner)
			{
				await ctx.Message.RespondAsync("Can not kick " + user.Mention);
			}
		}
	}
}
