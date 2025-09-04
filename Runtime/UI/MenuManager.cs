using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
namespace QuietSam.Common {

    [System.Serializable]
    struct MenuButtonEvent
    {

        public enum ButtonActions
        {
            LoadScene,
            RestartScene,
            CloseWindow,
            OpenWindow,
            QuitGame,

        }
        [SerializeField] private Button button;
        [SerializeField] private ButtonActions actionToPerform;
        [SerializeField] private SceneField scene;
        [SerializeField] private List<GameObject> windows;
        public Button Button => button;
        public ButtonActions ActionToPerform => actionToPerform;
        public SceneField Scene => scene;
        public List<GameObject> Windows => windows;
        
    }
    public class MenuManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private List<MenuButtonEvent> buttonEvents;
        [SerializeField] private bool showOnAwake;



        private void Start()
        {
            if (showOnAwake)
            {
                // Ensure cursor is visible and unlocked in menu scenes
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                ShowMenu();
            }

            foreach (var buttonEvent in buttonEvents)
            {
                switch (buttonEvent.ActionToPerform)
                {
                    case MenuButtonEvent.ButtonActions.LoadScene:
                        buttonEvent.Button.onClick.AddListener( () => LoadGameScene(buttonEvent.Scene));
                        break;
                    case MenuButtonEvent.ButtonActions.RestartScene:
                        buttonEvent.Button.onClick.AddListener(RestartScene);
                        break;
                    case MenuButtonEvent.ButtonActions.CloseWindow:
                        buttonEvent.Button.onClick.AddListener( () => {foreach (var window in buttonEvent.Windows) CloseWindow(window);});
                        break;
                    case MenuButtonEvent.ButtonActions.OpenWindow:
                        buttonEvent.Button.onClick.AddListener( () => {foreach (var window in buttonEvent.Windows) ShowWindow(window);});
                        break;
                    case MenuButtonEvent.ButtonActions.QuitGame:
                        buttonEvent.Button.onClick.AddListener(QuitGame);
                        break;
                }
            }
            

        }

    
        private void Update()
        {
            if (mainMenu.activeSelf)
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
        }

        public void ShowWindow(GameObject window)
        {
            window.SetActive(true);
        }
        public void CloseWindow(GameObject window)
        {
            window.SetActive(false);
        }

        public void ShowMenu()
        {
            mainMenu.SetActive(true);

        }
        public void RestartScene()
        {
            LevelLoader.ReloadScene();
        }



        public void LoadGameScene(SceneField scene)
        {

            LevelLoader.LoadScene(scene);
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
