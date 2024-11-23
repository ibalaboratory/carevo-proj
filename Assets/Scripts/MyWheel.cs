// SerialID: [1ba2ce2c-2b2a-4e6d-9764-8ce1f38e28f0]
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyWheel : MonoBehaviour
{
    public float torque;
    public Rigidbody rb;
    private Vector3 torque_vec;
    private HingeJoint WheelJoint;
    private Renderer renderer_;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        WheelJoint = GetComponent<HingeJoint>();
        renderer_ = GetComponent<Renderer>(); 
    }

    void Start()
    {
        rb.maxAngularVelocity = 200.0f;
        torque = 30.0f;
        torque_vec = new Vector3(0,0,-torque);
    }

    void FixedUpdate()
    {
        // タイヤを一定のトルクで回転
        rb.AddTorque(torque_vec);
    }

    // Change wheel pos and size
    public void ChangeWheel(float radius, float angle,float scale,float z,float T){
        // タイヤの位置(jointの固定位置)の変更
        WheelJoint.connectedAnchor = 
            new Vector3(radius*Mathf.Cos(-angle), radius*Mathf.Sin(-angle),z);
        // タイヤサイズ変更
        float C = 1.0f;
        transform.localScale = new Vector3(C*scale,0.2f,C*scale);
        // トルクとその大きささに合わせてタイヤの色を変更
        torque = T;
        torque_vec = new Vector3(0,0,-torque);
        byte c = (byte)(255*Mathf.Min(1.0f,T/40.0f));
        renderer_.material.color = new Color32(70,c,220,1);
    }
}
