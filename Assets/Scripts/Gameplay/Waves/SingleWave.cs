using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWave", menuName = "EnemyWave", order = 1)]
public class SingleWave : EnemyWave {
    [Header("Wave settings")]
    public int spawn;
    public int count;
    public float period;
    public float periodRand;
    
    [Header("Individual settings")]
    public Enemy enemyPrefab;
    public float health;
    public float speed = 3f;

    public IEnumerator SpawnDelayed(float delay, Level level) {
        yield return new WaitForSeconds(delay);
        yield return Spawn(level);
    }

    public override IEnumerator Spawn(Level level) {
        WaitForSeconds wait = new WaitForSeconds(this.period + Random.value * this.periodRand - this.periodRand * 0.5f);

        for(int i = 0; i < this.count; i++) {
            yield return wait;
            SpawnEnemy(level.enemySpawns[this.spawn]);
        }
        
        yield return null;
    }

    private void SpawnEnemy(Transform spawnPoint) {
        Enemy enemy = Pool.instance.Grab<Enemy>(this.enemyPrefab);
        // enemy.startHealth = this.health;
        // enemy.moveSpeed = this.speed;
        enemy.transform.position = spawnPoint.transform.position;
        enemy.Spawn(spawnPoint.transform.position);
    }
}