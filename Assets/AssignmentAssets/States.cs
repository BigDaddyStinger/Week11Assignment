using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MyStates
{
    WANDER,
    PURSUE,
    ATTACK,
    RECOVERY
}

public class States : MonoBehaviour
{

    [SerializeField] MyStates myStates;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (myStates)
        {
            case MyStates.WANDER:
                UpdateWander();
                break;
            case MyStates.PURSUE:
                UpdatePursue();
                break;
            case MyStates.ATTACK:
                UpdateAttack();
                break;
            case MyStates.RECOVERY:
                UpdateRecovery();
                break;
        }
    }

    void UpdateWander()
    {
        Debug.Log("I'm wandering");
    }

    void UpdatePursue()
    {
        Debug.Log("I'm pursuing");
    }

    void UpdateAttack()
    {
        Debug.Log("I'm attacking");
    }

    void UpdateRecovery()
    {
        Debug.Log("I'm recovering");
    }

    void OnMovement()
    {
        Debug.Log("I'm Moving");
    }



}
