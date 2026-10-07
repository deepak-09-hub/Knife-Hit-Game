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

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        // Space, left mouse click, or right mouse click
        throwPressed =
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetMouseButtonDown(0) ||
            Input.GetMouseButtonDown(1);
#endif

#if UNITY_ANDROID || UNITY_IOS
    // Mobile touch
    if (Input.touchCount > 0 &&
        Input.GetTouch(0).phase == TouchPhase.Began)
    {
        throwPressed = true;
    }
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

        Knife newKnife = Instantiate(knifePrefab, transform.position, transform.rotation, transform);

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
        SoundManager.Instance.PlaySFX("thorow");
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

    public void SetKnifePrefab(Knife selectedKnifePrefab)
    {
        if (selectedKnifePrefab == null)
        {
            Debug.LogError("Cannot set a null knife prefab.");
            return;
        }

        knifePrefab = selectedKnifePrefab;
    }

}