using UnityEngine;
public class Show : MonoBehaviour
{
    public void ShowObject()
    {
        gameObject.SetActive(true);
    }
    public void HideObject()
    {
        gameObject.SetActive(false);
    }
}