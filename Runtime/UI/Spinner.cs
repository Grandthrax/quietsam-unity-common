using UnityEngine;

public class Spinner : MonoBehaviour
{
    [SerializeField] private float speed = 180f; // degrees per second

    void Update()
    {
        transform.Rotate(0f, 0f, speed * Time.deltaTime);
    }
}