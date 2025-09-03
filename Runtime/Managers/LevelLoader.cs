using UnityEngine;
using UnityEngine.SceneManagement;

namespace QuietSam.Common
{
    public static class LevelLoader
    {
        public static void ReloadScene()
        {
            LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public static void LoadScene(Scene scene)
        {
            LoadScene(scene.buildIndex);
        }

        public static void LoadScene(string sceneName)
        {
            Debug.Log($"[LevelLoader] Loading scene with name {sceneName}.");

            // TODO: add transition between scenes

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        public static void LoadScene(int buildIndex)
        {
            Debug.Log($"[LevelLoader] Loading scene with buildIndex {buildIndex}.");

            // TODO: add transition between scenes

            SceneManager.LoadScene(buildIndex, LoadSceneMode.Single);
        }
    }
}