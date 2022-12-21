using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] public Transform CameraPosition;

    void Update()
    {
        transform.position = CameraPosition.position;
    }
}
