using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Enemies {

    // Шо может враг? Ходить, прыгать, летать, атаковать (обычная, усиленная, супер), терять хп, умирать, генерировать монетки
    public abstract class EnemyMain : MonoBehaviour
    {
        private Rigidbody2D m_Rigidbody2D;
        public bool m_FacingRight = true;
        public float life;
	    public bool invincible = false;
	    private bool canMove = true;
        private Animator animator;
        private GameObject player;


        private void Start() {
            m_Rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
            animator = gameObject.GetComponent<Animator>();
        }

        private void FixedUpdate()
        {
            // Если игрок в неком диапазоне, то производится расчет маршрута до него и Move()
        }
        public abstract void MoveToGoal(Vector2 playerCoordinates);
        
        private async UniTask WaitToDead()
        {
            animator.SetBool("IsDead", true);
            canMove = false;
            invincible = true;
            await UniTask.Delay(System.TimeSpan.FromSeconds(0.4f));
            m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
            await UniTask.Delay(System.TimeSpan.FromSeconds(1.1f));
            Destroy(gameObject);
        }
    }

    interface IMovement
    {
        void Move(Vector2 playerCoordinates);
    }

    interface IWalker
    {
        void Walk(Vector2 playerCoordinates);
    }

    class WalkAction : IMovement
    {
        public void Move(Vector2 playerCoordinates)
        {
            Debug.Log("Walking");
        }
    }

    interface IFlyer
    {
        void Fly(Vector2 playerCoordinates);
    }

    class FlyAction : IMovement
    {
        public void Move(Vector2 playerCoordinates)
        {
            Debug.Log("Flying");
        }
    }

    interface IAttack
    {
        void Attack();
    }
    
    interface IDefaultAttack
    {
        void DefaultAttack();
    }
    interface IHardAttack
    {
        void HardAttack();

    }
    interface ISuperAttack
    {
        void SuperAttack();
    }

    class DefaultEnemyAttack : IAttack
    {
        public void Attack()
        {
            Debug.Log("Default Attack");
        }
    }

    class HardEnemyAttack : IAttack
    {
        public void Attack()
        {
            Debug.Log("Hard Attack");
        }
    }

    class SuperEnemyAttack : IAttack
    {
        public void Attack()
        {
            Debug.Log("Super Attack");
        }
    }

}