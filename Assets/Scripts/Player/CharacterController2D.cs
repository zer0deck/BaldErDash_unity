using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterController2D : MonoBehaviour
{
	[SerializeField] private float m_JumpForce = 400f;							// Amount of force added when the player jumps.
	[Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;	// How much to smooth out the movement
	[SerializeField] private bool m_AirControl = false;							// Whether or not a player can steer while jumping;
	[SerializeField] private LayerMask m_WhatIsGround;							// A mask determining what is ground to the character
	[SerializeField] private Transform m_GroundCheck;							// A position marking where to check if the player is grounded.
	[SerializeField] private Transform m_WallCheck;								//Posicion que controla si el personaje toca una pared
	[SerializeField] private Image Dash_animator; //Animators of the UI
	[SerializeField] private GameObject PlayerStats;

	const float k_GroundedRadius = 0.3f; // Radius of the overlap circle to determine if grounded
	private bool m_Grounded;            // Whether or not the player is grounded.
	private Rigidbody2D m_Rigidbody2D;
	private bool m_FacingRight = true;  // For determining which way the player is currently facing.
	private Vector3 velocity = Vector3.zero;
	private float limitFallSpeed = 25f; // Limit fall speed

	public int wallSlidingSpeed = 1;
	public bool canDoubleJump = true; //If player can double jump
	
	public float dashCooldown = 1.0f;
	[SerializeField] private float m_DashForce = 15f;
	private bool canDash = true;
	private bool isDashing = false; //If player is dashing
	private bool m_IsWall = false; //If there is a wall in front of the player
	private bool isWallSliding = false; //If player is sliding in a wall
	private bool oldWallSlidding = false; //If player is sliding in a wall in the previous frame
	private float prevVelocityX = 0f;
	private bool canCheck = false; //For check if player is wallsliding

	public float MaxLife;
	public float life; //Life of the player
	public bool invincible = false; //If player can die
	private bool canMove = true; //If player can move

	private Animator animator;
	private RectTransform hb, mb;
	public ParticleSystem particleJumpUp; //Trail particles
	public ParticleSystem particleJumpDown; //Explosion particles

	private const float SHIFT = 232;
	private float jumpWallStartX = 0;
	private float jumpWallDistX = 0; //Distance between player and wall
	private bool limitVelOnWallJump = false; //For limit wall jump distance with low fps

	[Header("Events")]
	[Space]

	public UnityEvent OnFallEvent;
	public UnityEvent OnLandEvent;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();

		if (OnFallEvent == null)
			OnFallEvent = new UnityEvent();

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();
		
		hb = PlayerStats.GetComponent<RectTransform>();
	}

	private void Start() {
		MaxLife = DataSaver.instance.state.MaxLife;
		life = MaxLife;
	}

	private void FixedUpdate()
	{
		bool wasGrounded = m_Grounded;
		m_Grounded = false;

		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject != gameObject)
				m_Grounded = true;
				if (!wasGrounded )
				{
					OnLandEvent.Invoke();
					if (!m_IsWall && !isDashing) 
						particleJumpDown.Play();
					canDoubleJump = true;
					if (m_Rigidbody2D.velocity.y < 0f)
						limitVelOnWallJump = false;
				}
		}

		m_IsWall = false;

		if (!m_Grounded)
		{
			OnFallEvent.Invoke();
			Collider2D[] collidersWall = Physics2D.OverlapCircleAll(m_WallCheck.position, k_GroundedRadius, m_WhatIsGround);
			for (int i = 0; i < collidersWall.Length; i++)
			{
				if (collidersWall[i].gameObject != null)
				{
					isDashing = false;
					m_IsWall = true;
				}
			}
			prevVelocityX = m_Rigidbody2D.velocity.x;
		}

		if (limitVelOnWallJump)
		{
			if (m_Rigidbody2D.velocity.y < -0.5f)
				limitVelOnWallJump = false;
			jumpWallDistX = (jumpWallStartX - transform.position.x) * transform.localScale.x;
			if (jumpWallDistX < -0.5f && jumpWallDistX > -1f) 
			{
				canMove = true;
			}
			else if (jumpWallDistX < -1f && jumpWallDistX >= -2f) 
			{
				canMove = true;
				m_Rigidbody2D.velocity = new Vector2(10f * transform.localScale.x, m_Rigidbody2D.velocity.y);
			}
			else if (jumpWallDistX < -2f) 
			{
				limitVelOnWallJump = false;
				m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
			}
			else if (jumpWallDistX > 0) 
			{
				limitVelOnWallJump = false;
				m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
			}
		}
	}


	public void Move(float move, bool jump, bool dash)
	{
		if (canMove) {
			if (dash && canDash && !isWallSliding)
			{
				//m_Rigidbody2D.AddForce(new Vector2(transform.localScale.x * m_DashForce, 0f));
				DashCooldown(dashCooldown).Forget();
				DashAnimation(dashCooldown).Forget();
			}
			// If crouching, check to see if the character can stand up
			if (isDashing)
			{
				m_Rigidbody2D.velocity = new Vector2(transform.localScale.x * m_DashForce, 0);
			}
			//only control the player if grounded or airControl is turned on
			else if (m_Grounded || m_AirControl)
			{
				if (m_Rigidbody2D.velocity.y < -limitFallSpeed)
					m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, -limitFallSpeed);
				// Move the character by finding the target velocity
				Vector3 targetVelocity = new Vector2(0f, 0f);
				if (m_Grounded) {
				targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
				}
				else {
				targetVelocity = new Vector2(move * 13f, m_Rigidbody2D.velocity.y);
				}
				// And then smoothing it out and applying it to the character
				m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref velocity, m_MovementSmoothing);

				// If the input is moving the player right and the player is facing left...
				if (move > 0 && !m_FacingRight && !isWallSliding)
				{
					// ... flip the player.
					Flip();
				}
				// Otherwise if the input is moving the player left and the player is facing right...
				else if (move < 0 && m_FacingRight && !isWallSliding)
				{
					// ... flip the player.
					Flip();
				}
			}
			// If the player should jump...
			if (m_Grounded && jump)
			{
				// Add a vertical force to the player.
				animator.SetBool("IsJumping", true);
				animator.SetBool("JumpUp", true);
				m_Grounded = false;
				m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
				canDoubleJump = true;
				particleJumpDown.Play();
				particleJumpUp.Play();
			}
			else if (!m_Grounded && jump && canDoubleJump && !isWallSliding)
			{
				canDoubleJump = false;
				m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0);
				m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
				animator.SetBool("IsDoubleJumping", true);
			}

			else if (m_IsWall && !m_Grounded)
			{
				if (!oldWallSlidding && m_Rigidbody2D.velocity.y < 0 || isDashing)
				{
					isWallSliding = true;
					m_WallCheck.localPosition = new Vector3(-m_WallCheck.localPosition.x, m_WallCheck.localPosition.y, 0);
					Flip();
					WaitToCheck(0.1f).Forget();
					canDoubleJump = true;
					animator.SetBool("IsWallSliding", true);
				}
				isDashing = false;

				if (isWallSliding)
				{
					if (move * transform.localScale.x > 0.1f)
					{
						WaitToEndSliding().Forget();
					}
					else 
					{
						oldWallSlidding = true;
						m_Rigidbody2D.velocity = new Vector2(-transform.localScale.x * 2, -wallSlidingSpeed);
					}
				}

				if (jump && isWallSliding)
				{
					animator.SetBool("IsJumping", true);
					animator.SetBool("JumpUp", true); 
					m_Rigidbody2D.velocity = new Vector2(0f, 0f);
					m_Rigidbody2D.AddForce(new Vector2(transform.localScale.x * m_JumpForce, m_JumpForce));
					jumpWallStartX = transform.position.x;
					limitVelOnWallJump = true;
					canDoubleJump = true;
					isWallSliding = false;
					animator.SetBool("IsWallSliding", false);
					oldWallSlidding = false;
					m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
					canMove = false;
				}
				else if (dash && canDash)
				{
					isWallSliding = false;
					animator.SetBool("IsWallSliding", false);
					oldWallSlidding = false;
					m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
					canDoubleJump = true;
					DashCooldown(dashCooldown).Forget();
					DashAnimation(dashCooldown).Forget();
				}
			}
			else if (isWallSliding && !m_IsWall && canCheck) 
			{
				isWallSliding = false;
				animator.SetBool("IsWallSliding", false);
				oldWallSlidding = false;
				m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
				canDoubleJump = true;
			}
		}
	}


	private void Flip()
	{
		// HealthSystem.instance.SetValue(0.5f*life/MaxLife);
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	public void ApplyDamage(float damage, Vector3 position) 
	{
		if (!invincible)
		{
			animator.SetBool("Hit", true);
			life -= damage;
			// hb.offsetMax = new Vector2( - ( SHIFT * (life/MaxLife) ), hb.offsetMax [1] );
			Vector2 damageDir = Vector3.Normalize(transform.position - position) * 40f ;
			m_Rigidbody2D.velocity = Vector2.zero;
			m_Rigidbody2D.AddForce(damageDir * 10);
			HealthSystem.instance.SetValue(life/MaxLife);
			if (life <= 0)
			{
				WaitToDead().Forget();
			}
			else
			{
				Stun(0.25f).Forget();
				MakeInvincible(1f).Forget();
			}
		}
	}

	private async UniTask DashCooldown(float time)
	{
		animator.SetBool("IsDashing", true);
		isDashing = true;
		canDash = false;
		await UniTask.Delay(System.TimeSpan.FromSeconds(0.1f));
		isDashing = false;
		await UniTask.Delay(System.TimeSpan.FromSeconds(time));
		canDash = true;
	}

	private async UniTask DashAnimation(float time) {
		float fulltime = time;
		while (time > 0) {
			Dash_animator.fillAmount = 1 - time/fulltime;
			time -= 0.01f;
			await UniTask.Delay(System.TimeSpan.FromSeconds(0.1f));
		}
		Dash_animator.fillAmount = 1;
	}

	private async UniTask Stun(float time) 
	{
		canMove = false;
		await UniTask.Delay(System.TimeSpan.FromSeconds(time));
		canMove = true;
	}
	private async UniTask MakeInvincible(float time) 
	{
		invincible = true;
		await UniTask.Delay(System.TimeSpan.FromSeconds(time));
		invincible = false;
	}
	private async UniTask WaitToMove(float time)
	{
		canMove = false;
		await UniTask.Delay(System.TimeSpan.FromSeconds(time));
		canMove = true;
	}

	private async UniTask WaitToCheck(float time)
	{
		canCheck = false;
		await UniTask.Delay(System.TimeSpan.FromSeconds(time));
		canCheck = true;
	}

	private async UniTask WaitToEndSliding()
	{
		await UniTask.Delay(System.TimeSpan.FromSeconds(0.1f));
		canDoubleJump = true;
		isWallSliding = false;
		animator.SetBool("IsWallSliding", false);
		oldWallSlidding = false;
		m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
	}

	private async UniTask WaitToDead()
	{
		animator.SetBool("IsDead", true);
		canMove = false;
		invincible = true;
		GetComponent<Attack>().enabled = false;
		await UniTask.Delay(System.TimeSpan.FromSeconds(0.4f));
		m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
		await UniTask.Delay(System.TimeSpan.FromSeconds(1.1f));
		await SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
	}

#if UNITY_EDITOR
	private void OnDrawGizmos() {
		Gizmos.DrawSphere(m_GroundCheck.position, k_GroundedRadius);
		if (!m_Grounded){
			Gizmos.DrawSphere(m_WallCheck.position, k_GroundedRadius);
}
	}
#endif // UNITY_EDITOR
}