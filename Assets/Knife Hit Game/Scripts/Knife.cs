using System.Collections;
using UnityEngine;

public class Knife : MonoBehaviour
{
    public float speed = 20f;
    public float currentSpeed = 20f;

    private Rigidbody2D knifeRigid;
    private Collider2D knifeCollider;

    public bool canHit = false;
    public bool throow = false;

    [SerializeField] public GameObject spawn;
    public bool scriptEnabled = false;

    private bool secondChance = false;
    private bool impactHandled = false;
    private bool isDying = false;

    private void Awake()
    {
        knifeRigid = GetComponent<Rigidbody2D>();
        knifeCollider = GetComponent<Collider2D>();

        if (knifeRigid == null)
        {
            Debug.LogError("Knife prefab is missing Rigidbody2D.", gameObject);
        }
    }

    private void Start()
    {
        if (GameController.instance != null)
        {
            spawn = GameController.instance.SpawnPoint;
        }
    }

    private void FixedUpdate()
    {
        if (throow && knifeRigid != null)
        {
            knifeRigid.MovePosition(
                knifeRigid.position + Vector2.up * speed * Time.deltaTime
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!scriptEnabled || impactHandled)
        {
            return;
        }

        Apple apple = collider.GetComponentInParent<Apple>();

        if (apple != null || collider.gameObject.CompareTag(Tags.Apple.ToString()))
        {
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
            impactHandled = true;

            canHit = false;
            throow = false;
            scriptEnabled = false;

            //transform.SetParent(collider.transform);

            SoundManager.Instance.PlaySFX(TrunkController.instance.currentTrunk.soundClipName);
            int damage = GameController.instance.GetDamageForTrunkHit();
            TrunkController.instance.Damage(damage);
            TrunkController.instance.currentTrunk.SetLastHitPoint(collider.ClosestPoint(transform.position));
            TrunkController.instance.currentTrunk.AddToExtraObjects(knifeRigid);
            if (TrunkController.instance.currentTrunk.currentHealth <= 0)
            {
                throow = true;
            }
            else
            {
                transform.SetParent(collider.transform);
                enabled = false;
            }
            GameController.instance.SetScore(1);


            if (SpawnController.instance != null)
            {
                SpawnController.instance.SpawnOnject();
            }


            return;
        }

        // Safer than relying only on the Knife tag.
        Knife otherKnife = collider.GetComponentInParent<Knife>();

        if (otherKnife != null && otherKnife != this)
        {
            impactHandled = true;

            Debug.Log("Knife hit knife: " + otherKnife.name);

            secondChance = GameController.instance.TryUseSecondChance();

            if (secondChance)
            {
                SoundManager.Instance.PlaySFX("invalidhit");
                RecoverFromKnifeCollision();
            }
            else
            {
                SoundManager.Instance.PlaySFX("invalidhit");
                Die();
            }
        }
    }

    private void RecoverFromKnifeCollision()
    {
        canHit = false;
        throow = false;
        scriptEnabled = false;

        if (knifeCollider != null)
        {
            knifeCollider.enabled = false;
        }

        if (SpawnController.instance != null)
        {
            SpawnController.instance.SpawnOnject();
        }

        if (knifeRigid != null)
        {
            ThrowKnife();
        }

        Destroy(gameObject, 2);
    }

    private void Die()
    {
        if (isDying)
        {
            return;
        }

        isDying = true;

        canHit = false;
        throow = false;
        scriptEnabled = false;

        // This falling knife must stop triggering future losses.
        if (knifeCollider != null)
        {
            knifeCollider.enabled = false;
        }

        if (knifeRigid != null)
        {
            ThrowKnife();
        }

        StartCoroutine(Failed());

        // Lets the failed knife fall briefly, but it cannot collide anymore.
        Destroy(gameObject, 2.5f);
    }

    private void OnDestroy()
    {
        if (SpawnController.instance != null &&
            SpawnController.instance.currentKnife == this)
        {
            SpawnController.instance.currentKnife = null;
        }
    }

    public void ThrowKnife()
    {
        knifeRigid.bodyType = RigidbodyType2D.Dynamic;
        knifeRigid.velocity = Vector2.zero;
        knifeRigid.AddForce(Vector2.down * 10f, ForceMode2D.Impulse);
        knifeRigid.AddTorque(Random.Range(5f, 20f) * ((Random.Range(0, 2) % 2) == 0 ? 1 : -1), ForceMode2D.Impulse);
    }

    IEnumerator Failed()
    {
        yield return new WaitForSeconds(0.5f);

        if (TrunkController.instance != null)
        {
            TrunkController.instance.DestroyCurrentTrunk();
        }

        // Remove every other knife, including old leftovers from earlier rounds.
        if (SpawnController.instance != null)
        {
            SpawnController.instance.ClearKnives(this);
        }

        if (GameController.instance != null)
        {
            GameController.instance.ResetScore();
            GameController.instance.ShowRestartScreen();
        }
    }
}