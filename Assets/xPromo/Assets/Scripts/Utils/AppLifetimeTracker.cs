using System;
using PlayerPrefsUtils;

public class AppLifetimeTracker {		
    private PlayerPrefsDate _appFirstInitializationDate;
    public AppLifetimeTracker() {
        _appFirstInitializationDate  = new PlayerPrefsDate("_appFirstInitializationDate", DateTime.UtcNow, true);
    }
    public TimeSpan TimeSinceFirstInit {
        get {
            DateTime now = DateTime.UtcNow;
            DateTime then = _appFirstInitializationDate.Value;
            TimeSpan timePassed = now - then;
            return timePassed;
        }
    }
    public int DaysSinceFirstInit {
        get {
            return (int)TimeSinceFirstInit.Days;
        }
    }
}