using UnityEngine;
using UnityEngine.UI;

namespace QuietSam.Common
{
    public class StatsBarUI : MonoBehaviour
    {

        public Image fillImage;

        [Header("Shake Settings")]
        [SerializeField] private bool enableShakeOnDamage = true;
        [SerializeField] private ShakeEffect shakeEffect;

        [Header("Events")]
        [SerializeField] private Int2EventChannel _statChangedEvent;

        private int previousStat;

        private float startingScale;

        private void Awake()
        {
            if (fillImage == null)
                fillImage = GetComponent<Image>();

            if (_statChangedEvent == null)
            {
                Debug.LogError("HealthChangedEvent is not set");
                return;
            }

            _statChangedEvent.OnEventRaised += UpdateBar;

            startingScale = fillImage.transform.localScale.x;
        }

        private void UpdateBar(int current, int max)
        {
            if (fillImage == null) return;

            // Check for damage (health decreased)
            if (enableShakeOnDamage && shakeEffect != null)
            {
                if (current < previousStat)
                {
                    // Calculate damage amount for shake intensity
                    int damage = previousStat - current;
                    float shakeStrength = Mathf.Clamp(damage * 2f, 5f, 20f);
                    shakeEffect.ShakeWithCustomSettings(0.5f, shakeStrength, Color.red);
                }
                previousStat = current;
            }

            float fill = (float)current / max;
            fill = Mathf.Clamp01(fill);

            if (fillImage.type == Image.Type.Filled)
            {
                fillImage.fillAmount = fill;
            }
            else
            {
                Vector3 scale = fillImage.transform.localScale;
                scale.x = fill * startingScale;
                fillImage.transform.localScale = scale;
            }
        }
    }
}