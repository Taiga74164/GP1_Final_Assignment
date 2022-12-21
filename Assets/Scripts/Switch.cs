using UnityEngine;

public class Switch : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] public AudioSource SwitchAudio;

    [Header("Door")]
    [SerializeField] public Transform Door;

    private bool _rotateDoor = false;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            SwitchAudio.Play();

            if (!_rotateDoor)
            {
                Door.Rotate(65f, 0, 0);
                _rotateDoor = true;
            }
        }
    }
}
