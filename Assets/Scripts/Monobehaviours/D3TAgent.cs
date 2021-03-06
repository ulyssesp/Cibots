﻿using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;

[RequireComponent(typeof(RaycastShooter))]
[RequireComponent(typeof(EnergyAgent))]
[RequireComponent(typeof(HealthAgent))]
[RequireComponent(typeof(RayPerception3D))]
[RequireComponent(typeof(Rigidbody))]
public class D3TAgent : EnemyAgent, IResettable
{
    public float InitialHealth;
    public float InitialEnergy;
    public float MoveSpeed = 3f;
    public float TurnSpeed = 300f;
    Rigidbody agentRb;
    HealthAgent HealthAgent;
    HealthAgent PlayerHealthAgent;
    EnergyAgent EnergyAgent;
    RaycastShooter RaycastShooter;
    private RayPerception3D rayPer;
    // Start is called before the first frame update
    public override void InitializeAgent()
    {
        base.InitializeAgent();
        agentRb = GetComponent<Rigidbody>();
        rayPer = GetComponent<RayPerception3D>();
        HealthAgent = GetComponent<HealthAgent>();
        EnergyAgent = GetComponent<EnergyAgent>();
        RaycastShooter = GetComponent<RaycastShooter>();

        PlayerHealthAgent = Player.GetComponent<HealthAgent>();

        FloatVariable health = ScriptableObject.CreateInstance<FloatVariable>();
        health.InitialValue = InitialHealth;
        health.RuntimeValue = InitialHealth;
        HealthAgent.Health = health;

        FloatVariable energy = ScriptableObject.CreateInstance<FloatVariable>();
        energy.InitialValue = InitialEnergy;
        energy.RuntimeValue = InitialEnergy;
        EnergyAgent.EnergyPool = energy;
    }

    public override void CollectObservations() {
        float rayDistance = 50f;
        float[] rayAngles = {0f, 20f, 90f, 160f, 45f, 135f, 70f, 110f, 180f };
        string[] detectableObjects = { "enemy", "wall", "player" };
        AddVectorObs(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
        Vector3 localVelocity = transform.InverseTransformDirection(agentRb.velocity);
        AddVectorObs(localVelocity.x);
        AddVectorObs(localVelocity.z);
        AddVectorObs(HealthAgent.Health.RuntimeValue);
        AddVectorObs(EnergyAgent.EnergyPool.RuntimeValue);
        AddVectorObs(PlayerHealthAgent.Health.RuntimeValue);
    }

    public override void AgentAction(float[] vectorAction, string textAction) {
        AddReward(-0.01f);
        if(Mathf.Clamp(vectorAction[2], -1, 1) > 0.5f) {
            string fireResult = RaycastShooter.Fire();
            Debug.Log(fireResult);
            switch(fireResult) {
                case "player":
                    AddReward(0.2f);
                    break;
                case "enemy":
                    AddReward(-0.001f);
                    break;
                // default:
                //     AddReward(-0.0001f);
                //     break;
            }
        }

        // if(!RaycastShooter.Ability.CanRun(EnergyAgent.EnergyPool)) {
        //     AddReward(-0.001f);
        // }

        if (Mathf.Abs(transform.position.x) > 19 || Mathf.Abs(transform.position.z) > 19) {
            AddReward(-0.05f);
            Done();
            Reset();
        }

        if (Mathf.Abs(transform.position.x) > 20 || Mathf.Abs(transform.position.z) > 20) {
            AddReward(-1f);
            Done();
            Reset();
        }

        if (PlayerHealthAgent.Health.RuntimeValue <= 0) {
            AddReward(1f);
            Done();
        }

        if (HealthAgent.Health.RuntimeValue < 0) {
            AddReward(-1f);
            Done();
            HealthAgent.Health.RuntimeValue = InitialHealth;
            EnergyAgent.EnergyPool.RuntimeValue = InitialEnergy;
            Reset();
        }

        Vector3 dirToGo = transform.forward * Mathf.Clamp(vectorAction[0], -0.6f, 1f);
        Vector3 rotateDir = transform.up * Mathf.Clamp(vectorAction[1], -1f, 1f);

        agentRb.AddForce(dirToGo * MoveSpeed, ForceMode.VelocityChange);
        transform.Rotate(rotateDir, Time.fixedDeltaTime * TurnSpeed);
    }

    public void Reset() {
        int pos = (int) Mathf.Floor(Random.Range(0, 5));
        switch(pos) {
            case 0:
                transform.position = new Vector3(19, transform.position.y, 0);
                break;
            case 1:
                transform.position = new Vector3(-19, transform.position.y, 0);
                break;
            case 2:
                transform.position = new Vector3(0, transform.position.y, 19);
                break;
            case 3:
                transform.position = new Vector3(0, transform.position.y, -19);
                break;
        }
        Done();
    }
}
