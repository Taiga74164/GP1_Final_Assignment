using UnityEngine;

public class EndCollider : MonoBehaviour
{
    [SerializeField] PlayerController player;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
            StartCoroutine(player.Restart(5));
    }
}
