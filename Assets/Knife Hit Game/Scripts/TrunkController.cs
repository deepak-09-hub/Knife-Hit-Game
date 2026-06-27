using UnityEngine;

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
        if (trunkPrefabs == null || trunkPrefabs.Length == 0)
        {
            Debug.LogError("TrunkController needs at least one Trunk prefab.");
            return;
        }

        if (currentTrunk != null)
        {
            Destroy(currentTrunk.gameObject);
        }

        int prefabIndex = Random.Range(0, trunkPrefabs.Length);
        currentTrunk = Instantiate(trunkPrefabs[prefabIndex], transform.position, transform.rotation, transform);
        currentTrunk.Initialize();

        if (GameController.instance != null &&
            GameController.instance.ActivePowerUp == GameController.PowerUpType.ControlTrunkMovement)
        {
            currentTrunk.SetManualControl(true);
        }

        SyncHealthFromCurrentTrunk();
    }

    public void Damage(int value)
    {
        if (currentTrunk == null || health <= 0)
        {
            return;
        }

        currentTrunk.TakeDamage(Mathf.Max(1, value));
        SyncHealthFromCurrentTrunk();

        if (health <= 0)
        {
            GameController.instance.MoveToNextLevel();
        }
    }

    public void SyncHealthFromCurrentTrunk()
    {
        if (currentTrunk == null)
        {
            return;
        }

        health = currentTrunk.currentHealth;
        GameController.instance.UpdateHitsLeftText(health);
    }

    public void DestroyCurrentTrunk()
    {
        if (currentTrunk != null)
        {
            Destroy(currentTrunk.gameObject);
            currentTrunk = null;
        }
    }
}
