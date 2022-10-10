using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;


public class RollerAgent : Agent
{
    public Transform target;

    private Rigidbody _rigidBody;

    private readonly int _forceLevel = 8;

    public override void Initialize()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// エピソード開始時に呼ばれる
    /// </summary>
    public override void OnEpisodeBegin()
    {
        // RollerAgentが床から落下しているとき
        if (this.transform.localPosition.y < 0)
        {
            // RollerAgentの位置と速度をリセット
            _rigidBody.angularVelocity = Vector3.zero;
            _rigidBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3(0.0f, 0.5f, 0.0f);
        }

        // Targetの位置をリセット
        target.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
    }

    /// <summary>
    /// 観察取得時に呼ばれる
    /// </summary>
    /// <param name="sensor"></param>
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(target.localPosition);  //TargetのXYZ座標
        sensor.AddObservation(this.transform.localPosition);  // RollerAgentのXYZ座標
        sensor.AddObservation(_rigidBody.velocity.x);  // RollerAgentのX速度
        sensor.AddObservation(_rigidBody.velocity.z);  // RollerAgentのZ速度
    }

    /// <summary>
    /// 行動実行時に呼ばれる
    /// </summary>
    /// <param name="actions"></param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        // RollerAgentに力を加える
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        _rigidBody.AddForce(controlSignal * _forceLevel);

        // RollerAgentがTargetの位置に到着したとき
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, target.localPosition);
        if (distanceToTarget < 1.42f)
        {
            AddReward(1.0f);
            EndEpisode();
        }

        // RollerAgentが床から落ちたとき
        if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }

    /// <summary>
    /// フューリスティックモードの行動決定時に呼ばれる
    /// </summary>
    /// <param name="actionsOut"></param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        actionsOut.ContinuousActions.Array.SetValue(Input.GetAxis("Horizontal"), 0);
        actionsOut.ContinuousActions.Array.SetValue(Input.GetAxis("Vertical"), 1);
    }
}
