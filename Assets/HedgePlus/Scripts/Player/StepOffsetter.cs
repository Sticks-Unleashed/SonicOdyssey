using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepOffsetter : MonoBehaviour
{
    PlayerController Player;
    public float MaxStepHeight = 0.1f; //Highest step Sonic can adjust to
    public float MinStepHeight = 0.01f; //Lowest step Sonic can adjust to
    public float MinCheckDistance;
    public float MaxNormal = 0.95f;
    public float SpeedThreshold = 10f; //If Sonic is moving past this speed, we will use his velocity for checking. If not, we will use input.
    // Start is called before the first frame update
    void Start()
    {
        Player = GetComponent<PlayerController>();
    }

    private void FixedUpdate()
    {
        if (Player.Grounded)
        {
            AdjustToSteps();
        }
    }

    void AdjustToSteps()
    {
        Vector3 InitialPosition = transform.position;
        Vector3 InitialVelocity = Player.rigidBody.velocity;
        Vector3 FootPos = transform.position - transform.up * 0.5f;
        Vector3 LocalFootPos = transform.InverseTransformPoint(FootPos);
        Vector3 CheckPos = FootPos + transform.up * MaxStepHeight;
        Vector3 InputCheck = Player.InputDir;
        if (!Physics.Raycast(new Ray(CheckPos, InputCheck), MinCheckDistance, Player.GroundLayer))
        {
            Debug.DrawLine(CheckPos, CheckPos + InputCheck * MinCheckDistance, Color.green);
            CheckPos += InputCheck * MinCheckDistance;
            if (Physics.Raycast(CheckPos, -transform.up, out RaycastHit hit, MaxStepHeight, Player.GroundLayer))
            {
                Debug.DrawLine(CheckPos, hit.point);
                bool NormalCheck = Vector3.Dot(Player.GroundNormal, hit.normal) >= MaxNormal;
                if (NormalCheck)
                {
                    Vector3 LocalStepPos = transform.InverseTransformPoint(hit.point);
                    float Difference = LocalStepPos.y - LocalFootPos.y;
                    Debug.Log("Step Difference:" + Difference);
                    if (Difference > MinStepHeight)
                    {
                        Player.rigidBody.position += hit.normal * Difference;
                        Player.rigidBody.velocity = Vector3.ProjectOnPlane(InitialVelocity, hit.normal);
                    }
                }
            }
        } else
        {
            Debug.DrawLine(CheckPos, CheckPos + InputCheck * MinCheckDistance, Color.red);
        }
    }
}
