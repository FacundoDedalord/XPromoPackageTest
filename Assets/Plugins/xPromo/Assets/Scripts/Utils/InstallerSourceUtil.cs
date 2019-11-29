using UnityEngine;

public static class InstallerSourceUtil
{
    /// <summary>
    /// Returns the store from where the app was downloaded.
    /// If we are in editor, will be solved by platform.
    /// In case of being in editor and Android as plataform, will returns always Samsung store
    /// </summary>
    public static InstallerSource GetInstallSource() {
        if (Application.platform == RuntimePlatform.IPhonePlayer) {
            return InstallerSource.eAppleAppStore;
        } else if (Application.platform == RuntimePlatform.Android) {
            // Tries to define the store it has been downloaded from the safest way possible.
            // These are the expected names but they are not guarantied:
            // SAMSUNG GALAXY STORE: com.sec.android.app.samsungapps
            // SKILLZ: com.google.android.packageinstaller (Installed manually after downloading the .apk)

            if (Application.installerName != null && Application.installerName.Contains("samsung")) {
                return InstallerSource.eSamsungGalaxyStore;
            } else {
                return InstallerSource.eSkillzStore;
            }
        } else {
#if UNITY_IOS 
            return InstallerSource.eAppleAppStore;
#elif UNITY_ANDROID
            return InstallerSource.eSamsungGalaxyStore;
#else
            return InstallerSource.eUnknown;
#endif
        }
    }
}