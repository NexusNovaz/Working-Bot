using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Working_Bot.Utils
{
	class MakeConfigFile
	{
		
		public static void GenerateConfigFile()
		{

			StreamWriter file = new StreamWriter("config.json");
			file.WriteAsync($"{{\n{Indent(4)}\"token\": \"\", // Place your token in those quotation marks\n{Indent(4)}\"prefix\": \"!\", // Replace the ! with the prefix you want\n{Indent(4)}\"NASAApiKey\": \"\", // Register an API key with NASA if you want to use the !apod command!\n}}");
			file.Close();
		}
		public static string Indent(int count)
		{
			return "".PadLeft(count);
		}


	}
}
