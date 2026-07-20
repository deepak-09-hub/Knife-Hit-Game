using UnityEngine;

public class TrunkController : MonoBehaviour
{
    public int health = 5;
    public Trunk[] trunkPrefabs;
    public Trunk[] bossTrunkPrefabs;
    public Trunk currentTrunk;
    public int currentTrunkIndex = 0;

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
            Debug.LogError("TrunkController needs at least one normal Trunk prefab.");
            return;
        }

        if (bossTrunkPrefabs == null || bossTrunkPrefabs.Length == 0)
        {
            Debug.LogError("TrunkController needs at least one boss Trunk prefab.");
            return;
        }

        if (currentTrunk != null)
        {
            Destroy(currentTrunk.gameObject);
        }

        bool isBossLevel = GameController.instance != null && GameController.instance.level > 0 && GameController.instance.level % 4 == 0;

        if(PowerUpsMenuController.Instance.superStrikeUnlocked && GameController.instance.ActivePowerUp == GameController.PowerUpType.None)
        {
            int randomChance = Random.Range(0, 100);
            if (randomChance < 5)
            {
                GameController.instance.ActivePowerUp = GameController.PowerUpType.SuperStrike;
                GameController.instance.ShowPowerUpAnimationAndWait();
            }
        }
        else if(GameController.instance.ActivePowerUp == GameController.PowerUpType.SuperStrike)
        {
            GameController.instance.ActivePowerUp = GameController.PowerUpType.None;
        }

        if (isBossLevel)
        {
            int bossIndex = Random.Range(0, bossTrunkPrefabs.Length);

            currentTrunk = Instantiate(bossTrunkPrefabs[bossIndex], transform.position, transform.rotation, transform);
        }
        else
        {
            int prefabIndex = currentTrunkIndex % trunkPrefabs.Length;
            currentTrunk = Instantiate(trunkPrefabs[prefabIndex], transform.position, transform.rotation, transform);
            currentTrunkIndex = (currentTrunkIndex + 1) % trunkPrefabs.Length;
        }

        currentTrunk.Initialize();

        if (GameController.instance != null && GameController.instance.ActivePowerUp == GameController.PowerUpType.ControlTrunkMovement)
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
