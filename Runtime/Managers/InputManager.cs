using UnityEngine;
namespace QuietSam.Common
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        public InputSystem_Actions_QS Actions;
        public InputSystem_Actions_QS.PlayerActions Player => Actions.Player;
        public InputSystem_Actions_QS.UIActions UI => Actions.UI;

        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;
        }

        private void OnEnable() {
            Actions = new InputSystem_Actions_QS();
            Actions.Enable();
        }

        private void OnDisable() {
            Actions.Disable();
        }
    }
}