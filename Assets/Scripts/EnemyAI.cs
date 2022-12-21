using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

public class EnemyAI : MonoBehaviour
{
    [Header("AI")]
    [SerializeField] public GameObject GoalObject;

    [SerializeField]
    [Range(0f, 180f)]
    private float _attackRange = 20.0f;

    [SerializeField] private List<GameObject> _patrolPoints = new List<GameObject>();

    [Header("AI POV")]
    [Header("Enemy detection")]
    [Range(0f, 180f)]
    [SerializeField] public float Radius;
    [Range(0f, 180f)]
    [SerializeField] public float Angle;


    [SerializeField] private LayerMask _targetMask;
    [SerializeField] private LayerMask _obstructionMask;

    [SerializeField] private float _delay;

    [SerializeField] public bool IsSpotted = false;

    [Header("Delay position time patrol (seconds)")]
    [SerializeField] public float MinimumDelayTime = 1f;
    [SerializeField] public float MaximumDelayTime = 3f;

    [Header("Attacking")]
    [SerializeField] public Transform FireSpawnPoint;
    [SerializeField] private float _attackDistance = 10f;

    [Header("Effects")]
    [SerializeField] public ParticleSystem DeathParticles;
    [SerializeField] public GameObject AttackParticles;

    [Header("Audio")]
    [SerializeField] public AudioSource DeathSoundEffects;
    [SerializeField] public AudioSource AlertSoundEffects;

    private int _currentPatrolPoint = 0;
    private Transform _goal;
    private NavMeshAgent _agent;

    private bool _isAttacking = false;
    private bool _isDead = false;

    private enum AIState
    {
        STANDING,
        CHASING,
        PATROLLING,
        ATTACKING,
        DECEASED
    }

    private AIState _state;

    void Start()
    {
        _goal = GoalObject.transform;
        _agent = GetComponent<NavMeshAgent>();
        StartCoroutine(FOVRoutine());
        _state = AIState.STANDING;
    }

    /*private void UpdateAIState()
    {
        float distance = Vector3.Distance(transform.position, GoalObject.transform.position);
        if (distance < _visionDistance && distance > _attackDistance)
        {
            _state = AIState.CHASING;
        }
        else if (distance < _attackDistance)
        {
            _state = AIState.ATTACKING;
        }
        else if (distance > _visionDistance * 1.5f && (_state == AIState.CHASING || _state == AIState.ATTACKING))
        {
            _state = AIState.PATROLLING;
            SetPatrolPoint();
        }
    }*/

    void Update()
    {
        /*UpdateAIState();*/
        switch (_state)
        {
            case AIState.STANDING:
                StandStill();
                break;
            case AIState.CHASING:
                ChasePlayer();
                break;
            case AIState.PATROLLING:
                Patrol();
                break;
            case AIState.ATTACKING:
                AttackPlayer();
                break;
            case AIState.DECEASED:
                _isDead = true;
                break;
            default:
                StandStill();
                break;
        }
    }

    private void ChasePlayer()
    {
        _agent.SetDestination(_goal.position.normalized);
    }

    private void StandStill()
    {
        if (_patrolPoints.Count != 0)
        {
            _state = AIState.PATROLLING;
            SetPatrolPoint();
        }
        else
        {
            RotatePatrol();
        }
    }

    private void Patrol()
    {
        if (_patrolPoints.Count == 0)
            return;

        float threshold = 5.5f;
        if (Vector3.Distance(_agent.transform.position, _patrolPoints[_currentPatrolPoint].transform.position) < threshold && !_agent.pathPending)
        {
            StartCoroutine(ObservePatrol());
            _currentPatrolPoint = (_currentPatrolPoint + 1) % _patrolPoints.Count;
        }
    }

    private void RotatePatrol()
    {
        float randomRange = Mathf.Cos(Time.time) * Mathf.Sin(Time.time) * 70 * Mathf.PingPong(-1, 1) * UnityEngine.Random.Range(1, 3);
        transform.RotateAround(transform.position, Vector3.up, randomRange * Time.deltaTime);
    }

    // Add delay on AI
    private IEnumerator ObservePatrol()
    {
        float seconds = UnityEngine.Random.Range(MinimumDelayTime, MaximumDelayTime);
        yield return new WaitForSeconds(seconds);
        SetPatrolPoint();
    }

    private IEnumerator WaitPatrol()
    {
        yield return new WaitForSeconds(3);
    }

    private void SetPatrolPoint()
    {
        _agent.destination = _patrolPoints[_currentPatrolPoint].transform.position;
    }

    private void AttackPlayer()
    {
        //_agent.SetDestination(_goal.position);
        transform.LookAt(_goal.position);

        if (!_isAttacking)
        {
            Rigidbody rb = Instantiate(AttackParticles, FireSpawnPoint.position, Quaternion.identity)
                .GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * 5f, ForceMode.Impulse);
            rb.AddForce(transform.up * 5f, ForceMode.Impulse);
            _isAttacking = true;

            //Invoke(nameof(ResetAttack), 5);
            StartCoroutine(ResetAttack(rb.gameObject));
            //ResetAttack(rb.gameObject);
        }
    }

    private IEnumerator ResetAttack(GameObject go)
    {
        yield return new WaitForSeconds(2f);
        Destroy(go);
        yield return new WaitForSeconds(0.2f);
        _isAttacking = false;
    }

    //private async void ResetAttack(GameObject go)
    //{
    //    await Task.Delay(3000);
    //    Destroy(go);
    //    await Task.Delay(2000);
    //    _isAttacking = false;
    //}

    // Enemy death effect
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            var instObject = Instantiate(DeathParticles, transform.position, Quaternion.identity);
            DeathSoundEffects.Play();
            Destroy(instObject.gameObject, 5f);
            _state = AIState.DECEASED;
            Debug.Log(transform.name + " died");
        }
    }

    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);
        while (!_isDead)
        {
            yield return wait;
            FieldOfView();
        }
    }

    private void FieldOfView()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, Radius, _targetMask);

        // Check if layer is in range
        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            // Angle of enemy view 
            if (Vector3.Angle(transform.forward, directionToTarget) < Angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                // In distance of enemy view
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, _obstructionMask))
                {
                    if (!IsSpotted)
                        AlertSoundEffects.Play();

                    IsSpotted = true;

                    if (distanceToTarget < _attackRange)
                        _state = AIState.ATTACKING;
                    else
                        _state = AIState.CHASING;
                }
                else
                {
                    IsSpotted = false;
                    // Player got lost
                    _state = AIState.STANDING;
                }
            }
        }
        else
        {
            // Do nothing as the state will have clause with other call
            IsSpotted = false;
            WaitPatrol();
            _state = AIState.STANDING;
        }
    }
}