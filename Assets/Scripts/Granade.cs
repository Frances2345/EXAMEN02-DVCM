using Unity.VisualScripting;
using UnityEngine;

public class Granade : MonoBehaviour
{
    public float radius = 7;
    public float delay = 6;
    public GameObject ExplossionGranade;


    void Start()
    {
        Invoke("Explode", delay);
    }

    public void Explode()
    {
        if(ExplossionGranade != null)
        {
            GameObject fx = Instantiate(ExplossionGranade, transform.position, Quaternion.identity);
            Destroy(fx, 3f);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider hit in colliders)
        {
            if(hit.CompareTag("Enemy"))
            {
                Destroy(hit.gameObject); 
            }
        }
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);

        Gizmos.color = Color.yellow;
    }
}
