using UnityEngine;

public class TargetShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private Score score;

    [Header("Scoring Settings")]
    [SerializeField] private float baseScore = 100f;
    [SerializeField] private float maxTimeBonus = 50f;
    [SerializeField] private float timeBonusDecayRate = 2f;
    [SerializeField] private int missPenalty = 25;

    [Header("Debug")]
    public float timeSinceLastHit;

    void Update()
    {
        // Don't process input if game is over
        if (CountdownTimer.Instance != null && CountdownTimer.Instance.isGameOver)
        {
            return;
        }

        // Update time since last hit
        timeSinceLastHit += Time.deltaTime;

        // Process shooting
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        bool hitTarget = false;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Target target = hit.collider.gameObject.GetComponent<Target>();

            if (target != null)
            {
                ProcessTargetHit(target);
                hitTarget = true;
            }
        }

        if (!hitTarget)
        {
            ProcessMiss();
        }
    }

    private void ProcessTargetHit(Target target)
    {
        // Calculate time bonus that decays exponentially
        float timeBonus = maxTimeBonus * Mathf.Exp(-timeBonusDecayRate * timeSinceLastHit);
        
        // Calculate accuracy bonus based on current accuracy
        float accuracyBonus = score.scoreAccuracy;

        // Register hit and update score
        score.AddHit();
        score.AddScore(baseScore, timeBonus, accuracyBonus);

        // Trigger target hit effects
        target.Hit();

        // Reset timer
        timeSinceLastHit = 0f;
    }

    private void ProcessMiss()
    {
        // Register miss and deduct points
        score.AddMiss();
        score.DeductScore(missPenalty);
    }

    private void OnValidate()
    {
        if (timeBonusDecayRate < 0)
        {
            timeBonusDecayRate = 0;
        }
    }
}
