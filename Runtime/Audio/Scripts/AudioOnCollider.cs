using UnityEngine;
public class AudioOnCollider : MonoBehaviour
{
    [SerializeField] private SfxEvent sfx;


    private void OnTriggerEnter(Collider other)
    {
        
        
        {
            if(sfx != null)
            {
                AudioManager.Instance.Play(sfx);
            }
        }
    }

}
