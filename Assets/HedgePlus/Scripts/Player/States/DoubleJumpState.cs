using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[AddComponentMenu("Pinball/Actions/Double Jump")]
[RequireComponent(typeof(PlayerActions))]
public class DoubleJumpState : ActionBase
{
    public float Force = 0.75f;
    public override void InitializeState(PlayerController p, PlayerActions a)
    {
        base.InitializeState(p, a);
        actions.StateIndex = 1;
        player.rigidBody.velocity = Vector3.ProjectOnPlane(player.rigidBody.velocity, player.GroundNormal);
        player.rigidBody.AddForce(player.GroundNormal * Force, ForceMode.Impulse);
        actions.animator.PlayDoubleJumpSound();
    }

    public override void UpdateState()
    {
        if (player.a_input.GetButton("Jump", InputHandler.ButtonState.Down))
        {
            if (actions.ClosestTarget != null)
            {
                if (actions.CheckForState(typeof(HomingState)))
                {
                    actions.ChangeState(typeof(HomingState));
                }

            }
        }

        if (player.Grounded)
        {
            actions.ChangeState(typeof(DefaultState));
        }
    }
}
