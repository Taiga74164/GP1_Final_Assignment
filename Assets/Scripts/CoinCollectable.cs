using UnityEngine;

public class CoinCollectable : Collectable
{
    [SerializeField] public PlayerController Player;
    [SerializeField] public int CoinValue = 1;

    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (other.gameObject.tag == "Player")
            Player.AddScore(CoinValue);
    }

    void Update()
    {
        transform.Rotate(new Vector3(1f, 0f));
    }
}
