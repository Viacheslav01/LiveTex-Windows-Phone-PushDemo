using System.IO.IsolatedStorage;

namespace Livetex.Push
{
	internal static class Storage
	{
		private const string cLiveTexApplicationKey = "LT_APP";
		private const string cLiveTexKeyKey = "LT_KEY";
		private const string cLiveTexServerKey = "LT_SERVER";

		public static string LiveTexApplication
		{
			get { return GetValue<string>(cLiveTexApplicationKey); }
			set { SetValue(cLiveTexApplicationKey, value); }
		}

		public static string LiveTexKey
		{
			get { return GetValue<string>(cLiveTexKeyKey); }
			set { SetValue(cLiveTexKeyKey, value); }
		}

		public static string LiveTexServer
		{
			get { return GetValue<string>(cLiveTexServerKey); }
			set { SetValue(cLiveTexServerKey, value); }
		}

		private static T GetValue<T>(string key)
		{
			var settings = IsolatedStorageSettings.ApplicationSettings;

			T value;
			settings.TryGetValue(key, out value);

			return value;
		}

		private static void SetValue<T>(string key, T value)
		{
			var settings = IsolatedStorageSettings.ApplicationSettings;

			if(settings.Contains(key))
			{
				settings[key] = value;
			}
			else
			{
				settings.Add(key, value);
			}

			settings.Save();
		}
	}
}
