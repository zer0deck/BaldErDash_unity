using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerMovement : MonoBehaviour {

	public CharacterController2D controller;
	public Animator animator, L_animator, R_animator, Jump_ainimator;

	public float runSpeed = 40f;

	float horizontalMove = 0f;
	bool jump = false;
	bool dash = false;
	bool climb = false;

	//bool dashAxis = false;
	
	// Update is called once per frame
	void Update () {

		horizontalMove = CrossPlatformInputManager.GetAxis("Horizontal") * runSpeed;


		animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

		if (horizontalMove != 0) {
			if (horizontalMove > 0) {
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

		if (CrossPlatformInputManager.GetButtonDown("Jump"))
		{
			if (controller.m_onStair){
				climb = true;
			}
			else{
				jump = true;
			}
		}

		if (CrossPlatformInputManager.GetButton("Jump") && controller.m_onStair)
		{
			climb = true;
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
		controller.Move(horizontalMove * Time.fixedDeltaTime, jump, dash, climb);
		jump = false;
		dash = false;
		climb = false;
	}

	private IEnumerator JumpCouroutine()
	{
		Jump_ainimator.SetBool("IsJumping", true);
		yield return new WaitForSeconds(0.3f);
		Jump_ainimator.SetBool("IsJumping", false);
	}
}
