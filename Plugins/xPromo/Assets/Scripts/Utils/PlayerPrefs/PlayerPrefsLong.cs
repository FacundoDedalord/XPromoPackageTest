
using UnityEngine;

namespace PlayerPrefsUtils {
    public class PlayerPrefsLong {
        public long Value {
            get {
                string storedValue = _storageContainer.Value;
                long parsedValue;
                if(long.TryParse(storedValue, out parsedValue))
                {
                    return parsedValue;
                }
                return 0;
            }
            set {
                _storageContainer.Value = value.ToString();
            }
        }
        private PlayerPrefsString _storageContainer;

        public PlayerPrefsLong(string key, long defaultValue, bool saveAfterEveryWrite = false) {
            _storageContainer = new PlayerPrefsString(key, defaultValue.ToString(), saveAfterEveryWrite);
        }
    }
}