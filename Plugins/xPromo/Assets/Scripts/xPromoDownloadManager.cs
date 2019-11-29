// .NET Framework
using System;
using System.Collections.Generic;
using System.IO;

// Unity Framework
using UnityEngine;
using UnityEngine.Networking;

public class xPromoDownloadManager : MonoBehaviorSingleton<xPromoDownloadManager>
{
    public enum State
    {
        Idle,
        HeaderDownloading,
        Downloading,
        End
    };
    
    private const string xPromoStorageKey = "xPromoStorage";

    // Base Url where all the assets are hosted
    public string baseUrl = "http://www.dedalord.com/tmp";

    // Reference to the UnityWebRequest
    private UnityWebRequest www;

    // State
    private State state;

    // List of assets that should be downloaded
    private List<string> assetsToDownload;

    // List of all assets that should be download its header
    private List<string> assetsToDownloadHeader;

    // List of all assets lengths
    private List<long> assetsToDownloadHeaderLen;

    // Index of the current asset in downloading
    private int currentAssetInDownload;

    // Array of the assets in the storage    
    private List<string> assetsInStorage;

    [HideInInspector]
    // Download progress of the current downloaded asset
    public float downloadProgress = 0f;

    [HideInInspector]
    // Log on UI
    public bool logOnUI = true;

    // Reference to the logger
    private xPromoLogger logger;
    
    /// <summary>
    /// Unity Awake Method
    /// </summary>
    new void Awake() {

        base.Awake();
        
        logger = GetComponent<xPromoLogger>();

        Log($"- baseUrl: {baseUrl}");
        
        assetsInStorage = ReadNamesOfAssetsInStorage();

        if (assetsInStorage != null)
        {
            string filesOnLocal = "";
            for (int i = 0; i < assetsInStorage.Count; i++)
            {
                filesOnLocal += assetsInStorage[i];
                if (i < assetsInStorage.Count - 1) filesOnLocal += ", ";
            }

            Log($"Files on local: {filesOnLocal}");
        }
        else
        {
            Log($"Files on local: None");
        }

        if (logOnUI) logger.LogLocalStorageItems(assetsInStorage);
        
        state = State.Idle;
    }

    /// <summary>
    /// Unity Update Method
    /// </summary>
    void Update() {
        
        if (logOnUI) {
            xPromoLogger.Instance.LogDownloadManagerStatus(state);
        }
        
        switch (state) {
            case State.Downloading:
                
                downloadProgress = www.downloadProgress;

                if (www.isDone) {
                    if (!www.isHttpError) {
                        File.WriteAllBytes($"{Application.persistentDataPath}/{assetsToDownload[currentAssetInDownload]}", www.downloadHandler.data);
                        Log($"Asset {assetsToDownload[currentAssetInDownload]} downloaded ok");

                        if (assetsInStorage == null) assetsInStorage = new List<string>();
                        if (!assetsInStorage.Contains(assetsToDownload[currentAssetInDownload])) assetsInStorage.Add(assetsToDownload[currentAssetInDownload]);
                        WriteNamesOfAssetsInStorage(assetsInStorage);
                        
                        if (logOnUI) logger.LogLocalStorageItems(assetsInStorage);
                        
                    } else {
                        Log($"Cannot download asset {assetsToDownload[currentAssetInDownload]} Response code: {www.responseCode}");
                    }

                    currentAssetInDownload++;
                    if (currentAssetInDownload < assetsToDownload.Count) {
                        downloadProgress = 0f;
                        Load(assetsToDownload[currentAssetInDownload]);
                    } else {
                        Log("All assets were downloaded");

                        string filesOnLocal = "";
                        for (int i = 0; i < assetsInStorage.Count; i++) {
                            filesOnLocal += assetsInStorage[i];
                            if (i < assetsInStorage.Count - 1) filesOnLocal += ", ";
                        }

                        Log($"Files on local: {filesOnLocal}");
                        Log($"Finished Ok");
                        state = State.End;
                    }

                }

                break;

            case State.HeaderDownloading:
                
                downloadProgress = www.downloadProgress;
                
                if (www.isDone) {
                    
                    if (!www.isHttpError) {
                        var fileLen = www.GetResponseHeader("Content-Length");
                        //Debug.Log($"[xPromo] Header asset {assetsToDownloadHeader[currentAssetInDownload]} fileLen: {fileLen}");

                        //var fileMD5 = www.GetResponseHeader("Content-MD5");
                        //Debug.Log($"[xPromo] Header asset fileMD5: {fileMD5}");

                        assetsToDownloadHeaderLen.Add(long.Parse(fileLen));

                        currentAssetInDownload++;
                        if (currentAssetInDownload < assetsToDownloadHeader.Count) {
                            LoadHeader(assetsToDownloadHeader[currentAssetInDownload]);
                        } else {
                            Log("All headers assets were downloaded");

                            for (int i = 0; i < assetsToDownloadHeader.Count; i++) {
                                if (assetsToDownloadHeaderLen[i] != GetFileSize(assetsToDownloadHeader[i])) {
                                    Log(
                                        $"Asset {assetsToDownloadHeader[i]} local len: {GetFileSize(assetsToDownloadHeader[i])} server len: {assetsToDownloadHeaderLen[i]}. Should download.");
                                    assetsToDownload.Add(assetsToDownloadHeader[i]);
                                } else {
                                    Log($"Not necessary to download Asset {assetsToDownloadHeader[i]}. Local and server length are equal.");
                                }
                            }

                            if (assetsToDownload.Count > 0) {
                                currentAssetInDownload = 0;
                                Load(assetsToDownload[currentAssetInDownload]);
                            } else {
                                Log($"Finished Ok");
                                state = State.Idle;
                            }
                        }
                    } else {
                        Log($"Cannot download header asset {assetsToDownload[currentAssetInDownload]} Response code: {www.responseCode}");
                    }
                }

                break;
        }
        
    }

    /// <summary>
    /// Returns a list with all the assets in storage
    /// </summary>
    private static List<string> ReadNamesOfAssetsInStorage() {
        string files = PlayerPrefs.GetString(xPromoStorageKey, string.Empty);
        if (!string.IsNullOrEmpty(files)) {
            string[] stringArr = files.Split(';');
            return new List<string>(stringArr);
        } else {
            return null;
        }
    }

    /// <summary>
    /// Write the list of assets that are in the storage
    /// </summary>
    private static void WriteNamesOfAssetsInStorage(List<string> stringList) {
        if (stringList == null || stringList.Count == 0) {
            PlayerPrefs.DeleteKey(xPromoStorageKey);
            return;
        }

        string files = "";
        string[] stringArr = stringList.ToArray();

        for (int i = 0; i < stringArr.Length; i++) {
            files += stringArr[i];
            if (i < stringArr.Length - 1) files += ";";
        }
        Debug.Log($"Files in asset storage: '{files}'");
        PlayerPrefs.SetString(xPromoStorageKey, files);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Start downloading
    /// </summary>
    public void StartDownload(string baseUrl, List<string> assetsNotFilteredList)
    {
        this.baseUrl = baseUrl;
        
        PurgeStorage(assetsNotFilteredList);

        assetsToDownload = new List<string>();
        assetsToDownloadHeader = new List<string>();
        assetsToDownloadHeaderLen = new List<long>();

        // Step 1. I will check all the files of the not filtered list if they are on the local storage
        // If they are not in the local storage, will be added to the list of assets to download
        // If they are in the local storage, will be added to the list of assets to download its header (to check later if it's necessary to download them again)
        for (int i = 0; i < assetsNotFilteredList.Count; i++) {
            if (ExistsInStorage(assetsNotFilteredList[currentAssetInDownload])) {
                Log($"Asset {assetsNotFilteredList[currentAssetInDownload]} exists on storage. Should download header to check its size.");
                assetsToDownloadHeader.Add(assetsNotFilteredList[i]);
            } else {
                Log($"Asset {assetsNotFilteredList[currentAssetInDownload]} does not exist on storage. Should download");
                assetsToDownload.Add(assetsNotFilteredList[i]);
            }
        }

        // Start the process of download headers or files
        currentAssetInDownload = 0;
        if (assetsToDownloadHeader.Count > 0) {
            LoadHeader(assetsToDownloadHeader[currentAssetInDownload]);
        } else if (assetsToDownload.Count > 0) {
            Load(assetsToDownload[currentAssetInDownload]);
        } else {
            Log($"Download Manager finishes. It's not necessary to download any header or file.");
        }

    }

    /// <summary>
    /// Remove local assets that are not in the list to download
    /// </summary>
    private void PurgeStorage(List<string> assetsNotFilteredList) {
        // If assetsInStorage is null, nothing to purge
        if (assetsInStorage == null) return;

        List<string> assetsInStorageNew = new List<string>();
        for (int i = 0; i < assetsInStorage.Count; i++) {
            if (!assetsNotFilteredList.Contains(assetsInStorage[i])) {
                Log($"Asset {assetsInStorage[i]} was removed from local storage");
                RemoveFromStorage(assetsInStorage[i]);
            } else {
                assetsInStorageNew.Add(assetsInStorage[i]);
            }
        }

        assetsInStorage = assetsInStorageNew;
        
        if (logOnUI) xPromoLogger.Instance.LogLocalStorageItems(assetsInStorage);
        
        WriteNamesOfAssetsInStorage(assetsInStorage);
    }

    /// <summary>
    /// Download the specified asset
    /// </summary>
    private void Load(string fileName) {
        Log($"Downloading asset {fileName}...");

        www = UnityWebRequest.Get($"{baseUrl}/{fileName}");
        www.SendWebRequest();
        state = State.Downloading;
    }

    /// <summary>
    /// Download the header of the specified asset
    /// </summary>
    private void LoadHeader(string fileName) {
        Log($"Downloading header asset {fileName}...");
        www = UnityWebRequest.Head($"{baseUrl}/{fileName}");
        www.SendWebRequest();
        state = State.HeaderDownloading;
    }

    public bool IsDone() {
        return state == State.End;
    }

    /// <summary>
    /// Returns the byte[] of the downloaded asset
    /// </summary>
    /// <returns></returns>
    public byte[] GetData(string filename) {
        if (File.Exists($"{Application.persistentDataPath}/{filename}")) {
            return File.ReadAllBytes($"{Application.persistentDataPath}/{filename}");
        } else {
            Log($"Tried to get data from a non existent asset ({filename})");
            return null;
        }
    }

    /// <summary>
    /// Returns if the specified item exists in local storage
    /// </summary>
    public bool ExistsInStorage(string filename) {
        return File.Exists($"{Application.persistentDataPath}/{filename}");
    }

    /// <summary>
    /// Remove the specified file from local storage
    /// </summary>
    private void RemoveFromStorage(string filename) {
        File.Delete($"{Application.persistentDataPath}/{filename}");
    }

    /// <summary>
    /// Returns the size of the specified filename
    /// </summary>
    private long GetFileSize(string filename) {
        if (File.Exists($"{Application.persistentDataPath}/{filename}")) {
            var fi = new FileInfo($"{Application.persistentDataPath}/{filename}");
            return fi.Length;
        } else {
            return 0;
        }
    }

    public void RemoveAllLocalFiles() {
        for (int i = 0; i < assetsInStorage.Count; i++) {
            RemoveFromStorage(assetsInStorage[i]);
        }

        assetsInStorage.Clear();
        WriteNamesOfAssetsInStorage(assetsInStorage);
        
        if (logOnUI) {
            logger.LogLocalStorageItems(assetsInStorage);
        }
    }
    
    private void Log(string logText) {

        Debug.Log($"[xPromo] {logText}");

        if (logOnUI) {
            logger.LogDownloadManagerAppend(logText);
        }
    }
}
