﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class WalkingEnemy : MonoBehaviour
{

    [Header("Игрок")]
    public GameObject player;

    [Header("Дистанция зрения")]
    [Range(2.0f,10.0f)]
    public float FOV = 4.5f;

    [Header("Скорость передвижения")]
    [Range(1f,10f)]
    public float speed = 2f;

    [Header("ХП")]
    [Range(5f,30f)]
    public float life = 10f;

    [Header("Урон")]
    [Range(1f,5f)]
    public float dmg = 2.0f;

    [Header("Длительность атаки (с)")]
    [Range(0.1f,1.5f)]
    public float attackLength = 0.8f;

    [Header("Дистанция ближней атаки")]
    [Range(1.0f,3.0f)]
    public float meleeDist = 1.5f;

    [Header("Дистанция дальней атаки")]
    [Range(3.0f,8.0f)]
    public float rangeDist = 4f;

    [Header("Сила дэша")]
    [Range(10.0f,30.0f)]
    public float dashForce = 15f;


    // Технические элементы
	private Animator animator;
	private Rigidbody2D rb;
	private bool m_FacingRight = true;  // For determining which way the player is currently facing.
	private bool facingRight = true;

	// Поиск пути

	// Случайные решения
	private float randomDecision = 0;
	private bool doOnceDecision = true;
	private bool endDecision = false;


	// Атака и дамаг
	private bool isDashing = false;
	private bool dashCooldown = false;
    private bool canAttack = true;
    private bool isHitted = false;
    private bool isInvincible = false;
    private bool isDead=false;
	private float distToPlayer;
	private float distToPlayerY;
    private CharacterController2D pScript;
	public Transform attackCheck;
    // CancellationTokenSource cancellEverythingByHit;

	public GameObject throwableObject;


	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		attackCheck = transform.Find("AttackCheck").transform;
		animator = GetComponent<Animator>();
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if (isDead) return;
		if (life <= 0)
		{
			WaitToDead().Forget();
			isDead = true;
		}

		if (isDashing)
		{
			rb.velocity = new Vector2(transform.localScale.x * dashForce, 0);
		}
		else if (!isHitted)
		{
			distToPlayer = player.transform.position.x - transform.position.x;
			distToPlayerY = player.transform.position.y - transform.position.y;

			if (Mathf.Abs(distToPlayer) < 0.25f)
			{
				GetComponent<Rigidbody2D>().velocity = new Vector2(0f, rb.velocity.y);
				animator.SetBool("IsWaiting", true);
			}
			else if (Mathf.Abs(distToPlayer) > 0.25f && Mathf.Abs(distToPlayer) < meleeDist && Mathf.Abs(distToPlayerY) < 2f)
			{
				GetComponent<Rigidbody2D>().velocity = new Vector2(0f, rb.velocity.y);
				if ((distToPlayer > 0f && transform.localScale.x < 0f) || (distToPlayer < 0f && transform.localScale.x > 0f)) 
					Flip();
				if (canAttack)
				{
					MeleeAttack();
				}
			}
			else if (Mathf.Abs(distToPlayer) > meleeDist && Mathf.Abs(distToPlayer) < rangeDist)
			{
				animator.SetBool("IsWaiting", false);
				rb.velocity = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * speed, rb.velocity.y);
			}
			else
			{
				if (!endDecision)
				{

					if (randomDecision < 0.3f)
						Run();
					else if (randomDecision >= 0.3f && randomDecision < 0.4f && Mathf.Abs(distToPlayer)<FOV)
					{
						Debug.Log("Flip");
						SwitchDirection();
					}
					else if (randomDecision >= 0.4f && randomDecision < 0.6f)
						Jump();
					else if (randomDecision >= 0.6f && randomDecision < 0.8f && !dashCooldown)
					{
						dashCooldown = true;
						Dash().Forget();
					}
					else if (randomDecision >= 0.8f && randomDecision < 0.95f)
						RangeAttack();
					else
						Idle();
				}
				else
				{
					endDecision = false;
				}
			}
		}
		else if (isHitted)
		{
			if ((distToPlayer > 0f && transform.localScale.x > 0f) || (distToPlayer < 0f && transform.localScale.x < 0f))
			{
				Flip();
				Dash().Forget();
			}
			else
				Dash().Forget();
		}

        // if (rb.velocity.x>0 && !facingRight)
        // {
        //     Flip();
        // } else if (rb.velocity.x<0 && facingRight)
        // {
        //     Flip();
        // }
	}

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
			float direction = damage / Mathf.Abs(damage);
			damage = Mathf.Abs(damage);
			animator.SetBool("Hit", true);
			life -= damage;
			transform.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
			transform.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(direction * 300f, 100f)); 
			HitTime().Forget();
		}
	}

	public void MeleeAttack()
	{
		transform.GetComponent<Animator>().SetBool("Attack", true);
		Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, 0.9f);
		for (int i = 0; i < collidersEnemies.Length; i++)
		{
			if (collidersEnemies[i].gameObject.tag == "Enemy" && collidersEnemies[i].gameObject != gameObject )
			{
				if (transform.localScale.x < 1)
				{
					dmg = -dmg;
				}
				collidersEnemies[i].gameObject.SendMessage("ApplyDamage", dmg);
			}
			else if (collidersEnemies[i].gameObject.tag == "Player")
			{
				collidersEnemies[i].gameObject.GetComponent<CharacterController2D>().ApplyDamage(2f, transform.position);
			}
		}
		WaitToAttack(0.5f).Forget();
	}

	public void RangeAttack()
	{
		if (doOnceDecision)
		{
			GameObject throwableProj = Instantiate(throwableObject, transform.position + new Vector3(transform.localScale.x * 0.5f, -0.2f), Quaternion.identity) as GameObject;
			throwableProj.GetComponent<ThrowableProjectile>().owner = gameObject;
			Vector2 direction = new Vector2(transform.localScale.x, 0f);
			throwableProj.GetComponent<ThrowableProjectile>().direction = direction;
			NextDecision(0.5f).Forget();
		}
	}

	public void Run()
	{
		animator.SetBool("IsWaiting", false);
		rb.velocity = new Vector2(transform.localScale.x / Mathf.Abs(transform.localScale.x) * speed, rb.velocity.y);
		if (doOnceDecision)
			NextDecision(0.5f).Forget();
	}
	public void Jump()
	{
		Vector3 targetVelocity = new Vector2(transform.localScale.x / Mathf.Abs(transform.localScale.x) * speed, rb.velocity.y);
		Vector3 velocity = Vector3.zero;
		rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref velocity, 0.05f);
		if (doOnceDecision)
		{
			animator.SetBool("IsWaiting", false);
			rb.AddForce(new Vector2(0f, 850f));
			NextDecision(1f).Forget();
		}
	}

	public void Idle()
	{
		rb.velocity = new Vector2(0f, rb.velocity.y);
		if (doOnceDecision)
		{
			animator.SetBool("IsWaiting", true);
			NextDecision(1f).Forget();
		}
	}

	public void SwitchDirection()
	{
		Flip();
		if (doOnceDecision)
		{
			animator.SetBool("IsWaiting", true);
			NextDecision(0).Forget();
		}
	}
	public void EndDecision()
	{
		randomDecision = Random.Range(0.0f, 1.0f); 
		endDecision = true;
	}

	private async UniTask HitTime()
	{
		isInvincible = true;
		isHitted = true;
		await UniTask.Delay(System.TimeSpan.FromSeconds(0.1f));
		isHitted = false;
		isInvincible = false;
	}

	private async UniTask WaitToAttack(float time)
	{
		canAttack = false;
		await UniTask.Delay(System.TimeSpan.FromSeconds(time));
		canAttack = true;
	}

	private async UniTask Dash()
	{
		animator.SetBool("IsDashing", true);
		isDashing = true;
		await UniTask.Delay(System.TimeSpan.FromSeconds(0.1f));
		isDashing = false;
		EndDecision();
		await UniTask.Delay(System.TimeSpan.FromSeconds(1f));
		dashCooldown = false;
	}

	private async UniTask NextDecision(float time)
	{
		doOnceDecision = false;
		await UniTask.Delay(System.TimeSpan.FromSeconds(time));
		EndDecision();
		doOnceDecision = true;
		animator.SetBool("IsWaiting", false);
	}

	private async UniTask WaitToDead()
	{
		CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
		capsule.size = new Vector2(1f, 0.25f);
		capsule.offset = new Vector2(0f, -0.8f);
		capsule.direction = CapsuleDirection2D.Horizontal;
		animator.SetBool("IsDead", true);
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
            OnDrawGizmosCircle(transform.position, FOV, Color.yellow, 10);
            OnDrawGizmosCircle(attackCheck.position, meleeDist, Color.red, 10);
			OnDrawGizmosCircle(attackCheck.position, rangeDist, Color.red, 10);
        }
#endif // UNITY_EDITOR
}
