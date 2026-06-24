using UnityEngine;

public class Knife : MonoBehaviour
{
    public float speed = 20f;
    public float currentSpeed = 20f;
    private Rigidbody2D knifeRigid;

    public bool canHit = false;
    public bool throow = false;
    [SerializeField] public GameObject spawn;
    public bool scriptEnabled = true;

    private void Start()
    {
        knifeRigid = GetComponent<Rigidbody2D>();
        spawn = GameController.instance.SpawnPoint;
    }

    private void FixedUpdate()
    {
        if (throow)
        {
            knifeRigid.MovePosition(knifeRigid.position + Vector2.up * speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!scriptEnabled)
        {
            return;
        }

        PowerUps powerUp = collider.GetComponentInParent<PowerUps>();
        if (powerUp != null)
        {
            powerUp.Collect();
            return;
        }

        Apple apple = collider.GetComponentInParent<Apple>();
        if (collider.gameObject.CompareTag(Tags.Apple.ToString()) || apple != null)
        {
            apple = collider.GetComponentInParent<Apple>();

            if (apple != null)
            {
                apple.Collect();
            }
            else
            {
                GameController.instance.CollectApple();
                Destroy(collider.gameObject);
            }

            return;
        }

        if (collider.gameObject.CompareTag(Tags.Trunk.ToString()))
        {
            canHit = false;
            transform.parent = collider.transform;

            int damage = GameController.instance.GetDamageForTrunkHit();
            TrunkController.instance.Damage(damage);
            GameController.instance.SetScore(1);

            enabled = false;
            SpawnController.instance.SpawnOnject();
        }
        else if (collider.gameObject.CompareTag(Tags.Knife.ToString()))
        {
            if (GameController.instance.TryUseSecondChance())
            {
                RecoverFromKnifeCollision();
            }
            else
            {
                Die();
            }
        }
    }

    private void RecoverFromKnifeCollision()
    {
        canHit = false;
        throow = false;
        scriptEnabled = false;

        Collider2D knifeCollider = GetComponent<Collider2D>();
        if (knifeCollider != null)
        {
            knifeCollider.enabled = false;
        }

        SpawnController.instance.SpawnOnject();
        Destroy(gameObject);
    }

    private void Die()
    {
        canHit = false;
        GameController.instance.FinalizeRun();
        GameController.instance.ResetScore();
        scriptEnabled = false;
        knifeRigid.bodyType = RigidbodyType2D.Dynamic;
        knifeRigid.velocity = Vector2.zero;
        knifeRigid.AddForce(Vector2.down * 500f, ForceMode2D.Impulse);
        GameController.instance.ShowRestartScreen();
    }
}
