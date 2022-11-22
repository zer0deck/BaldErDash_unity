using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enemies;
using Pathfinding;

public class Bee : EnemyMain, IFlyer
{

    IMovement movement = new FlyAction();
    public void Fly(Transform target)
    {
        if(currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else{
            reachedEndOfPath = false;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - m_Rigidbody2D.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;

        m_Rigidbody2D.AddForce(force);
        float distance = Vector2.Distance(m_Rigidbody2D.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }
        movement.Move(seeker, gameObject.transform, target);
    }

    public override void MoveToGoal(Transform target)
    {
        Fly(target);
    }
}
