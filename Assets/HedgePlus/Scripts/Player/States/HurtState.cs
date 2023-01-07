using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[AddComponentMenu("Pinball/Actions/Hurt")]
[RequireComponent(typeof(PlayerActions))]
public class HurtState : ActionBase
{
    public float PositionOffset = 0.05f;
    public float BackForce; //How far back is Sonic bumped when hitting an enemy or hazard
    public float UpForce; //How high is Sonic bumped when hitting an enemy or hazard
    public float StateChangeDelay; //How long does Sonic stay on the ground before getting up
    public float RespawnDelay; //How long does Sonic stay on the ground before respawning
    float t;
    public override void InitializeState(PlayerController p, PlayerActions a)
    {
        base.InitializeState(p, a);
        if (!PlayerHealth.IsDead)
        {
            t = StateChangeDelay;
        }
        else
        {
            t = RespawnDelay;
        }
        actions.StateIndex = 3;
        //Lock Sonic's input and rotation
        player.InputLocked = true;
        player.RotationLocked = true;
        player.rigidBody.position += player.GroundNormal * (player.GroundCheckDistance + PositionOffset);
        player.rigidBody.velocity = -player.transform.forward * BackForce + Vector3.up * UpForce;
    }

    public override void UpdateState()
    {
        base.UpdateState();
        if (player.Grounded)
        {
            player.rigidBody.velocity = Vector3.zero;
            //Start the timer, and unlock input and change states when the timer is up
            t -= Time.deltaTime;
            if (t <= 0 && !PlayerHealth.IsDead)
            {
                player.InputLocked = false;
                player.RotationLocked = false;
                actions.ChangeState(typeof(DefaultState));
            }
            if (PlayerHealth.IsDead)
            {
                PlayerHealth _health = player.GetComponent<PlayerHealth>();
                if (_health.LifeCount > 0)
                {
                    StageProgress.instance.BeginRespawn();
                }
                else
                {
                    StageProgress.instance.BeginGameOver();
                }
                actions.ChangeState(typeof(DefaultState));
            }
        }
    }

    public override void FixedUpdateState()
    {
        base.FixedUpdateState();
    }
}
