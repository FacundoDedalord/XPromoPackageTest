
using System;
using UnityEngine;

namespace PlayerPrefsUtils {
    public class PlayerPrefsDate {
        public DateTime Value {
            get {
                long storedValue = _storageContainer.Value;
                return TimeUtils.UnixTimeToDate(storedValue);
            }
            set {
                _storageContainer.Value = TimeUtils.DateToUnixTime(value);
            }
        }
        private PlayerPrefsLong _storageContainer;

        public PlayerPrefsDate(string key, DateTime defaultValue, bool saveAfterEveryWrite = false)
        {
            _storageContainer = new PlayerPrefsLong(key, TimeUtils.DateToUnixTime(defaultValue), saveAfterEveryWrite);
        }
    }
}