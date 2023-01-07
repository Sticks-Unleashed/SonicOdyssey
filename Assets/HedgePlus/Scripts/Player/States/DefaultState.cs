using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[AddComponentMenu("Pinball/Actions/Default")]
[RequireComponent(typeof(PlayerActions))]
public class DefaultState : ActionBase
{
    public float RollingStartSpeed;
    public CapsuleCollider col;
    public float CrouchHeight = 0.67f; //How tall the collider is when crouching
    public float CrouchOffset = -0.028f; //Y Offset of the collider when crouching
    public override void InitializeState(PlayerController p, PlayerActions a)
    {
        base.InitializeState(p, a);
        actions.StateIndex = 0;
        Debug.Log("Current State: Regular");
    }

    public override void UpdateState()
    {
        float speed = player.rigidBody.velocity.magnitude;
        if (player.Grounded)
        {
            actions.DidDash = false;
            if (player.a_input.GetButton("Jump", InputHandler.ButtonState.Down))
            {
                bool JumpDisabled = player.Crouching && player.rigidBody.velocity.magnitude < RollingStartSpeed;
                if (!JumpDisabled)
                {
                    actions.animator.PlayJumpSound();
                    actions.animator.PlayJumpVoice();
                    if (actions.CheckForState(typeof(JumpState))) actions.ChangeState(typeof(JumpState));
                }
            }
            if (player.a_input.GetButton("Crouch", InputHandler.ButtonState.Down) && speed >= RollingStartSpeed)
            {
                player.Crouching = !player.Crouching;
            }
            if (speed < RollingStartSpeed)
            {
                if (player.a_input.GetButton("Crouch", InputHandler.ButtonState.Down))
                {
                    col.height = CrouchHeight;
                    col.center = new Vector3(0, CrouchOffset, 0);
                    actions.ChangeState(typeof(SpinDashState));
                }
            }

            col.height = player.Crouching ? CrouchHeight : 1f;
            Vector3 Offset = new Vector3(0, player.Crouching ? CrouchOffset : 0f, 0);
            col.center = Offset;

            //actions.homingState.OnExit();

        }
        else
        {
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

        actions.UpdateTargets();
    }
    public override void FixedUpdateState()
    {

    }
}
