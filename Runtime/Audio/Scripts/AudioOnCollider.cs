using UnityEngine;
namespace QuietSam.Common
{
    public class AudioOnCollider : MonoBehaviour
    {
        [SerializeField] private SfxEvent sfx;


        private void OnTriggerEnter(Collider other)
        {


            {
                if (sfx != null)
                {
                    AudioManager.Instance.Play(sfx);
                }
            }
        }

    }
}