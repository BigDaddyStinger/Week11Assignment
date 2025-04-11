using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    // Movement / Navigation
    public Rigidbody Rigidbody { get; private set; }
    private NavMeshAgent agent;

    // State Machine Variables
    public float wanderRange = 10f;        
    public float playerSightRange = 15f;   
    public float playerAttackRange = 2f;    
    public float recoveryTime = 2f;        

    private Vector3 startingLocation;
    private float currentStateElapsed = 0f;

    private Transform player;


    public enum EnemyState 
        { 
        WANDER, 
        PURSUE, 
        ATTACK, 
        RECOVERY
        }
    public EnemyState currentState = EnemyState.WANDER;

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();

        startingLocation = transform.position;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj)
            player = playerObj.transform;

        if (!agent)
            agent = gameObject.AddComponent<NavMeshAgent>();

        SwitchState(EnemyState.WANDER);
    }

    void Update()
    {
        currentStateElapsed += Time.deltaTime;

        switch (currentState)
        {
            case EnemyState.WANDER:
                WanderUpdate();
                break;
            case EnemyState.PURSUE:
                PursueUpdate();
                break;
            case EnemyState.ATTACK:
                AttackUpdate();
                break;
            case EnemyState.RECOVERY:
                RecoveryUpdate();
                break;
        }
    }

    void WanderUpdate()
    {
        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            Vector3 randomDirection = new Vector3(
                Random.Range(-wanderRange, wanderRange),
                0f,
                Random.Range(-wanderRange, wanderRange)
            );
            Vector3 wanderPos = startingLocation + randomDirection;
            agent.SetDestination(wanderPos);
        }

        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= playerSightRange)
            {
                SwitchState(EnemyState.PURSUE);
            }
        }
    }

    void PursueUpdate()
    {
        if (!player) return;

        agent.SetDestination(player.position);

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= playerAttackRange)
        {
            SwitchState(EnemyState.ATTACK);
        }

        else if (dist > playerSightRange)
        {
            SwitchState(EnemyState.WANDER);
        }
    }

    void AttackUpdate()
    {
        agent.isStopped = true;

        if (Rigidbody)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            Rigidbody.AddForce(direction * 5f, ForceMode.Impulse);
        }

        SwitchState(EnemyState.RECOVERY);
    }

    void RecoveryUpdate()
    {
        if (currentStateElapsed >= recoveryTime)
        {
            agent.isStopped = false;
            SwitchState(EnemyState.PURSUE);
        }
    }

    void SwitchState(EnemyState newState)
    {
        currentState = newState;
        currentStateElapsed = 0f;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (currentState == EnemyState.ATTACK)
        {
            var playerObj = collision.gameObject.GetComponent<FPSController>();
            if (playerObj != null)
            {
                Debug.Log("Player was hit by the enemy!");
                SwitchState(EnemyState.RECOVERY);
            }
        }
    }

    public void Respawn()
    {
        transform.position = startingLocation;
        if (agent)
            agent.Warp(startingLocation); 
    }
}