using System.Collections;
using UnityEngine;

public class Trunk : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] public int maxHealth = 10;
    public int currentHealth;

    [Header("Speed")]
    [SerializeField] private float baseSpeed = 100f;
    [SerializeField] private float maxSpeed = 300f;

    [Header("Movement")]
    [SerializeField] private MovementType movementType;

    [Tooltip("Used by the new movement modes. Existing clockwise/anti-clockwise modes keep their original directions.")]
    [SerializeField] private bool newMovementsStartClockwise = true;

    [Header("Existing Random Freeze Settings")]
    [SerializeField] private float randomFreezeMinInterval = 1.5f;
    [SerializeField] private float randomFreezeMaxInterval = 3f;
    [SerializeField] private float randomFreezeMinDuration = 0.05f;
    [SerializeField] private float randomFreezeMaxDuration = 0.15f;

    [Header("Stop And Go")]
    [SerializeField] private float stopAndGoMoveDuration = 1.5f;
    [SerializeField] private float stopAndGoStopDuration = 0.4f;

    [Header("Timed Reverse")]
    [SerializeField] private float timedReverseInterval = 2f;

    [Header("Slow Reverse")]
    [SerializeField] private float slowReverseMoveDuration = 2f;
    [SerializeField] private float slowReverseSlowDownDuration = 0.5f;
    [SerializeField] private float slowReverseSpeedUpDuration = 0.5f;

    [Header("Hit Reverse")]
    [Min(1)]
    [SerializeField] private int hitReverseEveryHits = 1;

    [Header("Speed Pulse")]
    [SerializeField] private float speedPulseMinMultiplier = 0.5f;
    [SerializeField] private float speedPulseMaxMultiplier = 1.5f;
    [SerializeField] private float speedPulseHalfCycleDuration = 1f;

    [Header("Burst Rotation")]
    [SerializeField] private float burstRotationNormalDuration = 1.5f;
    [SerializeField] private float burstRotationDuration = 0.4f;
    [SerializeField] private float burstRotationSpeedMultiplier = 2f;

    [Header("Random Segment Speed")]
    [SerializeField] private float randomSegmentMinMultiplier = 0.5f;
    [SerializeField] private float randomSegmentMaxMultiplier = 1.75f;
    [SerializeField] private float randomSegmentMinDuration = 0.75f;
    [SerializeField] private float randomSegmentMaxDuration = 1.75f;

    [Header("Stutter")]
    [SerializeField] private float stutterMoveDuration = 0.25f;
    [SerializeField] private float stutterPauseDuration = 0.08f;

    [Header("Step Rotation")]
    [SerializeField] private float stepRotationDegrees = 20f;
    [SerializeField] private float stepRotationInterval = 0.25f;

    [Header("Wobble")]
    [Range(0f, 0.95f)]
    [SerializeField] private float wobbleSpeedAmount = 0.25f;
    [SerializeField] private float wobbleFrequency = 2f;

    [Header("Fake Stop")]
    [SerializeField] private float fakeStopMoveDuration = 1.5f;
    [SerializeField] private float fakeStopSlowDownDuration = 0.45f;
    [SerializeField] private float fakeStopHoldDuration = 0.2f;
    [SerializeField] private float fakeStopSpeedUpDuration = 0.35f;
    [Range(0f, 0.25f)]
    [SerializeField] private float fakeStopMinimumSpeedMultiplier = 0.05f;
    [SerializeField] private bool fakeStopReverseAfterPause;

    [Header("Horizontal Drift")]
    [SerializeField] private float horizontalDriftDistance = 1f;
    [SerializeField] private float horizontalDriftHalfCycleDuration = 1.25f;

    private float currentSpeed;
    private float speedMultiplier = 1f;
    private float movementElapsed;
    private int direction = 1;
    private int hitsSinceLastReverse;

    private bool isFrozen;
    private bool movingClockwise = true;
    private bool isInitialized;
    private bool manualControlActive;

    private Vector3 startingLocalPosition;

    public enum MovementType
    {
        Clockwise,
        AntiClockwise,
        AlternateDirection,
        SpeedUpClockwise,
        SpeedUpAntiClockwise,
        RandomFreeze,
        StopAndGo,
        TimedReverse,
        SlowReverse,
        HitReverse,
        SpeedPulse,
        BurstRotation,
        RandomSegmentSpeed,
        Stutter,
        StepRotation,
        Wobble,
        FakeStop,
        HorizontalDrift
    }

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (isInitialized)
        {
            return;
        }

        isInitialized = true;
        currentHealth = maxHealth;
        currentSpeed = baseSpeed;
        startingLocalPosition = transform.localPosition;

        direction = (movementType == MovementType.AntiClockwise ||
                     movementType == MovementType.SpeedUpAntiClockwise)
            ? -1
            : (newMovementsStartClockwise ? 1 : -1);

        StartMovementRoutine();
    }

    private void Update()
    {
        if (manualControlActive)
        {
            return;
        }

        movementElapsed += Time.deltaTime;

        if (movementType == MovementType.HorizontalDrift)
        {
            UpdateHorizontalDrift();
        }

        if (isFrozen)
            return;

        switch (movementType)
        {
            case MovementType.Clockwise:
                Rotate(1);
                break;

            case MovementType.AntiClockwise:
                Rotate(-1);
                break;

            case MovementType.AlternateDirection:
                Rotate(movingClockwise ? 1 : -1);
                break;

            case MovementType.SpeedUpClockwise:
                Rotate(1);
                break;

            case MovementType.SpeedUpAntiClockwise:
                Rotate(-1);
                break;

            case MovementType.RandomFreeze:
                Rotate(1);
                break;

            case MovementType.StopAndGo:
            case MovementType.TimedReverse:
            case MovementType.SlowReverse:
            case MovementType.HitReverse:
            case MovementType.BurstRotation:
            case MovementType.RandomSegmentSpeed:
            case MovementType.Stutter:
            case MovementType.FakeStop:
            case MovementType.HorizontalDrift:
                Rotate(direction, speedMultiplier);
                break;

            case MovementType.SpeedPulse:
                UpdateSpeedPulse();
                Rotate(direction, speedMultiplier);
                break;

            case MovementType.Wobble:
                UpdateWobble();
                Rotate(direction, speedMultiplier);
                break;

            case MovementType.StepRotation:
                break;
        }
    }

    private void Rotate(int dir, float multiplier = 1f)
    {
        transform.Rotate(0f, 0f, currentSpeed * multiplier * dir * Time.deltaTime);
    }

    public void SetManualControl(bool enabled)
    {
        manualControlActive = enabled;
    }

    public void RotateManually(float degrees)
    {
        if (!manualControlActive)
        {
            return;
        }

        transform.Rotate(0f, 0f, degrees);
    }

    public void HalveRemainingHealth()
    {
        if (!isInitialized)
        {
            Initialize();
        }

        if (currentHealth <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(1, Mathf.CeilToInt(currentHealth / 2f));
        HandleDamageBasedMovement();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            return;
        }


        HandleDamageBasedMovement();

        if (movementType == MovementType.HitReverse && damage > 0)
        {
            HandleHitReverse(damage);
        }
    }

    private void HandleDamageBasedMovement()
    {
        float damagePercent = maxHealth > 0
            ? 1f - ((float)currentHealth / maxHealth)
            : 1f;

        currentSpeed = Mathf.Lerp(
            baseSpeed,
            maxSpeed,
            damagePercent);

        if (movementType == MovementType.AlternateDirection)
        {
            if (damagePercent >= 0.25f && damagePercent < 0.50f)
            {
                movingClockwise = false;
            }
            else if (damagePercent >= 0.50f && damagePercent < 0.75f)
            {
                movingClockwise = true;
            }
            else if (damagePercent >= 0.75f)
            {
                movingClockwise = false;
            }
        }
    }

    private void StartMovementRoutine()
    {
        switch (movementType)
        {
            case MovementType.RandomFreeze:
                StartCoroutine(RandomFreezeRoutine());
                break;

            case MovementType.StopAndGo:
                StartCoroutine(StopAndGoRoutine());
                break;

            case MovementType.TimedReverse:
                StartCoroutine(TimedReverseRoutine());
                break;

            case MovementType.SlowReverse:
                StartCoroutine(SlowReverseRoutine());
                break;

            case MovementType.BurstRotation:
                StartCoroutine(BurstRotationRoutine());
                break;

            case MovementType.RandomSegmentSpeed:
                StartCoroutine(RandomSegmentSpeedRoutine());
                break;

            case MovementType.Stutter:
                StartCoroutine(StutterRoutine());
                break;

            case MovementType.StepRotation:
                StartCoroutine(StepRotationRoutine());
                break;

            case MovementType.FakeStop:
                StartCoroutine(FakeStopRoutine());
                break;
        }
    }

    private void HandleHitReverse(int damage)
    {
        hitsSinceLastReverse += damage;
        int requiredHits = Mathf.Max(1, hitReverseEveryHits);

        while (hitsSinceLastReverse >= requiredHits)
        {
            hitsSinceLastReverse -= requiredHits;
            direction *= -1;
        }
    }

    private void UpdateSpeedPulse()
    {
        float halfCycle = Mathf.Max(0.01f, speedPulseHalfCycleDuration);
        float pulse = Mathf.PingPong(movementElapsed / halfCycle, 1f);
        speedMultiplier = Mathf.Lerp(speedPulseMinMultiplier, speedPulseMaxMultiplier, pulse);
    }

    private void UpdateWobble()
    {
        float wobble = Mathf.Sin(movementElapsed * Mathf.Max(0f, wobbleFrequency) * Mathf.PI * 2f);
        speedMultiplier = Mathf.Max(0f, 1f + (wobble * wobbleSpeedAmount));
    }

    private void UpdateHorizontalDrift()
    {
        float halfCycle = Mathf.Max(0.01f, horizontalDriftHalfCycleDuration);
        float horizontalOffset = Mathf.Sin(movementElapsed * Mathf.PI / halfCycle) * horizontalDriftDistance;
        transform.localPosition = startingLocalPosition + Vector3.right * horizontalOffset;
    }

    private IEnumerator RandomFreezeRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(GetRandomValue(randomFreezeMinInterval, randomFreezeMaxInterval));

            isFrozen = true;

            yield return new WaitForSeconds(GetRandomValue(randomFreezeMinDuration, randomFreezeMaxDuration));

            isFrozen = false;
        }
    }

    private IEnumerator StopAndGoRoutine()
    {
        while (true)
        {
            isFrozen = false;
            yield return new WaitForSeconds(PositiveDuration(stopAndGoMoveDuration));

            isFrozen = true;
            yield return new WaitForSeconds(PositiveDuration(stopAndGoStopDuration));

            isFrozen = false;
        }
    }

    private IEnumerator TimedReverseRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(PositiveDuration(timedReverseInterval));
            direction *= -1;
        }
    }

    private IEnumerator SlowReverseRoutine()
    {
        while (true)
        {
            speedMultiplier = 1f;
            yield return new WaitForSeconds(PositiveDuration(slowReverseMoveDuration));

            yield return BlendSpeedMultiplier(1f, 0f, slowReverseSlowDownDuration);
            direction *= -1;
            yield return BlendSpeedMultiplier(0f, 1f, slowReverseSpeedUpDuration);
        }
    }

    private IEnumerator BurstRotationRoutine()
    {
        while (true)
        {
            speedMultiplier = 1f;
            yield return new WaitForSeconds(PositiveDuration(burstRotationNormalDuration));

            speedMultiplier = Mathf.Max(0f, burstRotationSpeedMultiplier);
            yield return new WaitForSeconds(PositiveDuration(burstRotationDuration));
        }
    }

    private IEnumerator RandomSegmentSpeedRoutine()
    {
        while (true)
        {
            speedMultiplier = Mathf.Max(
                0f,
                GetRandomValue(randomSegmentMinMultiplier, randomSegmentMaxMultiplier));

            yield return new WaitForSeconds(
                GetRandomValue(randomSegmentMinDuration, randomSegmentMaxDuration));
        }
    }

    private IEnumerator StutterRoutine()
    {
        while (true)
        {
            isFrozen = false;
            yield return new WaitForSeconds(PositiveDuration(stutterMoveDuration));

            isFrozen = true;
            yield return new WaitForSeconds(PositiveDuration(stutterPauseDuration));

            isFrozen = false;
        }
    }

    private IEnumerator StepRotationRoutine()
    {
        while (true)
        {
            transform.Rotate(0f, 0f, stepRotationDegrees * direction);
            yield return new WaitForSeconds(PositiveDuration(stepRotationInterval));
        }
    }

    private IEnumerator FakeStopRoutine()
    {
        while (true)
        {
            speedMultiplier = 1f;
            yield return new WaitForSeconds(PositiveDuration(fakeStopMoveDuration));

            yield return BlendSpeedMultiplier(1f, fakeStopMinimumSpeedMultiplier, fakeStopSlowDownDuration);
            yield return new WaitForSeconds(PositiveDuration(fakeStopHoldDuration));

            if (fakeStopReverseAfterPause)
            {
                direction *= -1;
            }

            yield return BlendSpeedMultiplier(fakeStopMinimumSpeedMultiplier, 1f, fakeStopSpeedUpDuration);
        }
    }

    private IEnumerator BlendSpeedMultiplier(float from, float to, float duration)
    {
        float safeDuration = PositiveDuration(duration);
        float elapsed = 0f;

        while (elapsed < safeDuration)
        {
            elapsed += Time.deltaTime;
            speedMultiplier = Mathf.Lerp(from, to, elapsed / safeDuration);
            yield return null;
        }

        speedMultiplier = to;
    }

    private float GetRandomValue(float first, float second)
    {
        float min = Mathf.Min(first, second);
        float max = Mathf.Max(first, second);
        return Random.Range(min, max);
    }

    private float PositiveDuration(float duration)
    {
        return Mathf.Max(0.01f, duration);
    }
}
