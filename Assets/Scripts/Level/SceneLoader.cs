using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    [SerializeField] private string firstLevelSceneName = "Level 1";

    public void MainMenu()
    {
        Time.timeScale = 1f;
        GetTransitionManager().LoadSceneWithTransition(mainMenuSceneName);
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        SaveManager.DeleteSave();
        GetTransitionManager().LoadSceneWithTransition(firstLevelSceneName);
    }

    public void ContinueGame()
    {
        Time.timeScale = 1f;

        bool isMainMenu = string.Equals(
            SceneManager.GetActiveScene().name,
            mainMenuSceneName,
            System.StringComparison.OrdinalIgnoreCase
        );

       
        if (!isMainMenu && UIManager.uiManager != null)
        {
            UIManager.uiManager.ResumeGame();
            return;
        }

        
        if (SaveManager.HasSave())
        {
            SaveManager.LoadGame();
        }
        else
        {
            Debug.LogWarning("Continue was pressed, but no saved game exists.");
        }
    }

    public void SaveGame()
    {
        SaveManager.SaveGame();
        Debug.Log("Game saved.");
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SaveManager.DisableLoadOnRestart();

        int currentScene = SceneManager.GetActiveScene().buildIndex;
        GetTransitionManager().LoadSceneWithTransition(currentScene);
    }

    public void LoadNextScene()
    {
        Time.timeScale = 1f;

        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SaveManager.DeleteSave();
            GetTransitionManager().LoadSceneWithTransition(nextIndex);
        }
        else
        {
            Debug.LogWarning("No more scenes.");
        }
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.Selection.activeObject = null;
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private SceneTransitionManager GetTransitionManager()
    {
        if (SceneTransitionManager.instance != null)
            return SceneTransitionManager.instance;

        GameObject obj = new GameObject("SceneTransitionManager");
        return obj.AddComponent<SceneTransitionManager>();
    }
}