using UnityEngine;

public class Turret : MonoBehaviour
{
    public float range = 10;
    public float rotationSpeed = 5;
    private float nextFireTime;


    public GameObject bulletEffect;
    public Transform shootPoint;

    public float fireRate = 4;
    public float fireCooldown = 5;

    private void Start()
    {
        Destroy(gameObject, 15f);
    }


    void Update()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance <= range)
            {
                transform.LookAt(enemy.transform);

                if (Time.time >= nextFireTime)
                {
                    Shoot(enemy);
                    nextFireTime = Time.time + fireRate;
                }
                break;

            }
        }
    }

    void Shoot(GameObject target)
    {
        if (bulletEffect != null && shootPoint != null)
        {
            GameObject effect = Instantiate(bulletEffect, shootPoint.position, shootPoint.rotation);
            Destroy(effect, 0.5f);
        }
        Destroy(target);
    }
}
