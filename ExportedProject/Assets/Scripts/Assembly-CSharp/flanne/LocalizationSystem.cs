using System.Collections.Generic;

namespace flanne
{
	public class LocalizationSystem
	{
		public enum Language
		{
			English = 0,
			Japanese = 1,
			Chinese = 2,
			BrazilPortuguese = 3,
			TChinese = 4,
			Russian = 5,
			Spanish = 6,
			German = 7,
			Polish = 8,
			Italian = 9,
			Turkish = 10,
			French = 11,
			Korean = 12,
			Hungarian = 13
		}

		public static Language language;

		private static Dictionary<string, string> localizedEN;

		private static Dictionary<string, string> localizedJP;

		private static Dictionary<string, string> localizedCH;

		private static Dictionary<string, string> localizedBR;

		private static Dictionary<string, string> localizedTC;

		private static Dictionary<string, string> localizedRU;

		private static Dictionary<string, string> localizedSP;

		private static Dictionary<string, string> localizedGR;

		private static Dictionary<string, string> localizedPL;

		private static Dictionary<string, string> localizedIT;

		private static Dictionary<string, string> localizedTR;

		private static Dictionary<string, string> localizedFR;

		private static Dictionary<string, string> localizedKR;

		private static Dictionary<string, string> localizedHU;

		public static bool isInit;

		public static CSVLoader csvLoader;

		public static void Init()
		{
			csvLoader = new CSVLoader();
			csvLoader.LoadCSV();
			UpdateDictionaries();
			isInit = true;
		}

		public static void UpdateDictionaries()
		{
			localizedEN = csvLoader.GetDictionaryValues("en");
			localizedJP = csvLoader.GetDictionaryValues("jp");
			localizedCH = csvLoader.GetDictionaryValues("ch");
			localizedBR = csvLoader.GetDictionaryValues("br");
			localizedTC = csvLoader.GetDictionaryValues("tc");
			localizedRU = csvLoader.GetDictionaryValues("ru");
			localizedSP = csvLoader.GetDictionaryValues("sp");
			localizedGR = csvLoader.GetDictionaryValues("gr");
			localizedPL = csvLoader.GetDictionaryValues("pl");
			localizedIT = csvLoader.GetDictionaryValues("it");
			localizedTR = csvLoader.GetDictionaryValues("tr");
			localizedFR = csvLoader.GetDictionaryValues("fr");
			localizedKR = csvLoader.GetDictionaryValues("kr");
			localizedHU = csvLoader.GetDictionaryValues("hu");
		}

		public static Dictionary<string, string> GetDictionaryForEditor()
		{
			if (!isInit)
			{
				Init();
			}
			return localizedEN;
		}

		public static string GetLocalizedValue(string key)
		{
			if (!isInit)
			{
				Init();
			}
			string value = key;
			switch (language)
			{
			case Language.English:
				localizedEN.TryGetValue(key, out value);
				break;
			case Language.Japanese:
				localizedJP.TryGetValue(key, out value);
				break;
			case Language.Chinese:
				localizedCH.TryGetValue(key, out value);
				break;
			case Language.BrazilPortuguese:
				localizedBR.TryGetValue(key, out value);
				break;
			case Language.TChinese:
				localizedTC.TryGetValue(key, out value);
				break;
			case Language.Russian:
				localizedRU.TryGetValue(key, out value);
				break;
			case Language.Spanish:
				localizedSP.TryGetValue(key, out value);
				break;
			case Language.German:
				localizedGR.TryGetValue(key, out value);
				break;
			case Language.Polish:
				localizedPL.TryGetValue(key, out value);
				break;
			case Language.Italian:
				localizedIT.TryGetValue(key, out value);
				break;
			case Language.Turkish:
				localizedTR.TryGetValue(key, out value);
				break;
			case Language.French:
				localizedFR.TryGetValue(key, out value);
				break;
			case Language.Korean:
				localizedKR.TryGetValue(key, out value);
				break;
			case Language.Hungarian:
				localizedHU.TryGetValue(key, out value);
				break;
			}
			return value;
		}
	}
}
