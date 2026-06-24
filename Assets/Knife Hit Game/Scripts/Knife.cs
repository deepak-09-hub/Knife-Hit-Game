using UnityEngine;

public class Knife : MonoBehaviour {

    public float speed = 20f;
    public float currentSpeed = 20f;
    Rigidbody2D knifeRigid;
    public bool canHit = false;
    public bool throow = false;
    [SerializeField] public GameObject spawn;

    public bool scriptEnabled = true;

	// Use this for initialization
	void Start () 
    {
        knifeRigid = GetComponent<Rigidbody2D>();
        spawn = GameController.instance.spawnPoint;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (throow)
        {
            knifeRigid.MovePosition(knifeRigid.position + Vector2.up * speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(!scriptEnabled)
            return;

        if (collider.gameObject.CompareTag(Tags.Trunk.ToString()))
        {
            canHit = false;
            transform.parent = collider.transform;
            TrunkController.instance.Damage(1);
            GameController.instance.SetScore(1);
            transform.GetComponent<Knife>().enabled = false;
            SpawnController.instance.SpawnOnject();
        }
        else if (collider.gameObject.CompareTag(Tags.Knife.ToString()))
        {
            if (GameController.instance.lives > 0)
            {
                GameController.instance.lives--;
            }
            else
            {
                Die();
            }
        }
    }

    private void Die()
    {
        canHit = false;
        GameController.instance.ResetScore();
        scriptEnabled = false;
        knifeRigid.bodyType = RigidbodyType2D.Dynamic;
        knifeRigid.velocity = Vector2.zero;
        knifeRigid.AddForce(Vector2.down * 500f, ForceMode2D.Impulse);
        GameController.instance.ShowRestartScreen();
    }
}
