using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class TestAgent : Agent {
    public Transform target;
    Rigidbody rb;
    // RayPerceptionSensor3D
    private RayPerceptionSensorComponent3D ray3;
    // RayPerceptionSensorの出力
    private RayPerceptionOutput ray3o;
    // RayPerceptionSensorの入力設定
    private RayPerceptionInput ray3i;
    // RayPerceptionSensorの出力を保存する配列
    private float[] raybuffer;
    // timerカウントフラグ
    private bool flag_count = false;
    // timer保存
    private float countup = 0.0f;
    // 制限時間 3s
    private float timelimit = 3.0f;

    public Transform EndTarget;

    public Transform spawnPoint;
    

    void Start() {
        rb = GetComponent<Rigidbody>();
        ray3 = GetComponent<RayPerceptionSensorComponent3D>();
        ray3o = new RayPerceptionOutput();
        //ray3o = ray3.RayPer
        ray3i = ray3.GetRayPerceptionInput();
        

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Debug.Log(sensor.GetName());
    }

    void Update()
    {
        // 時間経過するとマイナス報酬
        AddReward(-0.01f);
        ray3o = RayPerceptionSensor.Perceive(ray3i, false);
        
        //Debug.Log(ray3.RayLength); OK:12
        //Debug.Log(ray3.DetectableTags); NG
        //Debug.Log(ray3.DetectableTags.Count); OK:1
        //Debug.Log(ray3.GetCastType());  OK:Cast3D
        //Debug.Log(ray3.GetRayPerceptionInput());  NG
        //Debug.Log(ray3.GetRayPerceptionInput().DetectableTags); NG
        //Debug.Log(ray3.GetRayPerceptionInput().CastRadius); OK:0.5
        //Debug.Log(ray3.GetRayPerceptionInput().RayExtents(0));  ????
        //Debug.Log(ray3.RaysPerDirection); OK:3
        //Debug.Log(ray3.ObservationStacks); OK:1 
        //Debug.Log(ray3o.RayOutputs.Length); OK:7
        // ray3o.RayOutputs[0]はエージェント正面のRay情報
        //Debug.Log(ray3o.RayOutputs[0].ScaledCastRadius); OK:0.5
        //Debug.Log(ray3o.RayOutputs[0].HasHit); OK:true or false
        //Debug.Log(ray3o.RayOutputs[0].HitFraction); OK: ヒットした物体との距離を1で正規化したもの
        raybuffer = new float[42];
        ray3o.RayOutputs[0].ToFloatArray(0, 0, raybuffer);
        // 正面のRayがTargetにヒットしたらフラグ（raybuffer[0]）が”0”になり、正規化された距離（raybuffer[1]）が出てくる
        //Debug.Log(raybuffer[0]+" "+raybuffer[1]+" "+raybuffer[2]+" "+raybuffer[3]+" "+raybuffer[4]+" "+raybuffer[5]+" "+raybuffer[6]+" "+raybuffer[7]+" "+raybuffer[8]+" "+raybuffer[9]);
        //Debug.Log(ray3o.RayOutputs[0].HitTagIndex);
        if (ray3o.RayOutputs[0].HitTagIndex == 0 && raybuffer[1] < 1.0f)
        {
            if (!flag_count)
            {
                flag_count = true;
            }

            if (flag_count)
            {
                countup += Time.deltaTime;
                if (countup >= timelimit)
                {
                    AddReward(10.0f);
                    //countup = 0.0f;
                }
            }
        }
        else
        {
            flag_count = false;
            countup = 0.0f;
        }

        if (flag_count)
        {
            gameObject.GetComponent<Renderer>().material.color = new Color(0, 255, 0);
        }
        else
        {
            gameObject.GetComponent<Renderer>().material.color = new(0, 0, 255);
        }
        
    }
    

    // エピソード開始時の初期化処理
    public override void OnEpisodeBegin() {
        // エージェントが落下したとき
        if (transform.localPosition.y < 0) {
            // Agentの位置と速度をリセット
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;
            transform.localPosition = spawnPoint.localPosition;
        }

        countup = 0.0f;
    }

    // 行動実行時の処理
    public override void OnActionReceived(ActionBuffers actions) {
        Vector3 dirToGo = Vector3.zero;
        Vector3 rotateDir = Vector3.zero;
        int action = actions.DiscreteActions[0];
        /*
        action値＝
        0:移動なし
        1:前進
        2:後退
        3:右回転
        4:左回転
        */
        if (action == 1) dirToGo = transform.forward;
        if (action == 2) dirToGo = transform.forward * -1.0f;
        if (action == 3) rotateDir = transform.up * -1.0f;
        if (action == 4) rotateDir = transform.up;
        //Debug.Log("dirToGo"+dirToGo);
        //Debug.Log("rotateDir"+rotateDir);
        // エージェントを回転
        transform.Rotate(rotateDir, Time.deltaTime * 200f);
        // エージェントを前進・後退
        rb.AddForce(dirToGo * 0.4f, ForceMode.VelocityChange);

        // エージェントとターゲットの距離を計算
        float distanceToTarget = Vector3.Distance(
            transform.localPosition, target.localPosition);
        // エージェントとの距離が密接したとき
        if (distanceToTarget < 1.42f) {
            // プラスの報酬を与える
            AddReward(10.0f);
        }
        float distanceToEndTarget = Vector3.Distance(
            transform.localPosition, EndTarget.localPosition);
        // エージェントとの距離が密接したとき
        if (distanceToEndTarget < 1.42f)
        {
            // プラスの報酬を与える
            AddReward(10.0f);
            EndEpisode();
        }

        // エージェントが落下したときも
        if (this.transform.localPosition.y < -0.1f) {
            // エピソードを終了
            EndEpisode();
        }
    }

    // ヒューリスティックモードの行動決定時に呼ばれる
    public override void Heuristic(in ActionBuffers actionsOut) {
        var actions = actionsOut.DiscreteActions;
        actions[0] = 0;
        // 矢印キーによって、移動方向を設定する
        if (Input.GetKey(KeyCode.UpArrow)) actions[0] = 1;
        if (Input.GetKey(KeyCode.DownArrow)) actions[0] = 2;
        if (Input.GetKey(KeyCode.LeftArrow)) actions[0] = 3;
        if (Input.GetKey(KeyCode.RightArrow)) actions[0] = 4;
    }
}