using UnityEngine;

public class Turret : MonoBehaviour
{
    public float range = 10;
    public float rotationSpeed = 5;


    public GameObject bulletPrefab;
    public Transform FIREPOINT;
    public float fireRate = 4;
    public float fireCooldown = 5;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
