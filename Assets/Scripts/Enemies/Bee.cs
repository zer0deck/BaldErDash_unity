using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enemies;

public class Bee : EnemyMain, IFlyer, IDefaultAttack, IHardAttack
{
    IMovement flyaction;
    private Bee()
    {
        flyaction = new FlyAction();
    }

    private void MoveToGoal(Vector2 playerCoordinates)
    {
        Fly(playerCoordinates);
    }
    
    private void Fly(Vector2 playerCoordinates)
    {
        flyaction.Move(playerCoordinates);
    }

}