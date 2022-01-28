using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class ArgumentParser
	{
		private Dictionary<string, List<string>> options = new Dictionary<string, List<string>>();

		public ArgumentParser(string[] args)
		{
			// go over args
			for(int i = 0; i < args.Length;)
			{
				string key = "";
				// long flag or option (--something)
				if(args[i].StartsWith("--"))
				{
					if (args[i].Length > 2)	// not empty
						key = args[i].Substring(2);
					i++;
				}
				// short flags or options (-a or -abc)
				else if (args[i].StartsWith('-'))
				{
					if (args[i].Length == 2)    // single option, can have parameters
						key = args[i].Substring(1, 1);
					else
					{
						// flags, no parameters
						foreach (char flag in args[i].Substring(1))
						{
							AddOption(flag.ToString());
						}
						i++;
						continue;
					}
					i++;
				}
				// add new option
				AddOption(key);
				// add option's options
				while (i < args.Length && !args[i].StartsWith('-'))
				{
					AddOption(key, args[i]);
					i++;
				}
			}
		}

		private void AddOption(string key, string value = null)
		{
			// add key if it doesn't exist
			if(!options.ContainsKey(key))
			{
				options.Add(key, new List<string>());
			}
			// add value if it's specified
			if(value != null)
			{
				options[key].Add(value);
			}
		}

		public override string ToString()
		{
			string result = "";
			foreach(string key in options.Keys)
			{
				// add key and it's values
				result += "Option \"" + key + "\":\n";
				foreach(string value in options[key])
				{
					result += "\t\"" + value + "\"\n";
				}
			}
			return result;
		}
	}
}
