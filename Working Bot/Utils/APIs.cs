using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Working_Bot.Utils
{
	public partial class APIs
	{
		private static IReadOnlyDictionary<string, string> APIDictionary { get; }
		internal static IReadOnlyList<string> APISortedList { get; }

		static APIs()
		{
			APIDictionary = new Dictionary<string, string>()
			{
				{"NasaAPI", "https://api.nasa.gov/planetary/apod"},
				{"JokeAPI", "https://sv443.net/jokeapi/v2/joke/Any"},
				{"NumberAPI", "http://numbersapi.com/"},
				{"InsultAPI", "https://evilinsult.com/generate_insult.php?lang=en&type=json"},
				{"DadJokeAPI", "https://icanhazdadjoke.com/"}
			};

			var APIArray = APIDictionary.Values.Distinct().ToArray();
			Array.Sort(APIArray);
			APISortedList = APIArray;

		}

	}
}
