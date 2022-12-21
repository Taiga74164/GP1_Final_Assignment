using UnityEngine;

public class Collectable : MonoBehaviour
{
    [SerializeField] public AudioSource CollectAudio;

    public virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Destroy(gameObject);
            CollectAudio.Play();
        }
    }
}
