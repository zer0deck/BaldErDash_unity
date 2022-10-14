using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.UI;

public class Attack : MonoBehaviour
{
	[SerializeField] Image Attack_animator;
	public float dmgValue = 4;
	public float attackCooldown = 0.25f;
	public GameObject throwableObject;
	public Transform attackCheck;
	private Rigidbody2D m_Rigidbody2D;
	public Animator animator;
	public bool canAttack = true;
	public bool isTimeToCheck = false;

	public float Mana , MaxMana;

	public GameObject cam;

	private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();

	}

	private void Start() {
		MaxMana = DataSaver.instance.state.MaxMana;
		Mana = MaxMana;
	}
	
    // Update is called once per frame
    void Update()
    {
		if (CrossPlatformInputManager.GetButtonDown("Attack") && canAttack)
		{
			canAttack = false;
			animator.SetBool("IsAttacking", true);
			StartCoroutine(AttackCooldown(attackCooldown));
			StartCoroutine(AttackAnimation(attackCooldown));
		}

		if (CrossPlatformInputManager.GetButtonDown("Shoot"))
		{
			GameObject throwableWeapon = Instantiate(throwableObject, transform.position + new Vector3(transform.localScale.x * 0.5f,-0.2f), Quaternion.identity) as GameObject; 
			Vector2 direction = new Vector2(transform.localScale.x, 0);
			throwableWeapon.GetComponent<ThrowableWeapon>().direction = direction; 
			throwableWeapon.name = "ThrowableWeapon";
		}
	}

	IEnumerator AttackCooldown(float time)
	{
		yield return new WaitForSeconds(time);
		canAttack = true;
	}
	IEnumerator AttackAnimation(float time) {
		float fulltime = time;
		while (time > 0) {
			Attack_animator.fillAmount = 1 - time/fulltime;
			time -= 0.01f;
			yield return new WaitForSeconds(0.01f);
		}
		Attack_animator.fillAmount = 1;
	}

	public void DoDashDamage()
	{
		dmgValue = Mathf.Abs(dmgValue);
		Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, 0.9f);
		for (int i = 0; i < collidersEnemies.Length; i++)
		{
			if (collidersEnemies[i].gameObject.tag == "Enemy")
			{
				if (collidersEnemies[i].transform.position.x - transform.position.x < 0)
				{
					dmgValue = -dmgValue;
				}
				collidersEnemies[i].gameObject.SendMessage("ApplyDamage", dmgValue);
				cam.GetComponent<CameraFollow>().ShakeCamera();
			}
		}
	}
}
