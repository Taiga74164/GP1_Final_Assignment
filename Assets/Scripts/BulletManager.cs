using UnityEngine;

public class BulletManager : MonoBehaviour
{
    [SerializeField] public float CloneCount = 4;

    void Awake()
    {
        Destroy(this.gameObject, CloneCount);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bullet")
            Destroy(collision.gameObject);

        Destroy(this.gameObject);
    }
}
