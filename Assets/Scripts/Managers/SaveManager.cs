using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveManager
{
    private const string SceneKey = "SceneName";
    private const string PlayerXKey = "PlayerX";
    private const string PlayerYKey = "PlayerY";
    private const string PlayerZKey = "PlayerZ";
    private const string PlayerHealthKey = "PlayerHealth";
    private const string PlayerDataValidKey = "PlayerDataValid";

    private const string CollectedItemsKey = "CollectedItemIds";
    private const string KilledEnemiesKey = "KilledEnemyIds";
    private const string KilledEnemyCountKey = "KilledEnemyCount";

    private static bool loadingSave;
    private static bool ignoreNextLoad;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        loadingSave = false;
        ignoreNextLoad = false;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(SceneKey);
    }

    public static bool IsLoadingSave()
    {
        return loadingSave && !ignoreNextLoad;
    }

    public static void SaveGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            Vector3 pos = player.transform.position;

            PlayerPrefs.SetFloat(PlayerXKey, pos.x);
            PlayerPrefs.SetFloat(PlayerYKey, pos.y);
            PlayerPrefs.SetFloat(PlayerZKey, pos.z);
            PlayerPrefs.SetInt(PlayerDataValidKey, 1);

            PlayerHealth health = player.GetComponent<PlayerHealth>();

            if (health != null)
                PlayerPrefs.SetInt(PlayerHealthKey, health.CurrentHealth);
        }

        PlayerPrefs.SetString(SceneKey, SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
    }

    public static void LoadGame()
    {
        if (!HasSave())
        {
            Debug.LogWarning("No save found.");
            return;
        }

        loadingSave = true;
        ignoreNextLoad = false;

        string sceneName = PlayerPrefs.GetString(SceneKey);
        SceneManager.LoadScene(sceneName);
    }

    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SceneKey);
        PlayerPrefs.DeleteKey(PlayerXKey);
        PlayerPrefs.DeleteKey(PlayerYKey);
        PlayerPrefs.DeleteKey(PlayerZKey);
        PlayerPrefs.DeleteKey(PlayerHealthKey);
        PlayerPrefs.DeleteKey(PlayerDataValidKey);
        PlayerPrefs.DeleteKey(CollectedItemsKey);
        PlayerPrefs.DeleteKey(KilledEnemiesKey);
        PlayerPrefs.DeleteKey(KilledEnemyCountKey);

        loadingSave = false;
        ignoreNextLoad = false;

        PlayerPrefs.Save();
    }

    public static void DisableLoadOnRestart()
    {
        ignoreNextLoad = true;
        loadingSave = false;
    }

    public static void FinishLoadingSave()
    {
        loadingSave = false;
        ignoreNextLoad = false;
    }

    public static void SaveCollectedItem(string pickupSaveId)
    {
        SaveIdToList(CollectedItemsKey, pickupSaveId);
    }

    public static bool IsItemCollected(string pickupSaveId)
    {
        return IsIdInList(CollectedItemsKey, pickupSaveId);
    }

    public static void SaveKilledEnemy(string enemySaveId)
    {
        SaveIdToList(KilledEnemiesKey, enemySaveId);
    }

    public static bool IsEnemyKilled(string enemySaveId)
    {
        return IsIdInList(KilledEnemiesKey, enemySaveId);
    }

    public static void SaveKilledEnemyCount(int count)
    {
        PlayerPrefs.SetInt(KilledEnemyCountKey, count);
        PlayerPrefs.Save();
    }

    public static int GetKilledEnemyCount()
    {
        return PlayerPrefs.GetInt(KilledEnemyCountKey, 0);
    }

    private static void SaveIdToList(string key, string id)
    {
        if (string.IsNullOrEmpty(id))
            return;

        List<string> ids = GetIdList(key);

        if (!ids.Contains(id))
            ids.Add(id);

        PlayerPrefs.SetString(key, string.Join("|", ids));
        PlayerPrefs.Save();
    }

    private static bool IsIdInList(string key, string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        return GetIdList(key).Contains(id);
    }

    private static List<string> GetIdList(string key)
    {
        string raw = PlayerPrefs.GetString(key, "");
        List<string> result = new List<string>();

        if (string.IsNullOrEmpty(raw))
            return result;

        string[] ids = raw.Split('|');

        foreach (string id in ids)
        {
            if (!string.IsNullOrEmpty(id))
                result.Add(id);
        }

        return result;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (ignoreNextLoad)
        {
            ignoreNextLoad = false;
            loadingSave = false;
            return;
        }

        if (!loadingSave)
            return;

        SaveLoader loader = Object.FindAnyObjectByType<SaveLoader>();

        if (loader == null)
        {
            GameObject obj = new GameObject("SaveLoader");
            loader = obj.AddComponent<SaveLoader>();
        }

        loader.StartCoroutine(loader.ApplySaveAfterFrame());
    }

    public static Vector3 GetSavedPlayerPosition()
    {
        return new Vector3(
            PlayerPrefs.GetFloat(PlayerXKey),
            PlayerPrefs.GetFloat(PlayerYKey),
            PlayerPrefs.GetFloat(PlayerZKey)
        );
    }

    public static bool HasValidPlayerData()
    {
        return PlayerPrefs.GetInt(PlayerDataValidKey, 0) == 1;
    }

    public static int GetSavedHealth(int fallback)
    {
        return PlayerPrefs.GetInt(PlayerHealthKey, fallback);
    }
}