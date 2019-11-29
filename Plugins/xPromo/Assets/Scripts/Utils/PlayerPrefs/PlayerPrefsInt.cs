
using UnityEngine;

namespace PlayerPrefsUtils {
    public class PlayerPrefsInt {
		public int Value {
			get {
				return PlayerPrefs.GetInt(_key);
			}
			set {
				PlayerPrefs.SetInt(_key, value);
			}			
		}
		private string _key;
		public PlayerPrefsInt(string key, int defaultValue) {
			_key = key;
			if(!PlayerPrefs.HasKey(key))
			{
				Value = defaultValue;
			}
		}
	}
}