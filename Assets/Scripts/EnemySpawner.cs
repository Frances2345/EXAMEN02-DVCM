using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject EnemyPrefab;
    public float range = 10f;
    public float spawnInterval = 2f;
    public float LimitEmeny = 5f;
    public bool EnableSpawner;

    public float counter;

    void Start()
    {
        if (GetComponent<SphereCollider>() != null)
        {
            GetComponent<SphereCollider>().radius = range;
        }

    }
    void Update()
    {
        if (EnableSpawner && EnemyPrefab != null)
        {
            counter += Time.deltaTime;

            if (counter >= spawnInterval)
            {
                int EnemyCheck = GameObject.FindGameObjectsWithTag("Enemy").Length;

                if (EnemyCheck < LimitEmeny)
                {
                    SpawnEnemy();
                    counter = 0f;
                }
                else
                {
                    counter = 0f;
                }
            }
        }
    }


    public void SpawnEnemy()
    {
        Vector3 dir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        Vector3 finalPosition = transform.position + (dir * Random.Range(0, range));

        GameObject obj = Instantiate(EnemyPrefab, finalPosition, Quaternion.identity);

        EnemyIA ia = obj.GetComponent<EnemyIA>();
        if (ia != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) ia.target = player.transform;
        }


    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EnableSpawner = true;
            print("Modo Bienestar Activado");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EnableSpawner = false;
            print("Saliendo del País");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.aliceBlue;
        Gizmos.DrawWireSphere(transform.position, range);
    }

}