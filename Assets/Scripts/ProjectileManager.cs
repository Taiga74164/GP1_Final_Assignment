using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    [Header("Bullet")]
    [SerializeField] public GameObject BulletPrefab;
    [SerializeField] public Transform SpawnPoint;
    [SerializeField] public float BulletSpeed = 50f;

    [Header("Camera")]
    [SerializeField] public Camera MainCamera;

    [Header("Audio")]
    [SerializeField] public AudioSource GunShot;

    [Header("HUD")]
    [SerializeField] public GameObject HUD;

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (!HUD.activeSelf)
                return;

            // Get middle position of the screen
            var ray = MainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            var bullet = Instantiate(BulletPrefab, SpawnPoint.position, SpawnPoint.rotation);
            var bulletRb = bullet.GetComponent<Rigidbody>();
            bulletRb.AddForce(ray.direction * BulletSpeed, ForceMode.VelocityChange);

            GunShot.Play();
        }
    }
}
