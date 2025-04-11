using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class FPSController : MonoBehaviour
{
    // references
    CharacterController controller;
    [SerializeField] GameObject cam;
    [SerializeField] Transform gunHold;
    [SerializeField] Gun initialGun;

    // stats
    [SerializeField] float movementSpeed = 2.0f;
    [SerializeField] float lookSensitivityX = 1.0f;
    [SerializeField] float lookSensitivityY = 1.0f;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float jumpForce = 10;

    // INPUT SYSTEM
    [SerializeField] InputActionAsset inputActions;

    public InputAction moveAction;
    public InputAction lookAction;
    public InputAction jumpAction;
    public InputAction sprintAction;
    public InputAction fireAction;
    public InputAction altFireAction;
    public InputAction scrollAction;
    public InputAction reloadAction;

    // private variables
    Vector3 origin;
    Vector3 velocity;
    bool grounded;
    float xRotation;
    List<Gun> equippedGuns = new List<Gun>();
    int gunIndex = 0;
    Gun currentGun = null;

    // properties
    public GameObject Cam { get { return cam; } }

    void OnEnable()
    {
        // Find "Player" action map
        var playerMap = inputActions.FindActionMap("Player");

        // get references to actions
        moveAction = playerMap.FindAction("Move");
        lookAction = playerMap.FindAction("Look");
        jumpAction = playerMap.FindAction("Jump");
        sprintAction = playerMap.FindAction("Sprint");
        fireAction = playerMap.FindAction("Fire");
        altFireAction = playerMap.FindAction("AltFire");
        scrollAction = playerMap.FindAction("Scroll");
        reloadAction = playerMap.FindAction("Reload");

        // enable them
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
        fireAction.Enable();
        altFireAction.Enable();
        scrollAction.Enable();
        reloadAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
        fireAction.Disable();
        altFireAction.Disable();
        scrollAction.Disable();
        reloadAction.Disable();
    }


    void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        // spawn with an initial gun
        if (initialGun != null)
            AddGun(initialGun);

        origin = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        Look();
        HandleSwitchGun();
        FireGun();

        // gradually reset horizontal velocity
        Vector3 noVelocity = new Vector3(0, velocity.y, 0);
        velocity = Vector3.Lerp(velocity, noVelocity, 5 * Time.deltaTime);
    }

    void Movement()
    {
        grounded = controller.isGrounded;

        if (grounded && velocity.y < 0)
        {
            velocity.y = -1f;
        }

        // read movement input
        Vector2 movement = moveAction.ReadValue<Vector2>();
        Vector3 move = transform.right * movement.x + transform.forward * movement.y;

        // sprint?
        bool isSprinting = sprintAction.ReadValue<float>() > 0.5f;
        controller.Move(move * movementSpeed * (isSprinting ? 2 : 1) * Time.deltaTime);

        // jump
        if (jumpAction.WasPressedThisFrame() && grounded)
        {
            velocity.y += Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        // gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void Look()
    {
        Vector2 looking = lookAction.ReadValue<Vector2>();
        float lookX = looking.x * lookSensitivityX * Time.deltaTime;
        float lookY = looking.y * lookSensitivityY * Time.deltaTime;

        xRotation -= lookY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * lookX);
    }

    void HandleSwitchGun()
    {
        if (equippedGuns.Count == 0)
            return;

        // read the scroll input (Vector2). Typically y= +1 or -1 for mouse wheel
        Vector2 scrollValue = scrollAction.ReadValue<Vector2>();
        float scrollY = scrollValue.y;

        if (scrollY > 0f)
        {
            gunIndex++;
            if (gunIndex >= equippedGuns.Count)
                gunIndex = 0;
            EquipGun(equippedGuns[gunIndex]);
        }
        else if (scrollY < 0f)
        {
            gunIndex--;
            if (gunIndex < 0)
                gunIndex = equippedGuns.Count - 1;
            EquipGun(equippedGuns[gunIndex]);
        }
    }

    void FireGun()
    {
        if (currentGun == null) return;

        // Pressed the fire button
        if (fireAction.WasPressedThisFrame())
        {
            currentGun?.AttemptFire();
        }
        // Held the fire button (for automatic)
        else if (fireAction.IsPressed())
        {
            if (currentGun.AttemptAutomaticFire())
            {
                currentGun?.AttemptFire();
            }
        }

        // alt fire
        if (altFireAction.WasPressedThisFrame())
        {
            currentGun?.AttemptAltFire();
        }
    }

    void EquipGun(Gun g)
    {
        // disable old gun
        currentGun?.Unequip();
        currentGun?.gameObject.SetActive(false);

        // enable new gun
        g.gameObject.SetActive(true);
        g.transform.parent = gunHold;
        g.transform.localPosition = Vector3.zero;
        currentGun = g;

        g.Equip(this);
    }

    // public methods

    public void AddGun(Gun g)
    {
        // add new gun to the list
        equippedGuns.Add(g);

        // our index is the last one/new one
        gunIndex = equippedGuns.Count - 1;

        // put gun in the right place
        EquipGun(g);
    }

    public void IncreaseAmmo(int amount)
    {
        currentGun.AddAmmo(amount);
    }

    public void Respawn()
    {
        transform.position = origin;
    }

    // Input methods

    /* 
     * 
     * bool GetPressFire()
    {
        return Input.GetButtonDown("Fire1");
    }

    bool GetHoldFire()
    {
        return Input.GetButton("Fire1");
    }

    bool GetPressAltFire()
    {
        return Input.GetButtonDown("Fire2");
    }

    Vector2 GetPlayerMovementVector()
    {
        return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    Vector2 GetPlayerLook()
    {
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    bool GetSprint()
    {
        return Input.GetButton("Sprint");
    }


    */

    // Collision methods

    // Character Controller can't use OnCollisionEnter :D thanks Unity
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.GetComponent<Damager>())
        {
            var collisionPoint = hit.collider.ClosestPoint(transform.position);
            var knockbackAngle = (transform.position - collisionPoint).normalized;
            velocity = (20 * knockbackAngle);
        }

        if (hit.gameObject.GetComponent <KillZone>())
        {
            Respawn();
        }
    }


}
