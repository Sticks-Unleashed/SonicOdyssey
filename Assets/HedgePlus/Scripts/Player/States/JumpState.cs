using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[AddComponentMenu("Pinball/Actions/Jump")]
[RequireComponent(typeof(PlayerActions))]
public class JumpState : ActionBase
{
    public float InitialJumpForce;
    public float JumpSpeed;
    public float MaxJumpTime;
    public float MinJumpTime;
    public int MaxJumps;
    public int JumpIndex { get; set; }
    float jumpTimer;
    public override void InitializeState(PlayerController p, PlayerActions a)
    {
        base.InitializeState(p, a);
        Debug.Log("Current State: Jump");
        actions.StateIndex = 1;
        jumpTimer = 0;
        player.Grounded = false;
        player.rigidBody.position += player.GroundNormal * player.GroundCheckDistance;
        player.rigidBody.AddForce(player.GroundNormal * InitialJumpForce, ForceMode.Impulse);
    }

    public override void UpdateState()
    {
        actions.UpdateTargets();
        if (jumpTimer < MaxJumpTime)
            jumpTimer += Time.deltaTime;
        if (player.a_input.GetButton("Jump", InputHandler.ButtonState.Up) && jumpTimer < MaxJumpTime)
            jumpTimer = MaxJumpTime;
        if (player.Grounded && jumpTimer > MinJumpTime)
        {
            JumpIndex = 0;
            actions.ChangeState(typeof(DefaultState));
            //actions.homingState.OnExit();
        }

        if (player.a_input.GetButton("Jump", InputHandler.ButtonState.Down))
        {
            if (actions.ClosestTarget == null)
            {
                if (actions.CheckForState(typeof(DoubleJumpState)))
                {
                    actions.ChangeState(typeof(DoubleJumpState));
                }
                else
                {
                    if (actions.CheckForState(typeof(HomingState)) && !actions.DidDash)
                    {
                        actions.DidDash = true;
                        actions.ChangeState(typeof(HomingState));
                    }
                }
            }
            else
            {
                if (actions.CheckForState(typeof(HomingState)))
                {
                    actions.ChangeState(typeof(HomingState));
                }
            }

        }
    }
    public override void FixedUpdateState()
    {
        if (jumpTimer < MaxJumpTime)
            player.rigidBody.velocity += player.GroundNormal * JumpSpeed;
    }
}
