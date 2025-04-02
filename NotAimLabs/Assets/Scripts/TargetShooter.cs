using UnityEngine;

public class TargetShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Score score;

    [Header("Shooting Settings")]
    [SerializeField] private float maxShootDistance = 100f;
    [SerializeField] private float shootCooldown = 0.2f;
    private float nextFireTime = 0f;

    [Header("Scoring Settings")]
    [SerializeField] private float baseScore = 100f;
    [SerializeField] private float maxTimeBonus = 50f;
    [SerializeField] private float timeBonusDecayRate = 2f;
    [SerializeField] private int missPenalty = 25;

    [Header("Debug")]
    public float timeSinceLastHit;

    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (score == null && Score.Instance != null)
        {
            score = Score.Instance;
        }
    }

    private void Update()
    {
        if (CountdownTimer.Instance != null && CountdownTimer.Instance.isGameOver)
        {
            return;
        }

        timeSinceLastHit += Time.deltaTime;

        if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + shootCooldown;
            Shoot();
        }
    }

    private void Shoot()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        bool hitTarget = false;

        Debug.DrawRay(ray.origin, ray.direction * maxShootDistance, Color.red, 1f);

        if (Physics.Raycast(ray, out hit, maxShootDistance))
        {
            Target target = hit.collider.GetComponent<Target>();
            if (target != null)
            {
                ProcessTargetHit(target, hit.point);
                hitTarget = true;
            }
        }

        if (!hitTarget)
        {
            ProcessMiss();
        }
    }

    private void ProcessTargetHit(Target target, Vector3 hitPoint)
    {
        float timeBonus = maxTimeBonus * Mathf.Exp(-timeBonusDecayRate * timeSinceLastHit);

        float accuracyBonus = score != null ? score.scoreAccuracy : 0f;

        if (score != null)
        {
            score.AddHit();
            score.AddScore(baseScore, timeBonus, accuracyBonus);
        }

        target.Hit();

        timeSinceLastHit = 0f;

        Debug.Log("Target hit at position: " + hitPoint);
    }

    private void ProcessMiss()
    {
        if (score != null)
        {
            score.AddMiss();
            score.DeductScore(missPenalty);
        }

        Debug.Log("Shot missed - no target in raycast path.");
    }

    public void ResetGame()
    {
        if (score != null)
        {
            score.ResetScore();
        }

        timeSinceLastHit = 0f;
    }

    private void OnValidate()
    {
        if (timeBonusDecayRate < 0)
        {
            timeBonusDecayRate = 0;
        }
    }
}