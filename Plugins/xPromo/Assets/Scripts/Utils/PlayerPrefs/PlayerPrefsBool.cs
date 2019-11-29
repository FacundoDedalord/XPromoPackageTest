using UnityEngine;

namespace PlayerPrefsUtils {
    public class PlayerPrefsBool {
        public bool Value {
            get {
                return PlayerPrefs.GetInt(_key) == 1;
            }
            set {
                int boolToInt = value ? 1 : 0;
                PlayerPrefs.SetInt(_key, boolToInt);
            }
        }
        private string _key;
        public PlayerPrefsBool(string key, bool defaultValue) {
            _key = key;            
            if(!PlayerPrefs.HasKey(_key)) {
                Value = defaultValue;
            }
        }
    }
}