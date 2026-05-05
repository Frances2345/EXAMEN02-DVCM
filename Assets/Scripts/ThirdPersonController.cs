using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;


public class ThirdPersonController : MonoBehaviour
{
    [Header ("References")]
    public InputSystem_Actions inputs;
    private CharacterController controller;
    public CinemachineCamera characterCamera;
    public CinemachineCamera characterAimCamera;

    [Header ("Controller")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 3f;
    public float rotationSpeed = 200f;
    public float verticalVelocity = 0;
    public float jumpForce = 10;
    public float pushForce = 4;

    [Header ("Dash")]
    private bool IsDashing;
    public float dashForce;
    public float dashDuration = 0.2f;
    private float dashTimer;

    [Header ("Controller/Animator"), SerializeField]
    private CinemachineImpulseSource source;

    [SerializeField] private Vector2 moveInput;
    private bool isSprinting;

    [Header ("WallRun")]
    public float rayLenght;
    public float cameraTitlt = 15;
    public float maxTimeInAir;
    public bool enableWallRun;

    public bool aimMode = false;

    [Header ("FX")]
    public ParticleSystem impactParticlesPrefab; 
    public float lineDuration = 0.05f;
    public ParticleSystem walking;
    public ParticleSystem weaponShoot;
    public ParticleSystem muzzleFlash;

    public LineRenderer Rayprefab;
    public Transform WeaponShootAnchor;
    public GameObject CannonPrefab;
    public GameObject GranadePrefab;
    public Transform CannonSpawnPoint;
    public float throwForce;

    Vector3 normalDebug;
    Vector3 impactPoint;
    Vector3 crossResult;

    private void Awake()
    {
        inputs = new();
        controller = GetComponent<CharacterController>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        characterCamera.Priority = 10;
        characterAimCamera.Priority = 0;
    }
    private void OnEnable()
    {
        inputs.Enable();
        inputs.Player.SpawnTurret.performed += SpawnCannon;

        inputs.Player.ThrowGranade.started += ctx => ThrowSmt();

        inputs.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputs.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputs.Player.Jump.performed += OnJump;
        inputs.Player.Dash.performed += OnDash;

        inputs.Player.Sprint.started += ctx => isSprinting = true;
        inputs.Player.Sprint.canceled += ctx => isSprinting = false;

        inputs.Player.Aim.started += ctx =>
            {
                characterCamera.Priority = 0;
                characterAimCamera.Priority = 10;
                aimMode = true;
            };
        inputs.Player.Aim.canceled += ctx =>
        {
            characterCamera.Priority = 10;
            characterAimCamera.Priority = 0;
            aimMode = false;
        };
        inputs.Player.Attack.performed += OnAttack;

    }
    void Start()
    {

    }
    void Update()
    {
        EnableWallRun();
        OnMove();
    }

    private void SpawnCannon(InputAction.CallbackContext context)
    {
        Instantiate(CannonPrefab, CannonSpawnPoint.position, CannonSpawnPoint.rotation);
    }

    private void ThrowSmt()
    {
        GameObject granade = Instantiate(GranadePrefab, transform.position, Quaternion.identity);
        Vector3 dir = characterCamera.transform.forward;

        granade.GetComponent<Rigidbody>().AddForce(dir * throwForce, ForceMode.Impulse);

        if (granade == null) return;
    }

    public void OnMove()
    {
        Vector3 cameraForwardDir = characterCamera.transform.forward;
        cameraForwardDir.y = 0;
        cameraForwardDir.Normalize();


        if(!aimMode)
        {
            if (moveInput != Vector2.zero)
            {
                if (!walking.isPlaying) walking.Play();
                Quaternion targetQuaternion = Quaternion.LookRotation(cameraForwardDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetQuaternion, rotationSpeed * Time.deltaTime);
            }
            else
            {
                if (walking.isPlaying) walking.Stop();
            }
        }
        else
        {
            walking.Stop();
            Vector3 cameraForwardAimDir = characterCamera.transform.forward;
            cameraForwardAimDir.Normalize();
            Quaternion targetQuaternion = Quaternion.LookRotation(cameraForwardAimDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetQuaternion, rotationSpeed * Time.deltaTime);
        }

        float currentspeed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        Vector3 moveDir;
        if (!enableWallRun)
        {
            moveDir = (cameraForwardDir * moveInput.y + transform.right * moveInput.x) * currentspeed;
        }
        else
        {
            moveDir = (crossResult * moveInput.y) * currentspeed;
        }

        verticalVelocity += Physics.gravity.y * Time.deltaTime;

        if (enableWallRun)
            verticalVelocity = 0;

        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        moveDir.y = verticalVelocity;
        if (IsDashing)
        {
            moveDir = transform.forward * dashForce * (dashTimer / dashDuration);

            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0)
                IsDashing = false;
        }
        controller.Move(moveDir * Time.deltaTime);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (!controller.isGrounded) return;

        source.GenerateImpulse();
        verticalVelocity = jumpForce;
    }
    private void OnAttack(InputAction.CallbackContext context)
    {
        if (muzzleFlash != null) muzzleFlash.Play(); 
        Ray ray = new Ray(characterAimCamera.transform.position, characterAimCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 100))
        {
            LineRenderer line = Instantiate(Rayprefab);

            line.positionCount = 2;
            line.SetPosition(0, WeaponShootAnchor.position);
            line.SetPosition(1, hit.point);
           
            Destroy(line.gameObject, lineDuration);
         
            if (impactParticlesPrefab != null)
            {               
                ParticleSystem impact = Instantiate( impactParticlesPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact.gameObject, 2f);
            }
        }
        else
        {          
            LineRenderer line = Instantiate(Rayprefab);
            line.SetPosition(0, WeaponShootAnchor.position);
            line.SetPosition(1, ray.origin + ray.direction * 100);
            Destroy(line.gameObject, lineDuration);
        }
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Vector3 pushDir = (hit.transform.position - transform.position).normalized;

        if (hit.rigidbody != null && hit.rigidbody.linearVelocity == Vector3.zero)
        {
            print(hit.gameObject.name);
            hit.rigidbody.AddForce(pushDir * pushForce, ForceMode.Impulse);
        }
    }
    private void OnDash(InputAction.CallbackContext context)
    {
        IsDashing = true;
        dashTimer = dashDuration;
    }

    public void EnableWallRun()
    {
        RaycastHit hit = default;

        Physics.Raycast(transform.position, transform.right, out RaycastHit hitRight, rayLenght);

        Physics.Raycast(transform.position, -transform.right, out RaycastHit hitLeft, rayLenght);
   
        if (hitRight.collider != null && hitRight.collider.gameObject.tag == "Wall")
        {
            hit = hitRight;
            characterCamera.Lens.Dutch = cameraTitlt;
        }
        else if(hitLeft.collider != null && hitLeft.collider.gameObject.tag == "Wall")
        {
            hit = hitLeft;
            characterCamera.Lens.Dutch = -cameraTitlt;
        }
        else
        {
            characterCamera.Lens.Dutch = 0;
            enableWallRun = false;
        }

        if(hit.collider != null)
        {
            enableWallRun = true;

            normalDebug = hit.normal;
            impactPoint = hit.point;
            crossResult = Vector3.Cross(normalDebug, transform.up);//+1

            if (Vector3.Dot(crossResult, transform.forward) < 0)
            {
                crossResult *= -1;
            }
        }
    }

    public float GetSpeed()
    {
        return Mathf.Abs(controller.velocity.magnitude);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.purple;
        Gizmos.DrawRay(transform.position, transform.right * rayLenght);
        Gizmos.color = Color.navyBlue;
        Gizmos.DrawRay(transform.position, -transform.right * rayLenght);

        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(impactPoint, normalDebug * rayLenght);
        Gizmos.DrawSphere(impactPoint, 0.1f);

        Gizmos.color = Color.orange;
        Gizmos.DrawRay(impactPoint, crossResult * rayLenght);


    }
}
