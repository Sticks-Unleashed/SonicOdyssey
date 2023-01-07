using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class DashRingActor : MonoBehaviour
{
    AudioSource _source;
    public float Force = 25f;
    public float InputLockDuration = 1f;
    public float GravityLockDuration = 1f;

    public bool DebugForce;
    Vector3 Gravity = new Vector3(0, -32f, 0);
    float Drag = 0.065f;

    private void Start()
    {
        _source = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Application.isEditor)
        {
            if (DebugForce)
            {
                if (InputLockDuration > 0)
                {
                    Vector3[] DebugTrajectoryPoints = PreviewTrajectory(transform.position, transform.forward * Force, Gravity, Drag, InputLockDuration);
                    for (int i = 1; i < DebugTrajectoryPoints.Length; i++)
                    {
                        Debug.DrawLine(DebugTrajectoryPoints[i - 1], DebugTrajectoryPoints[i]);
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _source.Play();
        }
    }

    static Vector3[] PreviewTrajectory(Vector3 position, Vector3 velocity, Vector3 gravity, float drag, float time)
    {
        float timeStep = Time.fixedDeltaTime;
        int iterations = Mathf.CeilToInt(time / timeStep);
        if (iterations < 2)
        {
            Debug.LogError("PreviewTrajectory (Vector3, Vector3, Vector3, float, float): Unable to preview trajectory shorter than Time.fixedDeltaTime * 2");
            return new Vector3[0];
        }
        Vector3[] path = new Vector3[iterations];
        Vector3 pos = position;
        Vector3 vel = velocity;
        path[0] = pos;
        float dragScale = Mathf.Clamp01(1.0f - (drag * timeStep));
        for (int i = 1; i < iterations; i++)
        {
            vel = vel + (gravity * timeStep);
            vel *= dragScale;
            pos = pos + (vel * timeStep);
            path[i] = pos;
        }
        return path;
    }
}
