using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UINumberUpdater : MonoBehaviour
{
    public TMP_Text text;
    public IntEventChannel statChangedEvent;

    void Start()
    {
        if (text == null)
        {
            text = GetComponent<TMP_Text>();
        }

        if (statChangedEvent != null)
        {
            statChangedEvent.OnEventRaised += UpdateText;
        }
    }

    void UpdateText(int current)
    {
        text.text = current.ToString();
    }
}