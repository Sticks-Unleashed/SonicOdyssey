using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterVolume : MonoBehaviour
{
    [SerializeField] Transform Surface;
    [SerializeField] float WaterRunSpeed;
    [SerializeField] float UnderwaterDamping;
    [SerializeField] float UnderwaterGravity;
    Collider volCol;
    Transform _ply;
    Rigidbody _plyBody;
    PlayerController player;
    bool InWater;
    Vector3 Gravity;

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
        _ply = player.transform;
        _plyBody = player.GetComponent<Rigidbody>();
        volCol = GetComponent<Collider>();
    }

    private void Update()
    {
        if (!InWater)
        {
            volCol.isTrigger = _plyBody.velocity.magnitude < WaterRunSpeed;
        } else
        {
            volCol.isTrigger = true;
        }
    }

    private void FixedUpdate()
    {
        if (InWater)
        {
            if (_ply.position.y < Surface.position.y)
            {
                _plyBody.drag = UnderwaterDamping;
                player.Gravity = Vector3.up * UnderwaterGravity;
            } else
            {
                _plyBody.drag = 0;
                player.Gravity = Gravity;
                InWater = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            InWater = true;
            Gravity = player.Gravity;
        }
    }
}
