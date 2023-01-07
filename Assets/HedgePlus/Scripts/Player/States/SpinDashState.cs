using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[AddComponentMenu("Pinball/Actions/Spin Dash")]
[RequireComponent(typeof(PlayerActions))]
public class SpinDashState : ActionBase
{
    public CapsuleCollider col;
    public enum DashStyle { Classic, Adventure }
    public DashStyle spinDashType;

    public int MaxRevs;
    public float MaxCharge;
    public float ChargeDuration; //How long does it take to reach max charge
    public float SpinDashCharge { get; set; }
    float RevAmount;

    public bool ChargeWhileRunning;
    public override void InitializeState(PlayerController p, PlayerActions a)
    {
        base.InitializeState(p, a);
        Debug.Log("Current State: Spin Dash");
        if (player != null)
            Debug.Log("Found Player Controller");
        if (actions != null)
            Debug.Log("Found Action Controller");
        //col.height = actions.defaultState.CrouchHeight;
        //col.center = new Vector3(0, actions.defaultState.CrouchOffset, 0);
        actions.StateIndex = 4;
        SpinDashCharge = 0;
        RevAmount = MaxCharge / MaxRevs;
        switch (spinDashType)
        {
            case DashStyle.Adventure:
                actions.animator.SpinDashLoop();
                break;
            case DashStyle.Classic:
                actions.animator.SpinDashSingle();
                break;
        }
    }
    public override void UpdateState()
    {
        base.UpdateState();
        if (SpinDashCharge < MaxCharge)
        {
            switch (spinDashType)
            {
                case DashStyle.Adventure:
                    SpinDashCharge += MaxCharge * (Time.deltaTime / ChargeDuration);
                    break;
                case DashStyle.Classic:
                    if (player.a_input.GetButton("Jump", InputHandler.ButtonState.Down))
                    {
                        SpinDashCharge += RevAmount;
                        actions.animator.SpinDashSingle();
                    }
                    break;
            }
        }
        if (player.a_input.GetButton("Crouch", InputHandler.ButtonState.Up))
        {
            actions.animator.SpinDashRelease();
            player.Crouching = true;
            player.rigidBody.velocity = player.transform.forward * SpinDashCharge;
            actions.ChangeState(typeof(DefaultState));
        }
    }
    public override void FixedUpdateState()
    {
        base.FixedUpdateState();
        player.rigidBody.velocity = Vector3.zero;
    }
}
