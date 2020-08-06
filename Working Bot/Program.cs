using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using Working_Bot.Commands;
using Working_Bot.Utils;

namespace Working_Bot
{
	class Program
	{

		public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; set; }

		static void Main(string[] args)
		{

			if(File.Exists("config.json"))
			{
				Console.WriteLine("Found Config File. Hoping its all good!");

				var prog = new Program();
				prog.RunBotAsync().GetAwaiter().GetResult();
			} else if (!File.Exists("config.json"))
			{
				Console.WriteLine("------------------------------\nNo Config File Found!!!!\nGenerating One Now\nFill it with the data before running the program again!\n---------------");
				MakeConfigFile.GenerateConfigFile();
				Console.ReadLine();
			}

			

		}

		async Task RunBotAsync()
		{

			var json = string.Empty;

			using (var fs = File.OpenRead("config.json"))
			using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
				json = await sr.ReadToEndAsync().ConfigureAwait(false);

			var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

		var cfg = new DiscordConfiguration
			{
				Token = configJson.Token,
				TokenType = TokenType.Bot,

				AutoReconnect = true,
				UseInternalLogHandler = true,

			};

			var cmdcfg = new CommandsNextConfiguration
			{
				StringPrefixes = new string[] { configJson.Prefix },
				EnableMentionPrefix = true,
				EnableDms = false,
			};

			Client = new DiscordClient(cfg);

			Client.Ready += Client_Ready;
			Client.GuildAvailable += Client_GuildAvailable;
			Client.ClientErrored += Client_ClientErrored;

			Commands = Client.UseCommandsNext(cmdcfg);
			Commands.CommandExecuted += Commands_CommandExecuted;
			Commands.CommandErrored += Commands_CommandErrored;



			Commands.RegisterCommands<UserCommands>();
			Commands.RegisterCommands<AdminCommands>();

			await Client.ConnectAsync();
			await Task.Delay(-1);
		}

		private async Task Commands_CommandErrored(CommandErrorEventArgs e)
		{
			e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "NexusNovaz", $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

			// let's check if the error is a result of lack
			// of required permissions
			if (e.Exception is ChecksFailedException ex)
			{
				// yes, the user lacks required permissions, 
				// let them know

				var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

				// let's wrap the response into an embed
				var embed = new DiscordEmbedBuilder
				{
					Title = "Access denied",
					Description = $"{emoji} You do not have the permissions required to execute this command.",
					Color = new DiscordColor(0xFF0000) // red
													   // there are also some pre-defined colors available
													   // as static members of the DiscordColor struct
				};
				await e.Context.RespondAsync("", embed: embed);
			}

		}

		private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
		{
			e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "NexusNovaz", $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);
			return Task.CompletedTask;
		}

		private Task Client_GuildAvailable(GuildCreateEventArgs e)
		{
			e.Client.DebugLogger.LogMessage(LogLevel.Info, "NexusNovaz", $"Guild available: {e.Guild.Name}", DateTime.Now);
			return Task.CompletedTask;
		}

		private Task Client_ClientErrored(ClientErrorEventArgs e)
		{
			e.Client.DebugLogger.LogMessage(LogLevel.Error, "NexusNovaz", $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);
			return Task.CompletedTask;
		}

		private Task Client_Ready(ReadyEventArgs e)
		{
			e.Client.DebugLogger.LogMessage(LogLevel.Info, "NexusNovaz", "Client is ready to process events.", DateTime.Now);
			var DBC = e.Client;
			return Task.CompletedTask;
		}

		
	}
}
