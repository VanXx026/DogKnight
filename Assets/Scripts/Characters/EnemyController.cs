using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//声明枚举敌人状态 守卫、巡逻、追击、死亡
public enum EnemyStates { GUARD, PATROL, CHASE, DEAD };

//自动为敌人添加NavMeshAgent组件
[RequireComponent(typeof(NavMeshAgent))]
//自动为敌人添加CharacterStates组件
// [RequireComponent(typeof(CharacterStates))]

/*具体观察者类，实现了内含观察者状态改变方法的接口
//在主题状态变化后执行EndNotify方法
//在OnEnable和Disable中反向在主题类中注册自己*/
public class EnemyController : MonoBehaviour, IEndGameObserver
{
    //定义敌人状态枚举
    private EnemyStates enemyStates;
    private NavMeshAgent agent;
    private Animator anim;
    private Collider coll;
    protected CharacterStates characterStates;

    //在视察器中添加子标题
    [Header("Basic Settings")]
    public float sightRadius;
    public bool isGuard; //是否是站桩敌人
    protected GameObject attackTarget;
    private float speed;
    public float lookAtTime; //敌人在巡逻时停下来瞅的时间\
    private float lastAttackTime; //攻击cd
    private float remainLookAtTime;


    [Header("Patrol Settings")]
    public float patrolRange;
    private Vector3 wayPoint; //巡逻位点
    private Vector3 guardPos;
    private Quaternion guardRotation;

    //bool配合动画
    private bool isWalk;
    private bool isChase;
    private bool isFollow;
    private bool isDead;

    //玩家已经死了吗
    private bool playerDead;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStates = GetComponent<CharacterStates>();
        coll = GetComponent<Collider>();

        speed = agent.speed;
        guardPos = transform.position;
        guardRotation = transform.rotation;
        remainLookAtTime = lookAtTime;
    }

    private void Start()
    {
        //如果是站桩敌人
        if (isGuard)
        {
            enemyStates = EnemyStates.GUARD;
        }
        else
        {
            enemyStates = EnemyStates.PATROL;
            GetNewWayPoint();
        }
        // //FIXME:场景切换后修改掉
        // GameManager.Instance.AddObserver(this);
    }
    
    //切换场景时启用
    // void OnEnable()
    // {
    //     GameManager.Instance.AddObserver(this);
    // }

    void OnDisable()
    {
        if(!GameManager.IsInitialized) return;
        GameManager.Instance.RemoveObserver(this);
    }

    private void Update()
    {
        if (characterStates.characterData.currentHealth == 0)
        {
            isDead = true;
        }

        if (!playerDead)
        {
            switchStates();
            switchAnimation();
            //FIXME:可以优化，怪物多了的话开销比较大，先这样跟着教程写先
            lastAttackTime -= Time.deltaTime; //攻击cd计时器
        }

    }

    //切换动画设置
    void switchAnimation()
    {
        anim.SetBool("Walk", isWalk);
        anim.SetBool("Chase", isChase);
        anim.SetBool("Follow", isFollow);
        anim.SetBool("Critical", characterStates.isCritical);
        anim.SetBool("Death", isDead);
    }

    //切换敌人状态
    void switchStates()
    {
        if (isDead)
        {
            enemyStates = EnemyStates.DEAD; //如果已经死了 切换到DEAD
        }
        else if (FoundPlayer())
        {
            enemyStates = EnemyStates.CHASE; //如果发现player 切换到CHASE
        }

        //使用switch来控制敌人状态的切换
        switch (enemyStates)
        {
            //GUARD状态有两种进入方式：1.本身就处于GUARD 2.拉脱后回到GUARD
            case EnemyStates.GUARD:
                isChase = false;

                if (transform.position != guardPos)
                {
                    isWalk = true;
                    agent.isStopped = false;
                    agent.destination = guardPos;

                    if (Vector3.Distance(transform.position, guardPos) <= agent.stoppingDistance)
                    {
                        isWalk = false;
                        transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, 0.03f);
                    }
                }

                break;
            //    
            case EnemyStates.PATROL:
                isChase = false;    //在巡逻的时候自然就没有追击状态了
                agent.speed = speed * 0.5f;

                //判断当前敌人位置和巡逻点位的距离小于agent中定义的停止距离
                if (Vector3.Distance(transform.position, wayPoint) <= agent.stoppingDistance)
                {
                    //停下来侦察一下
                    isWalk = false;
                    //如果当前巡逻停下观察周围的时间未清零
                    if (remainLookAtTime > 0)
                    {
                        remainLookAtTime -= Time.deltaTime;
                    }
                    else
                    {
                        GetNewWayPoint();
                    }

                }
                else
                {
                    isWalk = true;
                    agent.destination = wayPoint;
                }
                break;

            case EnemyStates.CHASE:
                isWalk = false;
                isChase = true;
                agent.speed = speed;

                if (FoundPlayer())
                {
                    //:如果发现player，追击
                    isFollow = true;
                    agent.isStopped = false;
                    agent.destination = attackTarget.transform.position;
                }
                else
                {
                    //:拉脱则返回到原来位置
                    isFollow = false;
                    if (remainLookAtTime > 0)
                    {
                        //脱战后立刻停下
                        agent.destination = transform.position;
                        remainLookAtTime -= Time.deltaTime;
                    }
                    //当remainLookAtTime<=0后，才变换状态
                    else if (isGuard)
                    {
                        enemyStates = EnemyStates.GUARD;
                    }
                    else
                    {
                        enemyStates = EnemyStates.PATROL;
                    }
                }

                //在攻击范围内则攻击
                if (isTargetInAttackRange() || isTargetInSkillRange())
                {
                    isFollow = false;
                    agent.isStopped = true;
                    //重置攻击cd
                    if (lastAttackTime < 0)
                    {
                        lastAttackTime = characterStates.attackData.coolDown;

                        //暴击判定
                        characterStates.isCritical = Random.value < characterStates.attackData.criticalChance;
                        //执行攻击
                        Attack();
                    }
                }

                break;

            case EnemyStates.DEAD:
                //FIXME:后续会修改这种写法
                coll.enabled = false;
                // agent.enabled = false; //如果用这种写法动画机StopAgent在敌人死亡的时候判断agent会判空报错
                agent.radius = 0;
                Destroy(gameObject, 2f);
                break;
        }
    }

    //目标是否在近战攻击范围内
    bool isTargetInAttackRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(transform.position, attackTarget.transform.position) <= characterStates.attackData.attackRange;
        else
            return false;
    }

    //目标是否在远程攻击范围内
    bool isTargetInSkillRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(transform.position, attackTarget.transform.position) <= characterStates.attackData.skillRange;
        else
            return false;
    }

    //执行攻击
    void Attack()
    {
        transform.LookAt(attackTarget.transform);
        if (isTargetInAttackRange())
        {
            //近身攻击动画
            anim.SetTrigger("Attack");
        }
        if (isTargetInSkillRange())
        {
            //技能（远程）攻击动画
            anim.SetTrigger("Skill");
        }
    }

    //随机得到一个新的wayPoint
    void GetNewWayPoint()
    {
        remainLookAtTime = lookAtTime;

        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);

        //y值保留当前位置的y值，因为地图上有地形高度差
        Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);

        //没有考虑到地图杂物多时，所选择点位为不可移动的点位
        // wayPoint = randomPoint;

        //敌人巡逻时规避不可移动的点位，防止当前randomPoint取点导致敌人一头攒死在杂物上
        NavMeshHit hit;
        //SamplePosition：在指定范围内找到导航网格上离randomPoint最近的一个可以移动的点，如果找到了就是true，反之false
        //areaMask:在navigation中的area处可以查到1是Walkable
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ? randomPoint : transform.position;
    }

    //在sightRadius内寻找是否存在Player
    bool FoundPlayer()
    {
        var colliders = Physics.OverlapSphere(transform.position, sightRadius);

        foreach (var target in colliders)
        {
            if (target.CompareTag("Player"))
            {
                attackTarget = target.gameObject;
                return true;
            }
        }

        return false;
    }

    //用于画出敌人的侦察范围和索敌范围，可选择性的
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        //画出当前怪物的视野半径
        Gizmos.DrawWireSphere(transform.position, sightRadius);
        // Gizmos.color = Color.red;
        // Gizmos.DrawWireSphere(guardPos, patrolRange);
    }

    //Animation Event
    //攻击地方
    void Hit()
    {
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStates = attackTarget.GetComponent<CharacterStates>();

            targetStates.TakeDamage(characterStates, targetStates);
        }
    }

    //实现IEndGameObserver接口的函数
    public void EndNotify()
    {
        //获胜动画
        //停止所有移动
        //停止Agent
        anim.SetBool("Win", true);
        playerDead = true;
        isChase = false;
        isWalk = false;
        attackTarget = null;
    }
}
