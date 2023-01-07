using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [HideInInspector] public InputHandler a_input;
    [HideInInspector] public Rigidbody rigidBody;
    PlayerActions actions;
    [HideInInspector] public CapsuleCollider col;
    [SerializeField] Transform camera; //Reference to the main camera, so we can align our input correctly
    [SerializeField] [Range(0, 1)] float InputDeadZone = 0.1f; //Minimum amount of input needed for movement
    [HideInInspector] public Vector3 InputDir;
    [HideInInspector] public Vector3 GroundNormal;
    [HideInInspector] public float GroundAngle;
    [HideInInspector] public Vector3 GroundTangent; //Tangent of the current surface; this is used for camera orientation

    public LayerMask GroundLayer; //What layers are detected as solid ground
    public float GroundCheckDistance = 0.6f; //How far do we check for ground
    [SerializeField] AnimationCurve GroundCheckOverSpeed; //Modifies GroundCheckDistance so we can stick to the ground easier
    public Vector3 Gravity;
    [HideInInspector] public bool Grounded;
    [SerializeField] float MaxAngleDifference = 20f; //What's the biggest difference between angles that we can traverse
    [SerializeField] float NormalCorrectionSpeed = 5f; //How quickly does Sonic's orientation reset upon leaving the ground
    public Vector3 TransformNormal { get; set; } //Sonic's orientation

    [SerializeField] float AccelRate = 2.5f;
    [SerializeField] float MinDecelRate = 1.025f;
    [SerializeField] float MaxDecelRate = 1.05f;
    public float TopSpeed = 15f;
    [SerializeField] float SpeedFixOvershoot = 0.2f; //How far past GroundCheckDistance do we check for the high speed fix
    //Handling
    [SerializeField] float TurnSpeed = 1f;
    [SerializeField] AnimationCurve TurnSpeedOverSpeed; //How much TurnSpeed is applied based on Sonic's current speed
    [SerializeField] [Range(0, 1)] float GroundHandlingAmount = 1f;
    [SerializeField] [Range(0, 1)] float AirHandlingAmount = 0.5f;
    [SerializeField] float TurnSmoothing;
    [SerializeField] bool DecelerateInAir = false;
    [SerializeField] [Tooltip("Amount of horizontal speed needed to convert vertical speed")] float SpeedConversionDifference = 15f;
    Vector3 GroundPoint;

    //Slopes
    [SerializeField] float GroundStickSpeed = 10f;
    [SerializeField] float MaxStandingAngle = 30f;
    [SerializeField] float MaxAngle = 90f;
    [Tooltip ("Minimum angle for slope physics to be applied")] [SerializeField] float MinAngle = 15f;
    [Tooltip("How much force is added by Ground Angle")][SerializeField] AnimationCurve SlopeForceOverAngle;
    [Tooltip("How much force is added by Speed")] [SerializeField] AnimationCurve SlopeForceOverSpeed;
    [Tooltip("Drives if we are using UphillForce or DownhillForce")] [SerializeField] AnimationCurve SlopeForceOverDirection;
    [SerializeField] float RollingSlopeMultiplier = 1.5f;
    [SerializeField] float RollSteering = 1f;

    //Step Offsetting
    public float MaxStepHeight = 0.35f; //Highest step Sonic can adjust to
    public float MinStepHeight = 0.01f; //Lowest step Sonic can adjust to
    public float MinCheckDistance = 0.5f;
    public float MaxNormal = 0.95f;
    public float SpeedStepThreshold = 5;
    public bool InputLocked { get; set; }
    public bool RotationLocked { get; set; }
    public bool GravityLocked { get; set; }
    public float MaxFallingSpeed = 60f;
    public float SkidInputThreshold = -0.3f;
    public bool Crouching { get; set; }
    public bool Skidding { get; set; }
    float TurnRate;
    // Start is called before the first frame update
    void Start()
    {
        a_input = InputHandler.instance;
        rigidBody = GetComponent<Rigidbody>();
        GroundNormal = Vector3.up;
        col = GetComponent<CapsuleCollider>();
        actions = GetComponent<PlayerActions>();
        TurnRate = TurnSpeed;
        GravityLocked = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!InputLocked)
            SetInputDirection();

        bool CanSkid = PlayerActions.currentState is DefaultState;
        if (!CanSkid && Skidding)
        {
            Skidding = false;
        }
    }

    private void FixedUpdate()
    {
        //Adjust Ground Check Distance by speed if we are grounded and in Default State
        float GroundCheckMod = GroundCheckOverSpeed.Evaluate(rigidBody.velocity.magnitude / TopSpeed);
        float GroundCheck = GroundCheckDistance * (Grounded ? GroundCheckMod : 1);
        //Get if we are grounded
        if (Physics.Raycast(transform.position, -GroundNormal, out RaycastHit ground, (col.height / 2) + GroundCheck, GroundLayer) && CheckForPredictedGround(rigidBody.velocity, GroundNormal, Time.fixedDeltaTime, GroundCheck, 4))
        {
            if (!Grounded)
            {
                ConvertAirToGroundVelocity();
                Grounded = true;
            }
            GroundNormal = ground.normal;
            TransformNormal = Vector3.Slerp(TransformNormal, GroundNormal, Time.fixedDeltaTime * 5f);
            if (!InputLocked)
                HandleMovement();
            //Project velocity to ground normal
            rigidBody.velocity = Vector3.ProjectOnPlane(rigidBody.velocity, GroundNormal);
            if (PlayerActions.currentState is DefaultState)
            {
                //Set ground position so Sonic doesn't fly off
                rigidBody.position = ground.point + ground.normal * (col.height / 2);
                //AdjustToSteps();
            }
            if (rigidBody.velocity.magnitude > GroundStickSpeed)
            {
                SlopePrediction(Time.fixedDeltaTime);
            }
            SlopePhysics();

            //Here we will use Vector3.Cross to get the current ground tangent if Sonic is not on flat ground.
            GroundAngle = Vector3.Angle(GroundNormal, Vector3.up);
            GroundTangent = SurfaceTangent(GroundNormal, Gravity);
            Debug.DrawRay(transform.position, GroundTangent);

        } else
        {
            Grounded = false;
            if (Skidding) Skidding = false;
            GroundNormal = -Gravity.normalized;
            if (!InputLocked)
                HandleMovement();
            //Add gravity
            if (!GravityLocked) rigidBody.velocity += Gravity * Time.fixedDeltaTime;
            TransformNormal = Vector3.Slerp(TransformNormal, Vector3.up, Time.fixedDeltaTime * NormalCorrectionSpeed);

        }
        //Rotate Sonic to face ground normal and velocity
        if (!RotationLocked)
        {
            if (!HedgeMath.IsApproximate(rigidBody.velocity, Vector3.zero, 0.1f))
            {
                Vector3 LookDir = transform.forward;
                LookDir = Vector3.Slerp(transform.forward, Vector3.ProjectOnPlane(rigidBody.velocity, GroundNormal).normalized, Time.deltaTime * 20f);
                Quaternion Rotation = Quaternion.LookRotation(LookDir, TransformNormal);
                transform.rotation = Rotation;
            }
        }
    }
    /// <summary>
    /// In HandleMovement, we'll be splitting Sonic's current velocity into two different vectors (Planar and Air velocity) for the sake of adding acceleration,
    /// deceleration, steering, etc. After calculations, both vectors will be added together and set as Sonic's current velocity.
    /// </summary>
    void HandleMovement ()
    {
        //Get velocity and split it into Planar and Air
        Vector3 Velocity = rigidBody.velocity;
        HedgeMath.SplitPlanarVector(Velocity, GroundNormal, out Vector3 PlanarVelocity, out Vector3 AirVelocity);
        //Set up a movement vector to change Sonic's actual movement
        Vector3 MovementVector = PlanarVelocity;

        //Add acceleration if Input is above DeadZone
        if (InputDir.magnitude > InputDeadZone)
        {
            if (!Skidding)
            {
                if (!Crouching) //Here we'll be adding acceleration as long as Sonic is below top speed and not rolling.
                {
                    TurnRate = Mathf.Lerp(TurnRate, TurnSpeed, Time.fixedDeltaTime * TurnSmoothing); //Smooth out the handling
                    if (PlanarVelocity.magnitude < TopSpeed)
                        PlanarVelocity += InputDir * AccelRate * Time.fixedDeltaTime;
                    float Handling = TurnRate * (Grounded ? GroundHandlingAmount : AirHandlingAmount); //Adjusting the turn speed depending on whether or not we are grounded
                    Handling *= TurnSpeedOverSpeed.Evaluate(PlanarVelocity.magnitude / TopSpeed); //Use the animation curve to lower Sonic's handling at higher speeds
                    MovementVector = Vector3.Lerp(PlanarVelocity, InputDir.normalized * PlanarVelocity.magnitude, Time.fixedDeltaTime * Handling);//Lerp to the current velocity to smooth out the turning
                }
                else //If he is rolling, we just use his current momentum and use player input solely for steering.
                {
                    TurnRate = Mathf.Lerp(TurnRate, RollSteering, Time.fixedDeltaTime * TurnSmoothing); //Smooth out the handling
                    Vector3 NewVelocity = Quaternion.FromToRotation(PlanarVelocity.normalized, InputDir.normalized) * PlanarVelocity;
                    float Handling = TurnRate * (Grounded ? GroundHandlingAmount : AirHandlingAmount);
                    Handling *= TurnSpeedOverSpeed.Evaluate(PlanarVelocity.magnitude / TopSpeed);
                    MovementVector = Vector3.Slerp(PlanarVelocity, NewVelocity, Time.fixedDeltaTime * Handling);
                }

                if (Vector3.Dot(InputDir, rigidBody.velocity.normalized) < -SkidInputThreshold && !Skidding)
                {
                    Skidding = true;
                }
            }  else
            {
                if (MovementVector.magnitude > 2f)
                    MovementVector = Vector3.MoveTowards(MovementVector, Vector3.zero, MaxDecelRate * Time.fixedDeltaTime);
                else
                    //Put Sonic at a dead stop to make sure there's no remaining velocity
                    MovementVector = Vector3.zero;
            }
        } else
        {
            if (!Skidding)
            {
                bool CanDecel = DecelerateInAir || !DecelerateInAir && Grounded;
                if (CanDecel && !Crouching)
                {
                    //Decelerate if no input
                    //For more natural deceleration, we are lerping between two deceleration rates. This way Sonic can stop faster at lower speeds, but takes a bit longer to decelerate from top speed or above.
                    float Decel = Mathf.Lerp(MaxDecelRate, MinDecelRate, MovementVector.magnitude / TopSpeed);
                    if (MovementVector.magnitude > 1f)
                        MovementVector = Vector3.MoveTowards(MovementVector, Vector3.zero, Decel * Time.fixedDeltaTime);
                    else
                        //Put Sonic at a dead stop to make sure there's no remaining velocity
                        MovementVector = Vector3.zero;
                }
            }
        }

        if (Skidding && Vector3.Dot(InputDir, rigidBody.velocity.normalized) > SkidInputThreshold || MovementVector.sqrMagnitude < 0.1f || Crouching)
        {
            Skidding = false;
        }

        AirVelocity = Vector3.ClampMagnitude(AirVelocity, MaxFallingSpeed);
        //Reconstruct velocity
        Velocity = MovementVector + AirVelocity;
        rigidBody.velocity = Velocity;
    }

    void SlopePhysics()
    {
        //Get angle of slope
        GroundAngle = Vector3.Angle(GroundNormal, Vector3.up);
        //Fall off ground if we are in a loop and don't have enough speed
        if (rigidBody.velocity.magnitude < GroundStickSpeed && GroundAngle > MaxStandingAngle)
        {
            Grounded = false;
            rigidBody.position += transform.up * 0.2f;
        }

        ///This is the main slope physics bit, only applied if the ground angle is above a certain angle.
        ///We get the slope force by simply projecting the gravity onto the ground normal.
        ///Then we use the dot product of Sonic's velocity and the gravity direction to determine if he is moving uphill or not.
        ///If Sonic is moving uphill when not rolling, the force (as driven by an animation curve) will be lower at higher speeds,
        ///allowing Sonic to better stick to loops and other uphill slopes when moving faster. The downhill force remains consistent,
        ///so no matter the speed, Sonic always gains the same force when going downhill.
        if (GroundAngle > MinAngle)
        {
            bool Uphill = Vector3.Dot(rigidBody.velocity.normalized, Gravity) < 0;
            float GroundSpeedMod = SlopeForceOverSpeed.Evaluate((rigidBody.velocity.sqrMagnitude / TopSpeed) / TopSpeed); //Applies less uphill force when moving faster
            float RollingMod = Crouching ? RollingSlopeMultiplier : 1f;
            Vector3 SlopeForce = Vector3.ProjectOnPlane(Gravity, GroundNormal) * RollingMod * (Uphill ? GroundSpeedMod : 1f);
            rigidBody.velocity += SlopeForce * Time.fixedDeltaTime;
        }
    }

    /// <summary>
    /// This function serves to more or less fix Sonic's velocity and position based on a predicted position and direction,
    /// done using a series of raycasts along Sonic's current velocity by the delta time. This is so he can better stick to
    /// slopes and have less trouble traversing lower poly terrains.
    /// </summary>
    void HighSpeedFix(float dt)
    {
        Vector3 PredictedPosition = rigidBody.position;
        Vector3 PredictedNormal = GroundNormal;
        Vector3 PredictedVelocity = rigidBody.velocity;
        int steps = 8;
        int i;
        for (i = 0; i < steps; i++)
        {
            PredictedPosition += PredictedVelocity * dt / steps;
            if (Physics.Raycast(PredictedPosition, -PredictedNormal, out RaycastHit pGround, GroundCheckDistance + SpeedFixOvershoot, GroundLayer))
            {
                if (Vector3.Angle (PredictedNormal, pGround.normal) < MaxAngleDifference)
                {
                    Debug.DrawRay(PredictedPosition, -PredictedNormal, Color.green);
                    PredictedPosition = pGround.point + pGround.normal * 0.5f;
                    PredictedVelocity = Quaternion.FromToRotation(GroundNormal, pGround.normal) * PredictedVelocity;
                    PredictedNormal = pGround.normal;
                } else
                {
                    Debug.DrawRay(PredictedPosition, -PredictedNormal, Color.red);
                    i = -1;
                    break;
                }
            } else
            {
                Debug.DrawRay(PredictedPosition, -PredictedNormal, Color.red);
                i = -1;
                break;
            }
        }
        if (i >= steps)
        {
            GroundNormal = PredictedNormal;
            rigidBody.position = Vector3.MoveTowards(rigidBody.position, PredictedPosition, dt);
        }
    }

    /// <summary>
    /// By Greedy
    /// 
    /// This Function is made so that the character doesnt get clipped inside geometry giving him an extra layer of prediction and avoiding weird speed jumps due to colision solving
    /// </summary>
    void SlopePrediction(float dt)
    {
        float LowerValue = 0.43f;
        Vector3 PredictedPosition = rigidBody.position + (-GroundNormal * LowerValue); //reducing Height to get closer to the ground
        Vector3 PredictedNormal = GroundNormal;
        Vector3 PredictedVelocity = rigidBody.velocity;
        float SpeedFrame = rigidBody.velocity.magnitude * dt; //actually a good idea to cache this call at the start of fixed update so you dont have to do calc everytime (the velocity.mag)
        float LerpJump = 0.015f;

        Debug.DrawRay(PredictedPosition, PredictedVelocity.normalized * SpeedFrame * 1.3f, Color.red, 5, true);
        if (!Physics.Raycast(PredictedPosition, PredictedVelocity.normalized, out RaycastHit pGround, SpeedFrame * 1.3f, GroundLayer)) { HighSpeedFix(dt); return; } //if detects no slopes in the way go to Downforce check

        for (float lerp = LerpJump; lerp < MaxAngleDifference / 90; lerp += LerpJump) //increases lerp up until it breaks the angle limit
        {
            Debug.DrawRay(PredictedPosition, Vector3.Lerp(PredictedVelocity.normalized, GroundNormal, lerp) * SpeedFrame * 1.3f, Color.blue, 5, false);
            if (!Physics.Raycast(PredictedPosition, Vector3.Lerp(PredictedVelocity.normalized, GroundNormal, lerp), out pGround, SpeedFrame * 1.3f, GroundLayer)) //Go until Find a suitable position above Ground
            {
                lerp += LerpJump; //add an extra one for extra hoverness
                Debug.DrawRay(PredictedPosition + (Vector3.Lerp(PredictedVelocity.normalized, GroundNormal, lerp) * SpeedFrame * 1.3f) + (Vector3.right * 0.05f), -PredictedNormal, Color.yellow, 5, false);
                Physics.Raycast(PredictedPosition + (Vector3.Lerp(PredictedVelocity.normalized, GroundNormal, lerp) * SpeedFrame * 1.3f), -PredictedNormal, out pGround, GroundCheckDistance + SpeedFixOvershoot, GroundLayer); // hit the ground on the valid Position

                PredictedPosition = (PredictedPosition + Vector3.Lerp(PredictedVelocity.normalized, GroundNormal, lerp) * SpeedFrame) + (pGround.normal * LowerValue);
                PredictedVelocity = Quaternion.FromToRotation(GroundNormal, pGround.normal) * PredictedVelocity;
                GroundNormal = pGround.normal;
                rigidBody.position = Vector3.MoveTowards(rigidBody.position, PredictedPosition, dt);
                rigidBody.velocity = PredictedVelocity;
                break;
            }
        }
    }

    /// <summary>
    /// This function is pretty self explanatory, it converts Sonic's air velocity to ground velocity when landing.
    /// </summary>
    void ConvertAirToGroundVelocity()
    {
        if (Physics.Raycast (transform.position, rigidBody.velocity.normalized, out RaycastHit velocityFix, rigidBody.velocity.magnitude, GroundLayer))
        {
            //Check if the angle is good
            float NextGroundAngle = Vector3.Angle(velocityFix.normal, Vector3.up);
            if (NextGroundAngle <= MaxAngleDifference)
            {
                Vector3 FixedVelocity = Vector3.ProjectOnPlane(rigidBody.velocity, transform.up);
                FixedVelocity = Quaternion.FromToRotation(transform.up, velocityFix.normal) * FixedVelocity;
                rigidBody.velocity = FixedVelocity;
            }
        }

    }

    void SetInputDirection()
    {
        //Get the input as a Vector3
        float hInput = a_input.GetAxis("Horizontal");
        float vInput = a_input.GetAxis("Vertical");
        Vector3 RawInput = new Vector3(hInput, 0, vInput); //Horizontal Input controls left and right, Vertical Input controls forward and backward
        RawInput = Vector3.ClampMagnitude(RawInput, 1); //Clamp RawInput's magnitude to 1 so it doesn't double up when holding diagonally on the stick

        //Align RawInput to Camera Direction and Ground Normal
        Vector3 TransformedInput = Quaternion.FromToRotation(camera.up, GroundNormal) * (camera.rotation * RawInput);
        //Project TransformedInput to the current ground normal to make sure it's aligning correctly
        TransformedInput = Vector3.ProjectOnPlane(TransformedInput, GroundNormal);
        //Set Input Direction to normalized TransformedInput with RawInput's magnitude
        InputDir = TransformedInput.normalized * RawInput.magnitude;

        //Debugging, to make sure we got it all right
        Debug.DrawRay(transform.position, InputDir, Color.cyan);
    }

    /// <summary>
    /// Predicts Sonic's next position and checks if he will be grounded at that position.
    /// </summary>
    /// <param name="Velocity">Sonic's current velocity</param>
    /// <param name="Normal">The normal vector to check along</param>
    /// <param name="deltaTime">Delta time of check</param>
    /// <param name="distance">How far below Sonic to check for ground</param>
    /// <param name="steps">How many checks will be performed</param>
    /// <returns></returns>
    public bool CheckForPredictedGround (Vector3 Velocity, Vector3 Normal, float deltaTime, float distance, int steps)
    {
        bool WillBeGrounded = false;
        //Much like the High Speed Fix, we'll be doing a series of raycasts that predict Sonic's next position so we can check if he will still be grounded in that position.
        //If all the checks return true, this will return true. If one of them returns false, this will return false and Sonic will no longer be grounded.
        //This is to prevent Sonic from snapping to edges.
        Vector3 InitialVelocity = Velocity;
        Vector3 PredictedNormal = Normal;
        Vector3 PredictedPos = rigidBody.position;
        for (int i = 0; i < steps; i++)
        {
            PredictedPos += Velocity * deltaTime / steps;
            if (Physics.Raycast(PredictedPos, -PredictedNormal, out RaycastHit hit, (col.height / 2) + distance, GroundLayer))
            {
                float Dot = Vector3.Dot(rigidBody.velocity, hit.normal);
                float MaxAngle = Dot < 0 ? MaxAngleDifference : 35f;
                if (Vector3.Angle(PredictedNormal, hit.normal) < MaxAngle)
                {
                    PredictedPos = hit.point + hit.normal * (col.height / 2);
                    PredictedNormal = hit.normal;
                    InitialVelocity = Quaternion.FromToRotation(GroundNormal, PredictedNormal) * InitialVelocity;
                    WillBeGrounded = true;
                }
            }
        }

        return WillBeGrounded;
    }

    void AdjustToSteps()
    {
        Vector3 InitialPosition = transform.position;
        Vector3 InitialVelocity = rigidBody.velocity;
        Vector3 FootPos = transform.position - transform.up * 0.5f;
        Vector3 LocalFootPos = transform.InverseTransformPoint(FootPos);
        Vector3 CheckPos = FootPos + transform.up * MaxStepHeight;
        Vector3 InputCheck = rigidBody.velocity.magnitude > SpeedStepThreshold ? InputDir : rigidBody.velocity.normalized;
        if (!Physics.Raycast(new Ray(CheckPos, InputCheck), MinCheckDistance, GroundLayer))
        {
            Debug.DrawLine(CheckPos, CheckPos + InputCheck * MinCheckDistance, Color.green);
            CheckPos += InputCheck * MinCheckDistance;
            if (Physics.Raycast(CheckPos, -transform.up, out RaycastHit hit, MaxStepHeight, GroundLayer))
            {
                Debug.DrawLine(CheckPos, hit.point);
                bool NormalCheck = Vector3.Dot(GroundNormal, hit.normal) >= MaxNormal;
                if (NormalCheck)
                {
                    Vector3 LocalStepPos = transform.InverseTransformPoint(hit.point);
                    float Difference = LocalStepPos.y - LocalFootPos.y;
                    Debug.Log("Step Difference:" + Difference);
                    if (Difference > MinStepHeight)
                    {
                        //transform.position += hit.normal * Difference;
                        rigidBody.MovePosition(rigidBody.position + hit.normal * Difference + InitialVelocity * Time.fixedDeltaTime);
                        rigidBody.velocity = Vector3.ProjectOnPlane(InitialVelocity, hit.normal);
                    }
                }
            }
        }
        else
        {
            Debug.DrawLine(CheckPos, CheckPos + InputCheck * MinCheckDistance, Color.red);
        }
    }

    Vector3 SurfaceTangent (Vector3 normal, Vector3 gravity)
    {
        Vector3 v1 = Vector3.Cross(normal, gravity);
        Vector3 Tan = Vector3.Cross(v1, normal);
        return Tan.normalized;
    }

    public IEnumerator LockInput (float duration)
    {
        InputLocked = true;
        yield return new WaitForSeconds(duration);
        InputLocked = false;
    }

    public IEnumerator LockGravity (float duration)
    {
        GravityLocked = true;
        yield return new WaitForSeconds(duration);
        GravityLocked = false;
    }
}
