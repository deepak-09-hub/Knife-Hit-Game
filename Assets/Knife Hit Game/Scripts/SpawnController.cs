using UnityEngine;

public class SpawnController : MonoBehaviour
{
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
        bool throwPressed = false;

#if UNITY_EDITOR || UNITY_STANDALONE
        // Mouse click in Unity Editor / PC build.
        throwPressed = Input.GetKeyDown(KeyCode.Space);
#else
    // One screen tap on Android / iPhone.
    throwPressed = Input.touchCount > 0 &&
                   Input.GetTouch(0).phase == TouchPhase.Began;
#endif

        if (throwPressed &&
            currentKnife != null &&
            currentKnife.canHit)
        {
            ThrowObject();
        }
    }


    public void SpawnOnject()
    {
        if (currentKnife != null)
        {
            return;
        }

        if (knifePrefab == null)
        {
            Debug.LogError("Knife Prefab is not assigned in SpawnController.");
            return;
        }

        if (TrunkController.instance == null ||
            TrunkController.instance.health <= 0)
        {
            return;
        }

        Knife newKnife = Instantiate(
            knifePrefab,
            transform.position,
            transform.rotation,
            transform
        );

        newKnife.scriptEnabled = false;
        newKnife.throow = false;
        newKnife.canHit = true;

        currentKnife = newKnife;
    }

    public void ThrowObject()
    {
        if (currentKnife == null || !currentKnife.canHit)
        {
            return;
        }

        Knife knifeToThrow = currentKnife;

        // Clear this immediately because the knife is no longer waiting at spawn.
        currentKnife = null;

        knifeToThrow.scriptEnabled = true;
        knifeToThrow.canHit = false;
        knifeToThrow.throow = true;
        knifeToThrow.speed = knifeToThrow.currentSpeed;
    }

    // knifeToKeep is the knife currently falling after a failed hit.
    public void ClearKnives(Knife knifeToKeep = null)
    {
        Knife[] allKnives = FindObjectsOfType<Knife>();

        foreach (Knife knife in allKnives)
        {
            if (knife != null && knife != knifeToKeep)
            {
                Destroy(knife.gameObject);
            }
        }

        currentKnife = null;
    }
}