using UnityEngine;
namespace QuietSam.Common {
    public class MainMenuManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject settingsMenu;


        [Header("Scenes")]
        [SerializeField] private SceneField gameScene;
        [SerializeField] private SceneField creditsScene;
        [SerializeField] private SceneField leaderboardScene;

        private void Awake()
        {
            // Ensure cursor is visible and unlocked in menu scenes
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            ShowMainMenu();
        }

        private void Update()
        {
            // Continuously ensure cursor is visible in menu scenes
            // This overrides any other scripts that might be hiding the cursor
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            if (!Cursor.visible)
            {
                Cursor.visible = true;
            }
        }

        public void ShowMainMenu()
        {
            mainMenu.SetActive(true);
            if (settingsMenu)
                settingsMenu.SetActive(false);
        }

        public void ShowSettingsMenu()
        {
            mainMenu.SetActive(false);
            settingsMenu.SetActive(true);
        }

        public void LoadGameScene()
        {
            
            LevelLoader.LoadScene(gameScene);
        }

        public void LoadCreditsScene()
        {
            LevelLoader.LoadScene(creditsScene);
        }

        public void LoadLeaderboardScene()
        {
            LevelLoader.LoadScene(leaderboardScene);
        }

        public void QuitGame()
        {
            Debug.Log("Quitting game.");

    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
                #endif
            }
        }
}
