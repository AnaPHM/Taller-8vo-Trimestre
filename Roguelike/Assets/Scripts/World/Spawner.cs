using UnityEngine;

public class Spawner : MonoBehaviour
{
    public bool shooter;
    private int counter = 0;
    private int spanwedEnemies = 0;

    [SerializeField]
    private int maxSpawnedEnemies = 5;

    [SerializeField]
    private GameObject fighterPrefab;
    [SerializeField]
    private GameObject shooterPrefab;

    private void OnEnable()
    {
        if (!shooter)
        RobotLuchador.OnEnemyDeath += DecreaseCounter;
        else
        RobotPistolero.OnEnemyDeath += DecreaseCounter;
    }

    private void OnDisable()
    {
        if (!shooter)
        RobotLuchador.OnEnemyDeath -= DecreaseCounter;
        else
        RobotPistolero.OnEnemyDeath -= DecreaseCounter;
    }

    void Start()
    {
        SpawnEnemy();
    }

    void Update()
    {
        if (spanwedEnemies >= maxSpawnedEnemies) this.gameObject.SetActive(false);

        if (counter != 0) return;
        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        if (shooter)
            Instantiate(shooterPrefab, transform.position, Quaternion.identity);
        else
            Instantiate(fighterPrefab, transform.position, Quaternion.identity);

        counter++;
    }

    private void DecreaseCounter()
    {
        counter--;
        spanwedEnemies++;
    }
}
