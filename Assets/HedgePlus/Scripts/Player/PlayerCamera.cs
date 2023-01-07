using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public enum CameraState { Auto, Free, Fixed }
    public CameraState camState;
    [SerializeField] Transform FollowTarget;
    [SerializeField] PlayerController Player;
    [SerializeField] float MaxDistance = 4f;
    [SerializeField] float MaxHeight = 1f;
    [SerializeField] float YOffset = 0.1f;
    [SerializeField] float PositionDamping = 0.1f;
    [SerializeField] float TransitionSpeed = 5f;
    [SerializeField] float RotationSpeed = 15f;
    [SerializeField] float MinAngle = 10f;
    [SerializeField] [Range(0, 1)] float MinDot = 0.5f;
    [SerializeField] float CollisionRadius = 0.1f;
    [SerializeField] LayerMask CollidesWith;
    [Header("Camera Control")]
    [SerializeField] string GamepadX;
    [SerializeField] string GamepadY;
    [SerializeField] string MouseX;
    [SerializeField] string MouseY;
    [SerializeField] float OrbitSpeed;
    [Range(0, 10)] public float MouseSensitivity = 1f;
    [Range(0, 10)] public float StickSensitivity = 1f;
    float XInput;
    float YInput;


    Vector3 LookDir;
    Vector3 FollowDir;
    Vector3 TargetOffset;
    Vector3 Normal;
    Vector3 TargetNormal;
    Quaternion LookRot;
    [HideInInspector] public Vector3 TargetPosition;
    public bool UpdateCameraOrientation = true;
    bool SmoothPosition;

    Vector3 PositionDamp;
    // Start is called before the first frame update
    void Start()
    {
        //Upon loading, we will be setting the camera to its most default position and direction.
        TargetOffset = FollowTarget.position + FollowTarget.up * YOffset;
        LookDir = TargetOffset - transform.position;
        LookDir.Normalize();
        SmoothPosition = true;
    }

    // Update is called once per frame
    void Update()
    {
        ///In Update we will be adding an offset to the player so they are not dead center.
        ///Then while we want to update the orientation (i.e. not in a camera trigger) we will simply be setting the camera's look
        ///direction to the direction from the camera to Sonic, and normalizing it. Then we smooth out the direction by
        ///simply lerping it to Look Dir projected onto Sonic's up vector.
        TargetOffset = FollowTarget.position + FollowTarget.up * YOffset;
        if (UpdateCameraOrientation)
        {
            LookDir = TargetOffset - transform.position;
            LookDir.Normalize();

            XInput = (Input.GetAxis(GamepadX) * StickSensitivity) + (Input.GetAxis(MouseX) * MouseSensitivity);
            YInput = Input.GetAxis(GamepadY) + Input.GetAxis(MouseY);

            LookDir = Quaternion.AngleAxis(OrbitSpeed * XInput * Time.deltaTime, FollowTarget.up) * LookDir;
        }
        if (Player.Grounded)
        {
            float VelocityDot = Vector3.Dot(Player.rigidBody.velocity.normalized, Player.GroundTangent);
            if (Player.GroundAngle >= MinAngle)
            {
                if (VelocityDot > MinDot || VelocityDot < -MinDot)
                {
                    TargetNormal = Player.GroundNormal;
                } else
                {
                    TargetNormal = Vector3.up;
                }
            } else
            {
                TargetNormal = Vector3.up;
            }
        } else
        {
            TargetNormal = Vector3.up;
        }

        Normal = Vector3.Slerp(Normal, TargetNormal, Time.deltaTime * RotationSpeed);

        FollowDir = Vector3.ProjectOnPlane(LookDir, Normal);

    }

    private void LateUpdate()
    {
        switch (camState)
        {
            case CameraState.Auto:
                ///For Auto Camera, we are simply using the camera's FollowDir to position it, then using SmoothDamp to move it into position smoothly.
                ///We will also factor in Sonic's velocity for consistent positioning.
                ///Then we simply make it look at Sonic.
                if (SmoothPosition)
                {
                    TargetPosition = TargetOffset + FollowTarget.up * MaxHeight + -FollowDir * MaxDistance + Player.rigidBody.velocity * Time.fixedDeltaTime;
                    CameraCollision(TargetOffset, ref TargetPosition);
                    transform.position = Vector3.SmoothDamp(transform.position, TargetPosition, ref PositionDamp, PositionDamping);
                } else
                {
                    transform.position = TargetPosition;
                }
                
                LookRot = Quaternion.LookRotation(TargetOffset - transform.position, Normal);
                break;
            case CameraState.Fixed:
                ///For Fixed Camera, we just lerp the camera into the position we want and make it look at Sonic.
                transform.position = Vector3.Lerp(transform.position, TargetPosition, TransitionSpeed * Time.deltaTime);
                LookRot = Quaternion.LookRotation(TargetOffset - transform.position);
                break;
        }
        transform.rotation = LookRot;
    }

    /// <summary>
    /// Prevents the camera from clipping into walls.
    /// </summary>
    /// <param name="from">Target position</param>
    /// <param name="to">Camera target position</param>
    void CameraCollision (Vector3 from, ref Vector3 to)
    {
        //First we get the direction from Sonic to the target position, and use it to create
        //a wall offset by normalizing it and multiplying it by the collision radius.
        Vector3 CastDirection = from - to;
        Vector3 CastOffset = CastDirection.normalized * CollisionRadius;
        //Then we do a Linecast from Sonic to the target position to check if there is any geometry between the two.
        RaycastHit WallHit = new RaycastHit();
        if (Physics.Linecast(from, to, out WallHit, CollidesWith))
        {
            //If the Linecast returns true, we set the target position to the linecast point combined with the offset we created earlier.
            to = WallHit.point + CastOffset;
        }
    }

    public void RespawnCamera (Vector3 Position, Vector3 Forward)
    {
        ///What this is SUPPOSED to do is revert the camera to the most default position we can achieve.
        //camState = CameraState.Auto;
        SmoothPosition = false;
        UpdateCameraOrientation = true;
        TargetPosition = Position + FollowTarget.up * MaxHeight - FollowTarget.forward * MaxDistance;
        camState = CameraState.Auto;
        StartCoroutine("HardResetCamera");
    }

    public void SetLookDir (Vector3 Direction)
    {
        LookDir = Direction;
        Debug.Log("Set camera direction to " + Direction);
    }

    IEnumerator HardResetCamera()
    {
        ///This disables smooth camera movement for a single frame to hard reset it.
        transform.position = TargetPosition;
        yield return null;
        SmoothPosition = true;
    }




}
