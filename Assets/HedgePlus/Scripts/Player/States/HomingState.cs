using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[AddComponentMenu("Pinball/Actions/Homing")]
[RequireComponent(typeof(PlayerActions))]
public class HomingState : ActionBase
{
    public VolumeTrailRenderer HomingTrail;

    [Header("Air Dash")]
    public bool OverrideSpeed;
    public float AirDashSpeed;
    public float AirDashDuration;
    [Range(0, 1)] public float AirDashReleaseVelocity;
    float DashSpeed;

    [Header("Homing")]
    public float HomingTimeOut = 2f;
    public float HomingSpeed;
    [Range(0, 1)] public float HomingReleaseVelocity;
    public float HomingBouncePower;

    float t;


    public override void InitializeState(PlayerController p, PlayerActions a)
    {
        base.InitializeState(p, a);
        Debug.Log("Current State: Homing");
        actions.StateIndex = 2;
        //if (actions.ClosestTarget != null)
        //actions.ActiveTarget = actions.ClosestTarget;
        if (actions.ActiveTarget != null)
            actions.animator.PlayHomingVoice();
        t = 0;
        actions.animator.PlayHomingSound();

        if (!OverrideSpeed && player.rigidBody.velocity.magnitude > AirDashSpeed)
            DashSpeed = player.rigidBody.velocity.magnitude;
        else
            DashSpeed = AirDashSpeed;
        HomingTrail.emitTime = AirDashDuration;
        HomingTrail.emit = true;
        //actions.defaultState.DidDash = true;
    }

    public override void UpdateState()
    {

    }
    public override void FixedUpdateState()
    {
        t += Time.fixedDeltaTime;
        if (actions.ActiveTarget != null)
        {
            //Get direction to target
            Vector3 Direction = Vector3.Normalize(actions.ActiveTarget.position - player.transform.position);
            player.rigidBody.velocity = Direction * HomingSpeed;
            player.rigidBody.position = Vector3.MoveTowards(player.rigidBody.position, actions.ActiveTarget.position, HomingSpeed * Time.fixedDeltaTime);

        }
        else
        {
            if (t < AirDashDuration)
            {
                Vector3 DashDirection = Vector3.ProjectOnPlane(player.transform.forward, player.GroundNormal);
                player.rigidBody.velocity = DashDirection * DashSpeed;
            }
            else
            {
                //Release air dash
                player.rigidBody.velocity *= AirDashReleaseVelocity;
                if (player.Crouching)
                    player.Crouching = false;
                actions.ChangeState(typeof(DefaultState));
            }
        }
    }

    }
