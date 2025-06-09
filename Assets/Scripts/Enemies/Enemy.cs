using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

enum EnemyType
{
    Normal,
    Stun
}
 public class Enemy : MonoBehaviour, IEnemy, IDamagable
 {
        [Header("Enemy Type")]
        [SerializeField] private EnemyType enemyType = EnemyType.Normal;

        [Header("Stats")]
        [SerializeField] private float health = 100;
        [SerializeField] private float Maxhealth = 100;
        [SerializeField] public float moveSpeed = 3.5f;
        [SerializeField] public float attackDamage = 15f;
        [SerializeField] public float detectionRange = 8f;
        [SerializeField] public float attackRange = 1.5f;
        [SerializeField] private float attackCooldown = 2f;

        [Header("Scream Settings")]
        [SerializeField] private float StunDuration = 5f;
        [SerializeField] private float ScreamCooldown = 20f;

        [Header("References")]
        public GameObject player;
        public NavMeshAgent navMeshAgent;
        public Animator animator;

        [Header("IEnemy Linking")]
        public float Health { get => health; set => health = value; }
        public float MaxHealth { get => Maxhealth; set => Maxhealth = value; }
        public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
        public new Transform transform => base.transform;
        public float DetectionRange { get => detectionRange; set => detectionRange = value; }
        public List<Transform> Waypoints { get => waypoints; set => waypoints = value; }
        public float WaypointStopTime { get => waypointStopTime; set => waypointStopTime = value; }
        public int CurrentWaypointIndex { get => _currentWaypointIndex; set => _currentWaypointIndex = value; }
        public GameObject Player => player;
        public NavMeshAgent NavMeshAgent => navMeshAgent;
        public Animator Animator => animator;
        public bool IsDead => health <= 0;
        public float AttackRange { get => attackRange; set => attackRange = value; }
        public float AttackCooldown { get => attackCooldown; set => attackCooldown = value; }

        public Vector3 LastKnownPlayerPosition
        {
            get => _lastKnownPlayerPosition;
            set => _lastKnownPlayerPosition = value;
        }

        [Header("Patrol Settings")]
        public List<Transform> waypoints;
        public float waypointStopTime = 2f;
        public int _currentWaypointIndex = 0;

        // Private Vars
        private float _lastAttackTime;
        private float _lastScreamTime;
        private bool _hasScreamed;
        private IEnemyStates _currentState;
        [SerializeField] private DetectionChecker _detectionChecker;
        [SerializeField] private StartAttackChecker _attackChecker;
        [SerializeField] public ScreamAreaEffect StunArea;
        private bool _playerInDetectionRange;
        private bool _playerInAttackRange;
        private Vector3 _lastKnownPlayerPosition;

        private void Awake()
        {
            _detectionChecker = GetComponentInChildren<DetectionChecker>();
            _attackChecker = GetComponentInChildren<StartAttackChecker>();
            StunArea = GetComponentInChildren<ScreamAreaEffect>();

            if (_detectionChecker != null)
                _detectionChecker.Initialize(detectionRange, this);

            if (_attackChecker != null)
                _attackChecker.Initialize(attackRange, this);

            if (enemyType == EnemyType.Stun && StunArea != null)
            {
                StunArea.stunDuration = StunDuration;
                StunArea.radius = detectionRange;
            }
        }

        private void Start()
        {
            navMeshAgent.speed = moveSpeed;
            animator = GetComponent<Animator>();
            ChangeState(new IdleState());
            health = Maxhealth;
            _lastScreamTime = -ScreamCooldown;
            _hasScreamed = false;
        }

        void Update()
        {
            if (!IsDead)
            {
                // Handle scream logic for stun enemies
                if (enemyType == EnemyType.Stun)
                {
                    if (!_hasScreamed && IsPlayerInDetectionRange && CanScream()&&HasLineOfSight())
                    {
                        ChangeState(new PrepScreamState());
                        _hasScreamed = true;
                        _lastScreamTime = Time.time;
                        return;
                    }

                    if (_hasScreamed && Time.time > _lastScreamTime + ScreamCooldown)
                    {
                        _hasScreamed = false;
                    }
                }

                _currentState?.UpdateState(this);
            }
        }

        public bool IsPlayerInDetectionRange => _playerInDetectionRange;
        public bool IsPlayerInAttackRange => _playerInAttackRange;

        public void SetPlayerInDetectionRange(bool inRange) => _playerInDetectionRange = inRange;
        public void SetPlayerInAttackRange(bool inRange) => _playerInAttackRange = inRange;

        public void ChangeState(IEnemyStates newState)
        {
            _currentState?.ExitState(this);
            _currentState = newState;
            _currentState?.EnterState(this);
        }

        public bool HasLineOfSight()
        {
            if (Player == null) return false;

            Vector3 direction = (Player.transform.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, Player.transform.position);

            if (Physics.Raycast(transform.position + Vector3.up * 1.5f, direction, out RaycastHit hit, distance))
            {
            if (hit.collider.CompareTag("Player"))
            {
                _lastKnownPlayerPosition = Player.transform.position;
                return true;
                }
            }
            return false;
        }

        public bool CanAttack() => Time.time > _lastAttackTime + attackCooldown;
        public bool CanScream() => Time.time > _lastScreamTime + ScreamCooldown;
        public void StartScreamAfterDelay(float delay)
        {
            StartCoroutine(ScreamDelayCoroutine(delay));
        }

        private IEnumerator ScreamDelayCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (!IsDead) 
            {
                ChangeState(new ScreamState());
            }
        }

        public void TakeDamage(float damage)
        {
            health -= damage;
            if (IsDead) Die();
        }

        public float UpdateHealth(float heal, float damage)
        {
            health += heal;
            return health;
        }

        public void DealDamage() { }

        public void Die()
        {
            ChangeState(new DeathState());
        }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0.92f, 0.016f, 0.25f);
        Gizmos.DrawSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = new Color(1, 0, 0, 0.1f);
        Gizmos.DrawSphere(transform.position, attackRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (Player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, Player.transform.position);
        }

        if (waypoints != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] != null)
                    Gizmos.DrawSphere(waypoints[i].position, 0.3f);
            }
        }

        //Vector3 dir = (Player.transform.position - transform.position).normalized;
        //Gizmos.color = Color.red;
        //Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, dir * detectionRange);
    }
#endif

}
