using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AI;

namespace Enemies {

    // Шо может враг? Ходить, прыгать, летать, атаковать (обычная, усиленная, супер), терять хп, умирать, генерировать монетки
    /// <summary>
    ///  Абстрактный класс EnemyMain
    ///  основной родительский класс для врагов
    ///  содержит кучу абстрактного, виртуального, приватного и публичного говна. 
    ///  Я совершенно не понимаю, что там происходит уже, потому что писал это 7 часов без единого комментария.
    ///  А вы что думали?
    /// </summary>
    public abstract class EnemyMain : MonoBehaviour, IMeleeAttack
    {
        private Rigidbody2D m_Rigidbody2D;
        private bool m_FacingRight = true;  // For determining which way the player is currently facing.
        public float life = 10;
        private bool facingRight = true;
        public float speed = 5f; 
        public bool isInvincible = false;
        private bool isHitted = false;


        public GameObject player;
        private float distToPlayer;
        public float meleeDist = 1.5f;
        public float rangeDist = 5f;
        private bool canAttack = true;
        private Transform attackCheck;
        public float dmgValue = 4;
        public float ditanceToSeePlayer = 1f;

        public GameObject throwableObject;

        private float randomDecision = 0;
        private bool doOnceDecision = true;
        private bool endDecision = false;
        private Animator animator;

        private CharacterController2D playerScript;

        private NavMeshAgent agent;

        IAttack defaultAttack = new DefaultEnemyAttack();

        /// <summary>
        /// Метод Awake() является
        /// входной точкой инициализации.
        /// Заполняет Rigidbode2D, attackCheck и animator
        /// </summary>
        void Awake()
        {
            m_Rigidbody2D = GetComponent<Rigidbody2D>();
            attackCheck = transform.Find("AttackCheck").transform;
            animator = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();
        }

        /// <summary>
        /// Метод FixedUpdate(). Приватный. 
        /// В дочерних классах не редактируется, иначе все полетит к хуям.
        /// Содержит проверку на смерть, переворот тайла и основную иерархию действий.
        /// Если игрок входит в диапазон зрения врага, то либо 100% атака совсем вблизи, либо Другие атаки OtherAttacks(), либо другие действия OtherDoingsInClose();
        /// В противном случае выполняется виртуальная функция рандомного поведения, внутри которой 40%-ая вероятность действия IDLE и 60% вероятность абстрактной функции RandomMove()
        /// </summary>
        private void FixedUpdate()
        {
            if (player==null) return;
            if (life <=0) 
            {
                WaitToDead().Forget();
                return;
            }
            if (!isHitted)
            {
                // Если игрок в неком диапазоне, то производится расчет маршрута до него и Move()
                distToPlayer = player.transform.position.x - transform.position.x;
                if (distToPlayer<ditanceToSeePlayer) 
                {
                    if (canAttack)
                    {
                        if (distToPlayer < meleeDist)
                        {
                            if (randomDecision <= 0.5f)
                                MeleeAttack(playerScript, dmgValue);
                            else
                                HardAttack(playerScript, dmgValue);
                            
                            if (doOnceDecision)
                                NextDecision(0.5f).Forget();                                 
                        }
                        else{
                            OtherAttacks();
                        };
                    }
                    else{
                        
                        MoveToGoal(player);
                    }

                }
                else{
                    RandomBehaviour();
                }
            }

            if ((transform.localScale.x * m_Rigidbody2D.velocity.x > 0 && !m_FacingRight) || (transform.localScale.x * m_Rigidbody2D.velocity.x < 0 && m_FacingRight))
            {
                Flip();
            }      
        }


        /// <summary>
        /// Методы MeleeAttack(), HardAttack(), RangeAttack(), SuperAttack() являются видами атак.
        /// OtherAttacks() отвечает за Range и Super
        /// Все виды виртуальные, обязательный только Melee
        /// Если враг не атакует, а игрок в диапазоне, то враг двигается к нему (абстрактный метод MoveToGoal)
        /// </summary>
        public void MeleeAttack(CharacterController2D pScript, float dmg)
        {
            defaultAttack.Attack(this.gameObject, pScript, dmg);
            WaitToAttack(0.5f).Forget();
        }
        public virtual void HardAttack(CharacterController2D pScript, float dmg)
        {
            MeleeAttack(pScript, dmg);
        }
        public virtual void SuperAttack()
        {
            MoveToGoal(player);
        }
        public virtual void RangeAttack()
        {
            MoveToGoal(player);
        }
        public void OtherAttacks()
        {
            if (!endDecision)
            {
                if ((distToPlayer > 0f && transform.localScale.x < 0f) || (distToPlayer < 0f && transform.localScale.x > 0f)) 
                    Flip();
                if (randomDecision <= 0.2f)
                    RangeAttack();
                    if (doOnceDecision)
                        NextDecision(0.5f).Forget();   
                else if (randomDecision <= 0.4f)
                    SuperAttack();
                    if (doOnceDecision)
                        NextDecision(0.5f).Forget();   
                else
                    MoveToGoal(player);
                    if (doOnceDecision)
                        NextDecision(1f).Forget();
            }
        }
        public abstract void MoveToGoal(GameObject player);

        /// <summary>
        /// Метод RandomBehaviour() для случайного поведения, когда враг не видит игрока. движение с вероятностью 60%
        /// RandomMove() абстрактный, зависит от наследования IMovement
        /// </summary>
        public virtual void RandomBehaviour()
        {
            if (!endDecision)
            {
                if ((distToPlayer > 0f && transform.localScale.x < 0f) || (distToPlayer < 0f && transform.localScale.x > 0f)) 
                    Flip();

                if (randomDecision <= 0.6f)
                    MoveToGoal(player);
                    if (doOnceDecision)
                        NextDecision(0.5f).Forget();
                else
                    Idle();
            }
            else
            {
                endDecision = false;
            }
        }

        /// <summary>
        /// Метод EndDecision() и корутина NextDecision(float time)
        /// Генераторы решений.
        /// </summary>
        public void EndDecision()
        {
            randomDecision = Random.Range(0.0f, 1.0f); 
            endDecision = true;
        }
        private async UniTask NextDecision(float time)
        {
            doOnceDecision = false;
            await UniTask.Delay(System.TimeSpan.FromSeconds(time));
            EndDecision();
            doOnceDecision = true;
            animator.SetBool("Idle", false);
        }

        /// <summary>
        /// Метод Idle(), метод Flip()
        /// Дефолтный Idle аниматор, Flip разворачиватель
        /// </summary>
        public void Idle()
        {
            m_Rigidbody2D.velocity = new Vector2(0f, m_Rigidbody2D.velocity.y);
            if (doOnceDecision)
            {
                animator.SetBool("Idle", true);
                NextDecision(1f).Forget();
            }
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
                m_Rigidbody2D.velocity = new Vector2(0, 0);
                m_Rigidbody2D.AddForce(new Vector2(direction * 300f, 100f)); 
                HitTime().Forget();
            }
        }

        private async UniTask WaitToAttack(float time)
        {
            canAttack = false;
            await UniTask.Delay(System.TimeSpan.FromSeconds(time));
            canAttack = true;
        }
        private async UniTask HitTime()
        {
            isInvincible = true;
            isHitted = true;
            await UniTask.Delay(System.TimeSpan.FromSeconds(0.1f));
            isHitted = false;
            isInvincible = false;
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
            m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
            await UniTask.Delay(System.TimeSpan.FromSeconds(1f));
            Destroy(gameObject);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos(){
            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.DrawWireDisc(transform.position ,transform.forward, ditanceToSeePlayer);
        }
#endif // UNITY_EDITOR
    }

/// Interfaces
    interface IMovement
    {
        void Move(Vector2 playerCoordinates);
    }

    interface IWalker
    {
        void Walk(Vector2 playerCoordinates);
    }
    interface IFlyer
    {
        void Fly(Vector2 playerCoordinates);
    }

    class WalkAction : IMovement
    {
        public void Move(Vector2 playerCoordinates)
        {
            Debug.Log("Walking");
        }
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
        void Attack(GameObject enemy, CharacterController2D pScript, float dmg);
    }
    
    interface IMeleeAttack
    {
        public void MeleeAttack(CharacterController2D pScript, float dmg);
    }
    interface IHardAttack
    {
        void HardAttack(CharacterController2D pScript, float dmg);

    }
    interface ISuperAttack
    {
        void SuperAttack(CharacterController2D pScript, float dmg);
    }
    interface IRangeAttack
    {
        public void RangeAttack(GameObject player);
    }

    class DefaultEnemyAttack : IAttack
    {
        Transform attackCheck;
        public void Attack(GameObject enemy, CharacterController2D pScript, float dmg)
        {
            enemy.transform.GetComponent<Animator>().SetBool("Attack", true);
            Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, 0.9f);
            for (int i = 0; i < collidersEnemies.Length; i++)
            {
                if (collidersEnemies[i].gameObject.tag == "Player")
                {
                    pScript.ApplyDamage(dmg, enemy.transform.position);
                }
            }
        }

    }
    class HardEnemyAttack : IAttack
    {
        public void Attack(GameObject enemy, CharacterController2D pScript, float dmg)
        {
            Debug.Log("Hard Attack");
        }
    }
    class SuperEnemyAttack : IAttack
    {
        public void Attack(GameObject enemy, CharacterController2D pScript, float dmg)
        {
            Debug.Log("Super Attack");
        }
    }
    class RangeEnemyAttack : IAttack
    {
        public void Attack(GameObject enemy, CharacterController2D pScript, float dmg)
        {
            Debug.Log("Range Attack");
        }
    }

}