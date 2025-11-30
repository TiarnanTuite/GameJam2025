using TMPro;
using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private int enemiesPerWave = 5;
    [SerializeField] private float timeBetweenSpawns = 0.5f;
    [SerializeField] private float timeBetweenWaves = 5f;
    
    [Header("Wave Settings")]
    [SerializeField] private int currentWave = 0;
    [SerializeField] private bool autoStartWaves = true;
    [SerializeField] private float enemyIncreasePerWave = 2f;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI waveCountText;

    private Transform waveSpawner;
    private Transform[] spawnMarkers;
    private int enemiesAlive = 0;
    private bool isSpawning = false;
    
    void Start()
    {
        // Get wave spawner
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
        
        if (autoStartWaves)
        {
            StartCoroutine(StartNextWaveAfterDelay());
        }
    }
    
    IEnumerator StartNextWaveAfterDelay()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        StartWave();
    }
    
    public void StartWave()
    {
        if (isSpawning) return;
        
        currentWave++;
        
        // Lower the spawner after wave 1
        if (currentWave > 1)
        {
            waveSpawner.position = new Vector3(
                waveSpawner.position.x, 
                waveSpawner.position.y - 5, 
                waveSpawner.position.z
            );
        }
        
        int enemiesToSpawn = enemiesPerWave + Mathf.RoundToInt((currentWave - 1) * enemyIncreasePerWave);
        
        Debug.Log($"Starting Wave {currentWave} with {enemiesToSpawn} enemies");
        StartCoroutine(SpawnWave(enemiesToSpawn));

        // Update UI
        if (waveCountText != null)
        {
            waveCountText.text = currentWave.ToString();
        }
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
        // Pick random enemy prefab
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        
        // Get random position within the bounds of all markers
        Vector3 spawnPosition = GetRandomPositionInBounds();
        
        // Spawn enemy at random position
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemiesAlive++;
    }
    
    Vector3 GetRandomPositionInBounds()
    {
        // Find min and max bounds of all markers
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
        
        // Return random position within bounds
        return new Vector3(
            Random.Range(minX, maxX),
            avgY,
            Random.Range(minZ, maxZ)
        );
    }
    
    public void OnEnemyKilled()
    {
        enemiesAlive--;
        
        if (enemiesAlive <= 0 && !isSpawning)
        {
            // All enemies defeated, start next wave
            StartCoroutine(StartNextWaveAfterDelay());
        }
    }
    
    // Manual wave start (call from UI button)
    public void StartNextWaveManual()
    {
        if (!isSpawning && enemiesAlive <= 0)
        {
            StartWave();
        }
    }
    
    // Visualize spawn points in editor
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
}