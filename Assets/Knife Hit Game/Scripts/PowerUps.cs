using UnityEngine;

public class PowerUps : MonoBehaviour
{
    [SerializeField] private GameController.PowerUpType powerUpType;

    [Tooltip("Used only when Power Up Type is Multiplier.")]
    [Min(1)]
    [SerializeField] private int multiplierValue = 2;

    private bool collected;

    public bool Collect()
    {
        if (collected)
        {
            return false;
        }

        bool activated = GameController.instance.TryActivatePowerUp(powerUpType, multiplierValue);
        if (!activated)
        {
            return false;
        }

        collected = true;
        Destroy(gameObject);
        return true;
    }
}
