using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(DefaultState))]
public class PlayerActions : MonoBehaviour
{
    PlayerController player;
    public PlayerAnimator animator { get; set; }
    ActionBase objectState = new ActionBase(); //Empty action, used for pulleys and ziplines and such
    public static ActionBase currentState; //The current state being updated
    public ActionBase[] States;
    public int StateIndex { get; set; } //Changes the current animation state depending on Sonic's state
    //public List<Transform> homingTargets = new List<Transform>();
    public Transform ClosestTarget;
    [HideInInspector] public Transform ActiveTarget;
    public bool DidDash { get; set; }
    public LayerMask TargetLayer;
    public LayerMask BlockingLayers;
    [Range(0, 1)] public float FieldOfView;
    public float MaxHomingDistance;
    public RectTransform ReticleTransform;
    public Animator ReticleAnimator;
    public GameObject Reticle;
    public RectTransform Hud;

    private void Start()
    {
        player = GetComponent<PlayerController>();
        animator = GetComponent<PlayerAnimator>();
        ChangeState(typeof(DefaultState));
    }

    private void Update()
    {
        currentState.UpdateState();
        UpdateHomingReticle();
        //Debug.DrawLine(transform.position, ClosestTarget.position);
    }

    private void FixedUpdate()
    {
        currentState.FixedUpdateState();
    }
    /*
    public void ChangeState(int state)
    {
        ///Here we simply set currentState to the state we would like to transition to, then call Initialize.
        if (currentState is ActionHoming)
            homingState.OnExit();
        switch (state)
        {
            case 0:
                currentState = defaultState;
                break;
            case 1:
                currentState = jumpState;
                break;
            case 2:
                currentState = homingState;
                break;
            case 3:
                currentState = hurtState;
                break;
            case 4:
                currentState = spinDashState;
                break;
            case 5:
                currentState = objectState;
                break;
        }
        currentState.InitializeState(actionBase);
        StateIndex = state; //This is used for changing states in the animator.
    }
    */

    public void ChangeState (System.Type type)
    {
        ActionBase nextState;
        bool IsValidState = false;
        //First we check if the array contains the desired state
        for (int i = 0; i < States.Length; i++)
        {
            if (States[i].GetType() == type)
            {
                IsValidState = true;
                currentState = States[i];
            }
        }

        //Now we change states
        if (IsValidState)
        {
            currentState.InitializeState(player, this);
        } else
        {
            Debug.LogError("The given state does not exist.");
        }
    }
    /// <summary>
    /// Used for checking if a certain state is available before switching to it
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool CheckForState (System.Type type)
    {
        bool IsValidState = false;
        for (int i = 0; i < States.Length; i++)
        {
            if (States[i].GetType() == type)
            {
                IsValidState = true;
            }
        }
        return IsValidState;
    }
    /// <summary>
    /// Gets the closest object tagged "HomingTarget"
    /// </summary>
    /// <returns>Closest Object</returns>
    public Transform GetClosestTarget(LayerMask layer, float Radius, float FOV)
    {
        ///First we use a spherecast to get every object with the given layer in range. Then we go through the
        ///available targets from the spherecast to find which is the closest to Sonic.
        RaycastHit[] TargetsInRange = Physics.SphereCastAll(transform.position, Radius, transform.forward, Radius, layer);
        Transform closestTarget = null;
        float distance = 1f;
        foreach (RaycastHit t in TargetsInRange)
        {
            Transform target = t.transform;
            Vector3 Direction = target.position - transform.position;
            bool Facing = Vector3.Dot(Direction.normalized, transform.forward) > FOV; //Make sure Sonic is facing the target
            float TargetDistance = (Direction.sqrMagnitude / Radius) / Radius;
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(target.position); //Get the target's screen position
            bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1; //Make sure the target is on screen
            if (TargetDistance < distance && Facing && onScreen)
            {
                if (!Physics.Linecast(transform.position, target.position, BlockingLayers))
                {
                    closestTarget = target;
                    distance = TargetDistance;
                }
            }
        }

        return closestTarget;
    }

    public void UpdateTargets()
    {
        ClosestTarget = null;
        ClosestTarget = GetClosestTarget(TargetLayer, MaxHomingDistance, FieldOfView);
    }

    public void UpdateHomingReticle()
    {
        //This is going to be used for setting the reticle's position and animation, among other things.
        if (ClosestTarget != null && ActiveTarget != ClosestTarget && !player.Grounded)
        {
            if (!Reticle.activeSelf)
                Reticle.SetActive(true);
            ReticleAnimator.SetTrigger("Target");
            animator.PlayTargetSound();
            ActiveTarget = ClosestTarget;
        }
        if (ClosestTarget == null)
        {
            ActiveTarget = null;
        }
        if (ClosestTarget == null || ActiveTarget == null)
        {
            Reticle.SetActive(false);
        }

        if (Reticle.activeSelf)
        {
            ///For setting the reticle's position, we simply convert the target's screen position into canvas space.
            Vector2 IconPos = Camera.main.WorldToScreenPoint(ActiveTarget.position);
            Vector2 CanvasPos = new Vector2();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Hud, IconPos, null, out CanvasPos);
            ReticleTransform.localPosition = CanvasPos;
        }
    }
}
#region Action Classes

#endregion
