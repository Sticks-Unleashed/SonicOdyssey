using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;
[ExecuteAlways]
public class MovingPlatformActor : MonoBehaviour
{
    Rigidbody _rb;
    BoxCollider col;
    public Transform FloorPos;
    public Spline spline;
    Vector3 startPos, endPos;
    public enum Axis { X, Y, Z };
    public Axis movementAxis;
    public float Distance = 1f;
    [Range(0, 1)] public float Position;
    public AnimationCurve MovementCurve;

    public float Speed;
    public float Delay;
    public float Rotation;
    bool Reversed;
    bool Moving;
    Vector3 TargetPosition;
    float SplinePos;
    PlayerController child;
    Vector3 Euler;
    Vector3 LocalChildPos;
    Vector3 prevPos;
    Vector3 curPos;
    Vector3 deltaPos;
    private void Start()
    {
        if (Application.isPlaying)
        {
            switch (movementAxis)
            {
                case Axis.X:
                    startPos = transform.parent.TransformPoint(new Vector3(-Distance, 0, 0));
                    endPos = transform.parent.TransformPoint(new Vector3(Distance, 0, 0));
                    break;
                case Axis.Y:
                    startPos = transform.parent.TransformPoint(new Vector3(0, -Distance, 0));
                    endPos = transform.parent.TransformPoint(new Vector3(0, Distance, 0));
                    break;
                case Axis.Z:
                    startPos = transform.parent.TransformPoint(new Vector3(0, 0, -Distance));
                    endPos = transform.parent.TransformPoint(new Vector3(0, 0, Distance));
                    break;
            }
            _rb = GetComponent<Rigidbody>();
            col = GetComponent<BoxCollider>();
            //Get starting position
            float PointDistance = Distance * 2f;
            float PlatformDistance = Vector3.Distance(transform.position, startPos);
            Position = PlatformDistance / PointDistance;

            curPos = _rb.position;
            prevPos = curPos;
            Moving = true;
        }
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            switch (movementAxis)
            {
                case Axis.X:
                    startPos = transform.parent.TransformPoint(new Vector3(-Distance, 0, 0));
                    endPos = transform.parent.TransformPoint(new Vector3(Distance, 0, 0));
                    break;
                case Axis.Y:
                    startPos = transform.parent.TransformPoint(new Vector3(0, -Distance, 0));
                    endPos = transform.parent.TransformPoint(new Vector3(0, Distance, 0));
                    break;
                case Axis.Z:
                    startPos = transform.parent.TransformPoint(new Vector3(0, 0, -Distance));
                    endPos = transform.parent.TransformPoint(new Vector3(0, 0, Distance));
                    break;
            }
        }
    }

    private void FixedUpdate()
    {
        if (Moving)
        {
            if (Reversed)
            {
                Position -= Time.fixedDeltaTime * Speed;
            }
            else
            {
                Position += Time.fixedDeltaTime * Speed;
            }
            if (Reversed && Position <= 0.01f || !Reversed && Position >= 0.999f)
            {
                StartCoroutine("SetPositionDelay");
            }

            curPos = _rb.position;
            if (prevPos != curPos)
            {
                deltaPos = curPos - prevPos;
                deltaPos.y = 0;
                prevPos = curPos;
            }

        }
        TargetPosition = Vector3.Lerp(startPos, endPos, MovementCurve.Evaluate(Position));
        Euler.y += Rotation * Time.fixedDeltaTime;
        if (child != null) LocalChildPos = transform.InverseTransformPoint(child.rigidBody.position);
        _rb.MovePosition(TargetPosition);
        _rb.MoveRotation(Quaternion.Euler(Euler));
        StartCoroutine("UpdateChildren");
    }

    IEnumerator SetPositionDelay()
    {
        Moving = false;
        yield return new WaitForSeconds(Delay);
        Reversed = !Reversed;
        Moving = true;
    }

    IEnumerator UpdateChildren()
    {
        yield return new WaitForFixedUpdate();
        if (child != null)
        {
            child.transform.position += deltaPos * Time.fixedDeltaTime;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(startPos, 0.2f);
        Gizmos.DrawWireSphere(startPos, 0.2f);
        Debug.DrawLine(startPos, endPos);
    }

    private void OnCollisionEnter(Collision collision)
    {
        collision.transform.SetParent(transform);
        if (collision.transform.position.y > transform.position.y)
        {
            if (collision.gameObject.TryGetComponent<PlayerController>(out PlayerController player))
            {
                child = player;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        collision.transform.SetParent(null);
        child = null;
    }

    public float GetClosestPos(Vector3 Position)
    {
        float CurrentDist = 9999999f;
        for (float n = 0; n < spline.Length; n += Time.deltaTime * 10f)
        {
            float dist = ((spline.GetSampleAtDistance(n).location + spline.transform.position) - Position).sqrMagnitude;
            if (dist < CurrentDist)
            {
                CurrentDist = dist;
                SplinePos = n;
            }

        }
        return SplinePos;
    }
}
