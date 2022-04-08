using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class ArgumentParser
	{
		// string option -> list of parameters
		private Dictionary<string, List<string>> options = new Dictionary<string, List<string>>();

		// constructortor parses arguments into options dictionary
		public ArgumentParser(string[] args)
		{
			// go over args
			for(int i = 0; i < args.Length;)
			{
				string key = "";
				// long flag or option (--something)
				if (args[i].StartsWith("--") && args[i].Length > 2)
				{
					key = args[i].Substring(2);
					i++;
				}
				// short flags or options (-a or -abc)
				else if (args[i].StartsWith('-') && args[i].Length > 1)
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
				while (i < args.Length && !(args[i].StartsWith('-') && args[i].Length > 1))
				{
					AddOption(key, args[i]);
					i++;
				}
			}
		}

		// Method joins similair options together into the first option.
		// to: option to copy into
		// from: option to copy and delete
		// return: none
		public void JoinOptions(string to, string from)
		{
			if (!HasOption(from))
				return;

			List<string> parametersToAdd = options[from];
			options.Remove(from);
			// merge lists
			AddOption(to);	// add if doesn't exist
			options[to].AddRange(parametersToAdd);
		}

		// Method adds an option to the dictionary or adds a value to an existing option
		// key: option key to add at
		// value: value to add, default none (null)
		// return: none
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

		// Method checks if an option is included.
		// key: option to check
		// return: whether or not the option is included.
		public bool HasOption(string key)
		{
			return options.ContainsKey(key);
		}

		// Method returns list of parameters in some option.
		// option: option to get it's parameters.
		// return: list of parameters
		public List<string> GetParameters(string option)
		{
			if (HasOption(option))
				return options[option];
			else 
				return null;
		}

		// ToString override prints the parsed options dictionary.
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
