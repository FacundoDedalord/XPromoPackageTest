
using UnityEngine;

namespace PlayerPrefsUtils {
    public class PlayerPrefsString {		
		public string Value {
			get {
				return PlayerPrefs.GetString(_key);
			}
			set {
				PlayerPrefs.SetString(_key, value);

				if(_saveAfterEveryWrite) {
					PlayerPrefs.Save();
				}
			}			
		}
		private string _key;
		private bool _saveAfterEveryWrite;
		public PlayerPrefsString(string key, string defaultValue, bool saveAfterEveryWrite = false) {
			_key = key;
			_saveAfterEveryWrite = saveAfterEveryWrite;
			
			if(!PlayerPrefs.HasKey(key)) {
				Value = defaultValue;
			}
			
		}
	}
}