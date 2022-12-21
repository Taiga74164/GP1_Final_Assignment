using UnityEngine;

public class Platform : MonoBehaviour
{
    [SerializeField] public GameObject Player;

    /// <summary>
    /// To Sean: In Platform1, I didn't set the Collider properly so you will have to move along with it.
    /// Reason is because player is having trouble with movement when parent is set lol
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        Player.transform.SetParent(transform);
    }

    private void OnTriggerExit(Collider other)
    {
        Player.transform.SetParent(null);
    }
}
