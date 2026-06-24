using UnityEngine;

public class Apple : MonoBehaviour
{
    [Min(1)]
    [SerializeField] private int appleAmount = 1;

    private bool collected;

    public void Collect()
    {
        if (collected)
        {
            return;
        }

        collected = true;
        GameController.instance.CollectApple(appleAmount);
        Destroy(gameObject);
    }
}
