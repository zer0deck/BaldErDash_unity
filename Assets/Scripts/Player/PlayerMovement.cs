using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerMovement : MonoBehaviour {

	public CharacterController2D controller;
	public Animator animator, L_animator, R_animator, Jump_ainimator;

	public float runSpeed = 40f;

	float horizontalMove = 0f, horizontalMovePCDebug = 0f;
	bool jump = false;
	bool dash = false;

	//bool dashAxis = false;
	
	// Update is called once per frame
	void Update () {

		horizontalMove = CrossPlatformInputManager.GetAxis("Horizontal") * runSpeed;
		horizontalMovePCDebug = 4*Input.GetAxis("Horizontal");

		if (horizontalMove != 0) {
			animator.SetFloat("Speed", Mathf.Abs(horizontalMove));
		} else if (horizontalMovePCDebug !=0) {
			animator.SetFloat("Speed", Mathf.Abs(horizontalMovePCDebug));
		}
		else {animator.SetFloat("Speed", Mathf.Abs(0f)); }

		if (horizontalMove != 0 || horizontalMovePCDebug != 0) {
			if (horizontalMove > 0 || horizontalMovePCDebug > 0) {
				L_animator.SetBool("Pressed", false);
				R_animator.SetBool("Pressed", true);
			}
			else {
				L_animator.SetBool("Pressed", true);
				R_animator.SetBool("Pressed", false);
			}
		}
		else {
			L_animator.SetBool("Pressed", false);
			R_animator.SetBool("Pressed", false);
		}

		if (CrossPlatformInputManager.GetButtonDown("Jump") || Input.GetButtonDown("Jump"))
		{
			jump = true;
			StartCoroutine(JumpCouroutine());
		}

		if (CrossPlatformInputManager.GetButtonDown("Dash"))
		{
			dash = true;
		}

		/*if (Input.GetAxisRaw("Dash") == 1 || Input.GetAxisRaw("Dash") == -1) //RT in Unity 2017 = -1, RT in Unity 2019 = 1
		{
			if (dashAxis == false)
			{
				dashAxis = true;
				dash = true;
			}
		}
		else
		{
			dashAxis = false;
		}
		*/

	}

	public void OnFall()
	{
		animator.SetBool("IsJumping", true);
	}

	public void OnLanding()
	{
		animator.SetBool("IsJumping", false);
	}

	void FixedUpdate ()
	{
		// Move our character
		if (horizontalMove != 0) {
			controller.Move(horizontalMove * Time.fixedDeltaTime, jump, dash);
		} else if (horizontalMovePCDebug !=0) {
			controller.Move(horizontalMovePCDebug * Time.fixedDeltaTime, jump, dash);
		}
		else {controller.Move(0f * Time.fixedDeltaTime, jump, dash); }

		// controller.Move(horizontalMove * Time.fixedDeltaTime, jump, dash);
		jump = false;
		dash = false;
	}

	private IEnumerator JumpCouroutine()
	{
		Jump_ainimator.SetBool("IsJumping", true);
		yield return new WaitForSeconds(0.3f);
		Jump_ainimator.SetBool("IsJumping", false);
	}
}
