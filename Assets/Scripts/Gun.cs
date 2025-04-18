using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// gun base class
public class Gun : MonoBehaviour
{
    protected FPSController player;

    // references
    [SerializeField] protected Transform gunBarrelEnd;
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected Animator anim;

    // stats
    [SerializeField] protected int maxAmmo;
    [SerializeField] protected float timeBetweenShots = 0.1f;
    [SerializeField] protected bool isAutomatic = false;

    // private variables
    protected int ammo;
    protected float elapsed = 0;

    [SerializeField] private InputActionAsset inputActions;
    private InputAction reloadAction;

    // Start is called before the first frame update
    void Start()
    {
        ammo = maxAmmo;

        if (inputActions != null)
        {
            var playerMap = inputActions.FindActionMap("Player");
            reloadAction = playerMap.FindAction("Reload");
            if (reloadAction != null)
                reloadAction.Enable();
        }
    }

    // Update is called once per frame
    void Update()
    {
        elapsed += Time.deltaTime;

        // cheat code to refill ammo
        if (reloadAction != null && reloadAction.WasPressedThisFrame())
        {
            AddAmmo(999);
        }
    }

    public virtual void Equip(FPSController p)
    {
        player = p;
    }

    public virtual void Unequip() { }

    public bool AttemptAutomaticFire()
    {
        if (!isAutomatic)
            return false;

        return true;
    }

    public virtual bool AttemptFire()
    {
        if (ammo <= 0)
        {
            return false;
        }

        if(elapsed < timeBetweenShots)
        {
            return false;
        }

        return true;
    }

    public virtual bool AttemptAltFire()
    {
        return false;
    }

    public virtual void AddAmmo(int amount)
    {
        ammo += amount;

        if (ammo > maxAmmo)
            ammo = maxAmmo;
    }
}
