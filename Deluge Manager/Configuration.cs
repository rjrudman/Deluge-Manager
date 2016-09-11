using System;
using System.IO;
using Newtonsoft.Json;

namespace Deluge_Manager
{
	public class ConfigurationManager
	{
		private static ConfigurationSettings _settings;

		public static ConfigurationSettings Settings => _settings ?? (_settings = LoadSettings());

		private static ConfigurationSettings LoadSettings()
		{
			return JsonConvert.DeserializeObject<ConfigurationSettings>(
				File.ReadAllText("config.json")
			);
		}
	}

	public class ConfigurationSettings
	{
		public Uri LocalDelugeUri { get; set; }
		public string LocalDelugePassword { get; set; }

		public Uri RemoteDelugeUri { get; set; }
		public string RemoteDelugePassword { get; set; }

		public string PrivateTracker { get; set; }
	}
}
