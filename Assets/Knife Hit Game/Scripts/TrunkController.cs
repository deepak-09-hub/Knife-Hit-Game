using UnityEngine;
using TMPro;

public class TrunkController : MonoBehaviour 
{
    public int health = 5;

    public Trunk[] trunkPrefabs;
    public Trunk currentTrunk;


    public static TrunkController instance;

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

    public void spwanTrunk()
    {
        if (currentTrunk != null)
        {
            Destroy(currentTrunk.gameObject);
        }
        Trunk trunk = Instantiate(trunkPrefabs[Random.Range(0, trunkPrefabs.Length - 1)], transform.position, transform.rotation, this.transform);
        currentTrunk = trunk;
        health = currentTrunk.maxHealth;
        GameController.instance.updateHealthText(health);
    }

    // Decrease health
    public void Damage(int value)
    {
        if (currentTrunk == null)
            return;

        currentTrunk.TakeDamage(value);
        health = currentTrunk.currentHealth;
        GameController.instance.updateHealthText(health);

        if (health <= 0)
        {
            SpawnController.instance.ClearKnives();
            spwanTrunk();
            SpawnController.instance.SpawnOnject();
        }
    }
}
