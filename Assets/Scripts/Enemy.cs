using UnityEngine;
using UnityEngine.AI;

public class EnemyIA : MonoBehaviour
{
    public Transform target;
    private NavMeshAgent agent;
    public GameObject ExplosionEffect;
    public float range = 4f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (GetComponent<SphereCollider>() != null)
        {
            GetComponent<SphereCollider>().radius = range;
        }
    }

    public void ExplodeEnemy()
    {
        if (ExplosionEffect != null)
        {
            Instantiate(ExplosionEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Mencho Murio");
            Destroy(gameObject);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (target != null)
            {
                agent.destination = target.position;
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {

        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);

        Gizmos.color = Color.yellow;
    }
}