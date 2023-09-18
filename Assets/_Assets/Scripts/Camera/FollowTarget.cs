using System;
using System.Collections;
using System.Collections.Generic;
using FearProj.ServiceLocator;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform characterTransform; // Target to follow
    public Transform ghostTransform; // Target to follow
    public float smoothSpeed = 0.125f; // Speed of camera smoothing
    public Vector3 offset; // Offset from the target

    private Transform _target;

    private void Start()
    {
        _target = ghostTransform;
        var tickManager = ServiceLocator.Get<IServiceTickManager>();
        tickManager.PreTick += PreTick_SwitchToCreature;
        tickManager.PostTick += PostTick_SwitchToGhost;
    }

    void FixedUpdate()
    {
        Vector3 desiredPosition = _target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    void PreTick_SwitchToCreature()
    {
        _target = characterTransform;
    }

    void PostTick_SwitchToGhost()
    {
        _target = ghostTransform;
    }
}
