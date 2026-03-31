using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{
    // Status Enum -> Status Property & switch문 패턴 -> Update 내에서 switch문 패턴 => 상태에 따른 행동 패턴을 쉽게 관리할 수 있도록 하는 구조
    public enum Status
    {
        Idle,
        Trace,
        Attack,
        Die,
    }

    public Transform target;
    public HitBox hitBox;
    public LayerMask targetLayer;

    private NavMeshAgent agent;
    private Animator zombieAnimator;

    public float traceDistance = 10f;
    public float attackDistance = 1f;
    private float attackInterval = 0.05f;
    private float lastAttackTime;

    private Status currentStatus;

    public Status CurrentStatus 
    { 
        get { return currentStatus; }
        set
        {
            var prevStatus = currentStatus;
            currentStatus = value;

            Debug.Log($"{currentStatus}, {prevStatus}");    

            // 상태가 변경된 경우에 애니메이션을 업데이트
            switch (currentStatus)
            {
                case Status.Idle:
                    zombieAnimator.SetBool("HasTarget", false);
                    agent.isStopped = true; 
                    break;
                case Status.Trace:
                    zombieAnimator.SetBool("HasTarget", true);
                    agent.isStopped = false;
                    break;
                case Status.Attack:
                    zombieAnimator.SetBool("HasTarget", false);
                    agent.isStopped = true;
                    break;
                case Status.Die:
                    agent.isStopped = true;
                    break;
            }
        }
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        zombieAnimator = GetComponent<Animator>();

        zombieAnimator.SetBool("HasTarget", true);
    }

    private void Start()
    {
        CurrentStatus = Status.Idle;
    }
    private void Update()
    {
        switch (currentStatus)
        {
            case Status.Idle:
                UpdateIdle();
                break;
            case Status.Trace:
                UpdateTrace();
                break;
            case Status.Attack:
                UpdateAttck();
                break;
            case Status.Die:
                UpdateDie();
                break;
        }
    }

    private void UpdateDie()
    {

    }

    private void UpdateAttck()
    {
        if (target == null)
        {
            CurrentStatus = Status.Trace;
            return;
        }

        var find = hitBox.Colliders.Find(x => x.transform == target);

        if (find == null)
        {
            CurrentStatus = Status.Trace;
            return;
        }

        // 타겟을 계속 바라볼 수 있게 방향 조정
        var lookAt = target.position;
        lookAt.y = transform.position.y;
        transform.LookAt(lookAt);
        
        if (Time.time > lastAttackTime + attackInterval)
        {
            Debug.Log("Zombie Attacked the Player");
            lastAttackTime = Time.time;

            var damageable = target.GetComponent<IDamageable>();    
            if (damageable != null )
            {
                damageable.OnDamage(10f, transform.position, -transform.forward);  
            }
        }
    }

    private void UpdateTrace()
    {
        if (target != null)
        {
            var find = hitBox.Colliders.Find(x => x.transform == target);

            if (find != null)
            {
                CurrentStatus = Status.Attack;
                return;
            }
        }

        if (target == null || Vector3.Distance(target.position, transform.position) > traceDistance)
        {
           
            target = null;
            CurrentStatus = Status.Idle;
            return;
        }

        agent.SetDestination(target.position);

    }

    private void UpdateIdle()
    {
        if (target != null && Vector3.Distance(target.position, transform.position) < traceDistance)
        {
            CurrentStatus = Status.Trace;
            return;
        }

        target = FindTarget(traceDistance);
    }

    private Transform FindTarget(float radius)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, targetLayer); // 구의 중점, 구의 반경, 타겟 레이어
                                                                                               // 특정한 레이어들을 넘겨 받음
        if (colliders.Length == 0) return null;

        var target = colliders.OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).First(); // 제일 앞에 있는 콜라이더 반환

        return target.transform;
    }
}
