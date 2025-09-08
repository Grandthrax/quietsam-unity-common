using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace QuietSam.Common
{
    public class WarningSystemControl : MonoBehaviour
    {

        public static WarningSystemControl Instance;

        bool isWarningActive = false;

        [SerializeField] private GameObject warningSystemUIPanel;
        [SerializeField] private TMP_Text warningText;
        [SerializeField] private float warningDuration = 1f;
        [SerializeField] private StringEventChannel warningEvent;
        Coroutine _currentWarningCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            warningSystemUIPanel.SetActive(false);
            if (warningEvent != null)
            {
                warningEvent.OnEventRaised += AddWarning;
            }
        }

        public void AddWarning(string warning)
        {
            if (isWarningActive)
            {
                StopCoroutine(_currentWarningCoroutine);
            }
            _currentWarningCoroutine = StartCoroutine(ShowWarning(warning));
        }



        IEnumerator ShowWarning(string warning)
        {
            isWarningActive = true;
            warningSystemUIPanel.SetActive(true);
            warningText.text = warning;

            ShakeEffect shakeEffect = warningText.GetComponent<ShakeEffect>();
            if (shakeEffect != null)
            {
                shakeEffect.Shake();
            }
            yield return new WaitForSeconds(warningDuration);
            warningSystemUIPanel.SetActive(false);
            isWarningActive = false;
        }




    }
}
