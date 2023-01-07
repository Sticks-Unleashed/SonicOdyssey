using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This script contains every single action Sonic is capable. They should all be derived from ActionBase.
/// Every action script needs to have Initialize, Update, and FixedUpdate override functions, as well as
/// a line to initialize the base class. It is not necessary to call base.FixedUpdate or base.Update, only base.Initialize.
/// </summary>
[AddComponentMenu("")]
public class ActionBase : MonoBehaviour
{
    [HideInInspector] public PlayerController player;
    [HideInInspector] public PlayerActions actions;
    /// <summary>
    /// In child action classes, Initialize is used to set the Player and Action values if they have not already been set,
    /// as well as any other things you wish to perform upon changing actions, such as adding the initial jump force, or
    /// beginning the spin dash charge.
    /// </summary>
    public virtual void InitializeState(PlayerController p, PlayerActions a) {
        if (!player || !actions)
        {
            player = p;
            actions = a;
        }
    }
    /// <summary>
    /// UpdateState and FixedUpdateState are more or less normal Update and FixedUpdate functions, except they are only
    /// updated if that state is the current active state.
    /// </summary>
    public virtual void UpdateState() { }
    public virtual void FixedUpdateState() { }
}
