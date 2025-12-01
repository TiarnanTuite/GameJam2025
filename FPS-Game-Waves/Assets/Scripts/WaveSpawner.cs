using TMPro;
using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private int enemiesPerWave = 5;
    [SerializeField] private float timeBetweenSpawns = 0.5f;
    [SerializeField] private float timeBetweenWaves = 30f;

    [Header("Wave Settings")]
    [SerializeField] private int currentWave = 0;
    [SerializeField] private bool autoStartWaves = true;
    [SerializeField] private float enemyIncreasePerWave = 2f;
    [SerializeField] private bool continuousSpawning = true; // NEW: Spawn until kill target
    [SerializeField] private float continuousSpawnDelay = 3f; // Time between continuous spawns

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI waveCountText;
    [SerializeField] private TextMeshProUGUI waveInfoText;

    [Header("Floor Unlock Messages")]
    [SerializeField] private int[] floorUnlockWaves = new int[] { 2, 4, 6, 8 }; // Which waves unlock floors

    private Transform waveSpawner;
    private Transform[] spawnMarkers;
    private int enemiesAlive = 0;
    private int killsThisWave = 0; // NEW: Track kills per wave
    private int killsNeededThisWave = 0; // NEW: Kill target
    private bool isSpawning = false;
    private bool waveActive = false; // NEW: Is wave currently running

    void Start()
    {
        waveSpawner = this.gameObject.transform;

        // Get all child transforms as spawn markers
        spawnMarkers = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            spawnMarkers[i] = transform.GetChild(i);
        }

        if (spawnMarkers.Length == 0)
        {
            Debug.LogError("No spawn markers found! Add child objects to WaveSpawner.");
            return;
        }

        if (enemyPrefabs.Length == 0)
        {
            Debug.LogError("No enemy prefabs assigned! Add prefabs to the array.");
            return;
        }

        // Hide info text initially
        if (waveInfoText != null)
        {
            waveInfoText.gameObject.SetActive(false);
        }

        if (autoStartWaves)
        {
            StartCoroutine(ShowInitialMessage());
        }
    }

    IEnumerator ShowInitialMessage()
    {
        if (waveInfoText != null)
        {
            waveInfoText.gameObject.SetActive(true);
            waveInfoText.text = "ENEMIES ARE BOARDING YOUR SHIP!\nPREPARE FOR COMBAT";
            waveInfoText.color = Color.red;
            yield return new WaitForSeconds(4f);
        }

        // Start first wave countdown
        yield return StartCoroutine(ShowCountdown());
        StartWave();
    }

    IEnumerator StartNextWaveAfterDelay()
    {
        float timeUsed = 0f;

        // Show "Wave Complete" message
        if (currentWave > 0)
        {
            if (waveInfoText != null)
            {
                waveInfoText.gameObject.SetActive(true);

                // Wave complete message
                waveInfoText.text = $"WAVE {currentWave} COMPLETE!";
                waveInfoText.color = Color.green;
                yield return new WaitForSeconds(3f);
                timeUsed += 3f;

                // Check if this wave unlocked a floor
                if (System.Array.IndexOf(floorUnlockWaves, currentWave) >= 0)
                {
                    waveInfoText.text = "NEW FLOOR UNLOCKED!\nFind Better Weapons Before Next Wave";
                    waveInfoText.color = Color.cyan;
                    yield return new WaitForSeconds(4f);
                    timeUsed += 4f;
                }
                else
                {
                    // Generic message if no floor unlock
                    waveInfoText.text = "Get Ready!\nStock up on ammo and health";
                    waveInfoText.color = Color.yellow;
                    yield return new WaitForSeconds(3f);
                    timeUsed += 3f;
                }
            }
        }

        // Wait for remaining time minus countdown
        float remainingTime = timeBetweenWaves - timeUsed - 6f; // -6 for the countdown (5s + 1s)
        if (remainingTime > 0)
        {
            if (waveInfoText != null)
            {
                waveInfoText.text = "Prepare for next wave...";
                waveInfoText.color = Color.white;
            }
            yield return new WaitForSeconds(remainingTime);
        }

        // Countdown before next wave (last 5 seconds)
        yield return StartCoroutine(ShowCountdown());

        StartWave();
    }

    IEnumerator ShowCountdown()
    {
        if (waveInfoText != null)
        {
            waveInfoText.gameObject.SetActive(true);

            // Show countdown for last 5 seconds
            for (int i = 5; i > 0; i--)
            {
                waveInfoText.text = $"NEXT WAVE STARTING IN {i}...";
                waveInfoText.color = Color.yellow;
                yield return new WaitForSeconds(1f);
            }

            waveInfoText.text = "WAVE STARTING!";
            waveInfoText.color = Color.red;
            yield return new WaitForSeconds(1f);

            waveInfoText.gameObject.SetActive(false);
        }
        else
        {
            // Fallback if no UI text
            yield return new WaitForSeconds(5f);
        }
    }

    public void StartWave()
    {
        if (waveActive) return;

        currentWave++;
        waveActive = true;
        killsThisWave = 0;

        killsNeededThisWave = enemiesPerWave + Mathf.RoundToInt((currentWave - 1) * enemyIncreasePerWave);

        Debug.Log($"Starting Wave {currentWave} - Kill Target: {killsNeededThisWave}");

        if (continuousSpawning)
        {
            StartCoroutine(ContinuousSpawnWave());
        }
        else
        {
            StartCoroutine(SpawnWave(killsNeededThisWave));
        }

        // Update wave counter UI
        if (waveCountText != null)
        {
            waveCountText.text = currentWave.ToString();
        }
    }

    IEnumerator ContinuousSpawnWave()
    {
        isSpawning = true;

        // Keep spawning until kill target reached
        while (killsThisWave < killsNeededThisWave)
        {
            // Only spawn if we haven't reached the kill target yet
            // AND we don't have too many enemies at once
            if (killsThisWave < killsNeededThisWave && enemiesAlive < 10)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(continuousSpawnDelay);
            }
            else
            {
                // Just wait a bit before checking again
                yield return new WaitForSeconds(0.5f);
            }
        }

        isSpawning = false;
        Debug.Log($"Kill target reached ({killsThisWave}/{killsNeededThisWave})! Clearing remaining {enemiesAlive} enemies...");

        // Kill all remaining enemies once target is reached
        Enemy[] remainingEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in remainingEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }

        enemiesAlive = 0;
        waveActive = false;

        // Small delay before showing wave complete
        yield return new WaitForSeconds(1f);

        StartCoroutine(StartNextWaveAfterDelay());
    }

    IEnumerator SpawnWave(int enemyCount)
    {
        isSpawning = true;

        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        isSpawning = false;
    }

    void SpawnEnemy()
    {
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Vector3 spawnPosition = GetRandomPositionInBounds();
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemiesAlive++;
    }

    Vector3 GetRandomPositionInBounds()
    {
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;
        float avgY = 0f;

        foreach (Transform marker in spawnMarkers)
        {
            if (marker.position.x < minX) minX = marker.position.x;
            if (marker.position.x > maxX) maxX = marker.position.x;
            if (marker.position.z < minZ) minZ = marker.position.z;
            if (marker.position.z > maxZ) maxZ = marker.position.z;
            avgY += marker.position.y;
        }

        avgY /= spawnMarkers.Length;

        return new Vector3(
            Random.Range(minX, maxX),
            avgY,
            Random.Range(minZ, maxZ)
        );
    }

    public void OnEnemyKilled()
    {
        enemiesAlive--;
        killsThisWave++;

        Debug.Log($"Enemy killed - Wave kills: {killsThisWave}/{killsNeededThisWave}, Alive: {enemiesAlive}");

        // If using standard spawning and all enemies dead
        if (!continuousSpawning && enemiesAlive <= 0 && !isSpawning)
        {
            waveActive = false;
            StartCoroutine(StartNextWaveAfterDelay());
        }
    }

    public void StartNextWaveManual()
    {
        if (!isSpawning && enemiesAlive <= 0)
        {
            StartWave();
        }
    }

    void OnDrawGizmos()
    {
        if (transform.childCount == 0) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Gizmos.DrawWireSphere(child.position, 0.5f);
            Gizmos.DrawLine(child.position, child.position + Vector3.up * 2f);
        }
    }

    public int GetCurrentWave() => currentWave;
    public int GetEnemiesAlive() => enemiesAlive;
}