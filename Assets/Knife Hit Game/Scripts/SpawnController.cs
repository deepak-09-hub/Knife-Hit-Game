using UnityEngine;


public class SpawnController : MonoBehaviour {

    public Knife knifePrefab;
    public Knife currentKnife;

    public static SpawnController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentKnife.canHit)
            {
                ThrowObject();
            }
        }
    }

    public void SpawnOnject()
    {
        if (currentKnife == null && TrunkController.instance.health > 0)
        {
            Knife newKnife = Instantiate(knifePrefab, transform.position, transform.rotation, this.transform);
            currentKnife = newKnife;
            currentKnife.throow = false;
            currentKnife.canHit = true;
        }
    }

    public void ThrowObject()
    {
        if (currentKnife != null)
        {
            currentKnife.canHit = false;
            currentKnife.throow = true;
            currentKnife.speed = currentKnife.currentSpeed;
            currentKnife = null;
        }
    }

    public void ClearKnives()
    {
        if (currentKnife != null)
        {
            Destroy(currentKnife.gameObject);
            currentKnife = null;
        }
    }
}
