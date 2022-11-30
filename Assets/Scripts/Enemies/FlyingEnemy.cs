using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Cysharp.Threading.Tasks;
using System.Threading;

public class FlyingEnemy : MonoBehaviour
{

    [Header("Игрок")]
    public GameObject player;

    [Header("Дистанция зрения")]
    [Range(2.0f,10.0f)]
    public float FOV = 4.5f;

    [Header("Величина шага")]
    public float nextWaypointDistance = 3f;
    
    [Header("Скорость передвижения")]
    [Range(200f,600f)]
    public float speed = 400f;

    [Header("ХП")]
    [Range(50f,300f)]
    public float life = 100f;

    [Header("Урон")]
    [Range(1f,5f)]
    public float dmg = 2.0f;

    [Header("Длительность атаки (с)")]
    [Range(0.1f,1.5f)]
    public float attackLength = 0.8f;

    [Header("Дистанция ближней атаки")]
    [Range(1.0f,3.0f)]
    public float meleeDist = 2.0f;

    // Технические элементы
    private Animator animator;
    private Rigidbody2D rb;
    private bool facingRight = true;

    // Поиск пути
    private Transform target;
    private Seeker seeker;
    private Path path = null;
    private int currentWaypoint;
    private bool reachedEndOfPath=false;
    private bool isFollowingPlayer=false;

    // Атака и дамаг
    private bool canAttack = true;
    private bool isHitted = false;
    private bool isInvincible = false;
    private bool isDead=false;
	private float distToPlayer;
	private float distToPlayerY;
    private CharacterController2D pScript;
	public Transform attackCheck;
    CancellationTokenSource cancellEverythingByHit;


    private void Start ()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        target = player.transform;
        animator = GetComponent<Animator>();
        pScript = player.GetComponent<CharacterController2D>();
        cancellEverythingByHit = new CancellationTokenSource();
        // attackCheck = transform.Find("AttackCheck").transform;
        
        // InvokeRepeating("UpdatePath", 0f, .5f);
    }

    void UpdatePath()
    {
        if (seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }
    private void OnPathComplete(Path p) 
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        if (life <= 0)
		{
			WaitToDead().Forget();
            isDead = true;
		}
        distToPlayer = player.transform.position.x - transform.position.x;
		distToPlayerY = player.transform.position.y - transform.position.y;
        animator.SetBool("Idle", true);

        if ((Mathf.Abs(distToPlayer) < FOV &&  Mathf.Abs(distToPlayerY) < FOV) && !isFollowingPlayer)
        {
            // Debug.Log($"DistToPlayer: {Mathf.Abs(distToPlayer)}");
            // Debug.Log("Player SEEN");
            isFollowingPlayer = true;
            InvokeRepeating("UpdatePath", 0f, .5f);
        }

        if (isHitted) return;
        if (Mathf.Abs(distToPlayer) < meleeDist && Mathf.Abs(distToPlayerY) < 2f)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            if ((distToPlayer > 0f && transform.localScale.x < 0f) || (distToPlayer < 0f && transform.localScale.x > 0f)) 
                Flip();
            if (canAttack)
            {
                cancellEverythingByHit = new CancellationTokenSource();
                Attack().AttachExternalCancellation(cancellEverythingByHit.Token).Forget();
                return;
            }
        }
        if (path == null)
        {
            return;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        } else
        {
            reachedEndOfPath = false;
        }
        

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        // Debug.Log(direction);
        Vector2 force = direction * speed * Time.deltaTime;

        rb.AddForce(force);
        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x+rb.velocity.y));

        if (rb.velocity.x>0 && !facingRight)
        {
            Flip();
        } else if (rb.velocity.x<0 && facingRight)
        {
            Flip();
        }
    }

    // public void Idle()
    // {
    //     rb.velocity = new Vector2(0f, rb.velocity.y);
    //     if (doOnceDecision)
    //     {
    //         animator.SetBool("Idle", true);
    //         NextDecision(1f).Forget();
    //     }
    // }

    void Flip()
    {
        facingRight = !facingRight;

        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public void ApplyDamage(float damage)
    {
        if (!isInvincible)
        {
            cancellEverythingByHit.Cancel();
            float direction = damage / Mathf.Abs(damage);
            damage = Mathf.Abs(damage);
            animator.SetBool("Hit", true);
            animator.SetBool("Attack", false);
            animator.SetBool("Idle", false);
            life -= damage;
            Debug.Log($"Life: {life}");
            rb.velocity = new Vector2(0, 0);
            rb.AddForce(new Vector2(direction * 300f, 100f)); 
            HitTime().Forget();
        }
    }    

    private async UniTask Attack()
    {
        canAttack = false;
        // Debug.Log($"Anim Speed before: {animator.speed}");
        animator.SetBool("Attack", true);
        animator.speed = (float)Mathf.Round(0.8f/attackLength*100)/100;
        // Debug.Log($"Anim Speed in: {animator.speed}");
        await UniTask.Delay(System.TimeSpan.FromSeconds((float)Mathf.Round(attackLength/2.0f*100)/100));
        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, meleeDist);
        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Player")
            {
                if ((distToPlayer > 2.0f && transform.localScale.x < 0f) || (distToPlayer < 2.0f && transform.localScale.x > 0f))
                    pScript.ApplyDamage(dmg, transform.position);
            }
        }
        await UniTask.Delay(System.TimeSpan.FromSeconds((float)Mathf.Round(attackLength/2.0f*100)/100));
        canAttack = true;
        animator.SetBool("Attack", false);
        animator.speed = 1.0f;
        // Debug.Log($"Anim Speed after: {animator.speed}");
    }

    private async UniTask HitTime()
    {
        animator.SetBool("Hit", true);
        isInvincible = true;
        isHitted = true;
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.18f));
        isHitted = false;
        isInvincible = false;
        animator.SetBool("Hit", false);
    }
    private async UniTask WaitToDead()
    {
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
        capsule.size = new Vector2(1f, 0.25f);
        capsule.offset = new Vector2(0f, -0.8f);
        capsule.direction = CapsuleDirection2D.Horizontal;
        transform.GetComponent<Animator>().SetBool("IsDead", true);
        canAttack = false;
        isInvincible = true;
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.25f));
        rb.velocity = new Vector2(0, rb.velocity.y);
        await UniTask.Delay(System.TimeSpan.FromSeconds(1f));
        Destroy(gameObject);
    }

#if UNITY_EDITOR

        private static void OnDrawGizmosCircle(Vector3 center, float radius, Color color, int segments)
        {
            Gizmos.color = color;
            const float TWO_PI = Mathf.PI *2;
            float step = TWO_PI / (float)segments;
            float theta = 0;
            float x = radius * Mathf.Cos(theta);
            float y = radius * Mathf.Sin(theta);
            Vector3 pos = center + new Vector3(x, y, 0);
            Vector3 newPos;
            Vector3 lastPos = pos;
            for (theta = step; theta < TWO_PI; theta += step) 
            {
                x = radius * Mathf.Cos(theta);
                y = radius * Mathf.Sin(theta);
                newPos = center + new Vector3(x, y, 0);
                Gizmos.DrawLine(pos, newPos);
                pos = newPos;
            }
            Gizmos.DrawLine(pos, lastPos);
        }
        private void OnDrawGizmos(){
            // Gizmos.DrawSphere(attackCheck.position, meleeDist);
            OnDrawGizmosCircle(transform.position, FOV, Color.yellow, 10);
            OnDrawGizmosCircle(attackCheck.position, meleeDist, Color.red, 10);
            // UnityEditor.Handles.color = Color.yellow;
            // UnityEditor.Handles.DrawWireDisc(transform.position ,transform.forward, FOV);
        }
#endif // UNITY_EDITOR

}

