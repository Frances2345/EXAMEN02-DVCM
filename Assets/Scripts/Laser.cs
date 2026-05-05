using UnityEngine;

public class Laser : MonoBehaviour
{
    public float radiuslaser = 7;
    public GameObject ExplossionGranade;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("C murio :v");
            Destroy(gameObject);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, radiuslaser);

        foreach (Collider hit in colliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                Destroy(hit.gameObject);
            }
        }
    }
}
