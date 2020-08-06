using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Working_Bot.Commands 
{

	public class Request
	{
		public string hdurl { get; set; }
	}

	public class UserCommands : BaseCommandModule
	{
		[Command("Ping"), Description("Returns the ping of the bot/user")]
		public async Task Ping(CommandContext ctx)
		{
			await ctx.TriggerTypingAsync().ConfigureAwait(false);
			var emoji = DiscordEmoji.FromName(ctx.Client, ":ping_pong:");
			await ctx.RespondAsync($"{emoji} Pong! Ping: {ctx.Client.Ping}ms").ConfigureAwait(false);
		}

		[Command("APOD"), Description("Gets the Astronomy Picture Of the Day from NASA")]
		public async Task APOD(CommandContext ctx, [Description("Please provide the year as 4 digits")] int? givenYear = null, [Description("Please provide the month as either 2 digits or 1 digit")] int? givenMonth = null, [Description("Please provide the day as either 2 digits or 1 digit")] int? givenDay = null)
		{

			var json = string.Empty;

			using (var fs = File.OpenRead("config.json"))
			using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
				json = await sr.ReadToEndAsync().ConfigureAwait(false);

			var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

			DateTime myDate = default;
			DateTime givenDate = default;
			string apiURL = "https://api.nasa.gov/planetary/apod", apiKey = "?api_key=" + configJson.NASAApiKey, apiURLExt = null;

			if (givenYear == null && givenMonth == null && givenDay == null)
			{
				myDate = DateTime.Today;

				apiURL = apiURL + apiKey;

			} else if (givenYear != null && givenMonth != null && givenDay != null)
			{
				givenDate = new DateTime((int)givenYear, (int)givenMonth, (int)givenDay);

				apiURLExt = "&date=" + givenDate.Year.ToString() + "-" + givenDate.Month.ToString() + "-" + givenDate.Day.ToString();

				apiURL = apiURL + apiKey + apiURLExt;

			} else
			{
				Debug.Write("Something Fucked Up :/");
			}
			

			var footer = "The website updates every day, check back tomorrow for a new one!\nRequest Remaining for the hour: ";

			Random rnd = new Random();

			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri(apiURL);
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(
				new MediaTypeWithQualityHeaderValue("application/json"));

			try
			{
				HttpResponseMessage response = await client.GetAsync(apiURL).ConfigureAwait(false);

				if (response.IsSuccessStatusCode)
				{
					var bodyResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
					var headerResponse = response.Headers.GetValues("X-RateLimit-Remaining");
					dynamic data = JObject.Parse(bodyResponse);
					dynamic headerData = headerResponse.ToList();

					if (data.media_type == "video")
					{
						await ctx.Message.RespondAsync(embed: new DiscordEmbedBuilder
						{
							Title = data.title,
							Description = data.explanation + "\n\nVideo URL: " + data.url,
							
							Color = new DiscordColor(Convert.ToByte(rnd.Next(0, 255)), Convert.ToByte(rnd.Next(0, 255)), Convert.ToByte(rnd.Next(0, 255)))
						}.WithFooter(footer + headerData[0]).Build()).ConfigureAwait(false);


					} else if (data.media_type == "image")
					{
						await ctx.Message.RespondAsync(embed: new DiscordEmbedBuilder
						{
							Title = data.title,
							ImageUrl = data.hdurl,
							Description = data.explanation,
							Color = new DiscordColor(Convert.ToByte(rnd.Next(0, 255)), Convert.ToByte(rnd.Next(0, 255)), Convert.ToByte(rnd.Next(0, 255)))
						}.WithFooter(footer + headerData[0]).Build()).ConfigureAwait(false);
					} else {
						Console.WriteLine("Something Went Wrong around the distinguishing of media area in APOD. Get help now!");
					}

					
				} else
				{
					Console.WriteLine("Something fucked up");
				}

			} catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}


		}

		[Command("Joke"), Description("Will tell you a joke")]
		public async Task Joke(CommandContext ctx)
		{
			var apiURL = "https://sv443.net/jokeapi/v2/joke/Any";

			Random rnd = new Random();

			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri(apiURL);
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(
				new MediaTypeWithQualityHeaderValue("application/json"));

			try
			{
				HttpResponseMessage response = await client.GetAsync(apiURL).ConfigureAwait(false);
				var bodyResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
				dynamic data = JObject.Parse(bodyResponse);

				if(data.error == "false" && data.type == "single")
				{
					var embed = new DiscordEmbedBuilder
					{
						Title = "Here is a " + data.category + " joke:",
						Description = "🎤\n" + data.joke,
						Color = new DiscordColor(Convert.ToByte(rnd.Next(0, 255)), Convert.ToByte(rnd.Next(0, 255)), Convert.ToByte(rnd.Next(0, 255)))
					};

					

					await ctx.Message.RespondAsync(embed: embed).ConfigureAwait(false);

				} else if (data.error == "false" && data.type == "twopart")
				{
					DiscordColor color = new DiscordColor(Convert.ToByte(rnd.Next(0, 255)), Convert.ToByte(rnd.Next(0, 255)), Convert.ToByte(rnd.Next(0, 255)));
					var embed = new DiscordEmbedBuilder
					{
						Title = "Here is a " + data.category + " joke:",
						Description = "🎤\n" + data.setup,
						Color = color
					};

				
					DiscordMessage x = await ctx.Message.RespondAsync(embed: embed).ConfigureAwait(false);

					await Task.Delay(3000).ConfigureAwait(false);

					var editedEmbed = new DiscordEmbedBuilder
					{
						Title = "Here is a " + data.category + " joke:",
						Description = "🎤\n" + data.setup + "\n\n" + data.delivery,
						Color = color
					};

					await x.ModifyAsync(embed: editedEmbed.Build());

					
				}


			} catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

		}

		[Command("Num"), Description("Will give you a random bit of information based on a random or given number"), Aliases("numfact")]
		public async Task Num(CommandContext ctx, [Description("Optional number to give for a fact")] string givenNumber = "random")
		{
			Random rnd = new Random();
			var apiURL = "http://numbersapi.com/";
			var apiNumber = String.Empty;

			if (givenNumber == "random")
			{
				apiNumber = "random";
			} else if (givenNumber != "random")
			{
				try
				{
					int numberGiven = int.Parse(givenNumber);

					if (numberGiven >= 0)
					{
						apiNumber = numberGiven.ToString();
					} else if (numberGiven < 0)
					{
						await ctx.Message.RespondAsync("Please use the command with a number greater than or equal to 0").ConfigureAwait(false);
					}
					
				} catch (Exception e)
				{
					Console.WriteLine(e.Message);
					await ctx.Message.RespondAsync($"Error! Something went wrong\nError Message: {e.Message.ToString()}").ConfigureAwait(false);
				}
			}

			HttpClient client = new HttpClient
			{
				BaseAddress = new Uri(apiURL)
			};
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(
				new MediaTypeWithQualityHeaderValue("application/json"));

			try
			{
				HttpResponseMessage response = await client.GetAsync(apiURL + givenNumber + "?json").ConfigureAwait(false);

				if (response.IsSuccessStatusCode)
				{
					var bodyResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
					dynamic data = JObject.Parse(bodyResponse);

					if(data.found == "true")
					{
						await ctx.Message.RespondAsync(embed: new DiscordEmbedBuilder
						{
							Title = "Number Facts",
							Description = $"Here is something i found for {data.number}\n\n{data.text}",
							Color = new DiscordColor(Convert.ToByte(rnd.Next(0, 255)), Convert.ToByte(rnd.Next(0, 255)), Convert.ToByte(rnd.Next(0, 255)))
						}).ConfigureAwait(false);
					} else if (data.found == "false")
					{
						await ctx.Message.RespondAsync(embed: new DiscordEmbedBuilder
						{
							Title = "Number Facts",
							Description = $"Unfortunately, {data.number} is a boring number and returned nothing. Choose a different number",
							Color = DiscordColor.Red
						}).ConfigureAwait(false);
					}

				}
			} catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		[Command("Insult"), Description("Will insult the user given")]
		public async Task Insult(CommandContext ctx, DiscordMember user = null)
		{
			var apiURL = "https://evilinsult.com/generate_insult.php?lang=en&type=json";
			var okToGo = false;

			if (user.IsBot)
			{
				await ctx.Message.RespondAsync("Please @ a user that isn't a bot!").ConfigureAwait(false);
				await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("\U0001f620")).ConfigureAwait(false);
			} else if (user == ctx.Member)
			{
				await ctx.Message.RespondAsync("Please @ a user that isn't yourself!").ConfigureAwait(false);
				await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("\U0001f620")).ConfigureAwait(false);
			} else if (user == null)
			{
				await ctx.Message.RespondAsync("Please @ a user to insult").ConfigureAwait(false);
				await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("\U0001f62d")).ConfigureAwait(false);
			} else if (!user.IsBot && !user.IsCurrent)
			{
				okToGo = true;
			}

			HttpClient client = new HttpClient
			{
				BaseAddress = new Uri(apiURL)
			};
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(
				new MediaTypeWithQualityHeaderValue("application/json"));

			try
			{
				HttpResponseMessage response = await client.GetAsync(apiURL).ConfigureAwait(false);

				if (response.IsSuccessStatusCode && okToGo)
				{
					var bodyResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
					dynamic data = JObject.Parse(bodyResponse);

					await ctx.Message.RespondAsync(user.Mention + " " + data.insult).ConfigureAwait(false);
				} else if (!okToGo) {
					return;
				} else
				{
					Console.WriteLine("Something in insult command didn't work as intended");
				}

			} catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}


		}

		[Command("Dadjoke"), Description("Will tell a random dad joke")]
		public async Task DadJoke(CommandContext ctx)
		{
			var apiURL = "https://icanhazdadjoke.com/";
			Random rnd = new Random();

			HttpClient client = new HttpClient
			{
				BaseAddress = new Uri(apiURL)
			};
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(
				new MediaTypeWithQualityHeaderValue("text/plain"));

			try
			{
				HttpResponseMessage response = await client.GetAsync(apiURL).ConfigureAwait(false);

				if (response.IsSuccessStatusCode)
				{
					var bodyResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

					await ctx.Message.RespondAsync(embed: new DiscordEmbedBuilder
					{
						Title = "Dad Joke",
						Description = bodyResponse,
						Color = new DiscordColor(Convert.ToByte(rnd.Next(0, 255)), Convert.ToByte(rnd.Next(0, 255)), Convert.ToByte(rnd.Next(0, 255)))
					}.WithFooter("https://icanhazdadjoke.com/").Build()).ConfigureAwait(false);
				} else
				{
					Console.WriteLine("Something in dadjoke command didn't work as intended");
				}

			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}
	}
}
