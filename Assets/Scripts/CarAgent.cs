// SerialID: [1ba2ce2c-2b2a-4e6d-9764-8ce1f38e28f0]
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;

public class CarAgent : Agent
{
    private CarController Controller { get; set; }

    private Vector3 StartPosition { get; set; }
    private Quaternion StartRotation { get; set; }

    public float DriveTime { get; set; }
    public float DriveTimeLimit { get; set; }
    public float MaxDistance { get; set; }
    public float MaxRotation { get; set; }
    public float MaxHealth { get; set; }
    [SerializeField] private float Health;

    private Rigidbody CarRb { get; set; }

    [SerializeField] private Text statusText = null;
    private Text StatusText { get { return statusText; } }

    private MeshGenerator MeshGen{ get; set; }

    public GameObject FrontRight;
    public GameObject BackRight;
    public GameObject FrontLeft;
    public GameObject BackLeft;

    private MyWheel frontRightWheel;
    private MyWheel backRightWheel;
    private MyWheel frontLeftWheel;
    private MyWheel backLeftWheel;
    private GaEnvironment ga;

    private void Awake() {
        // 車の制御コントローラーを取得
        Controller = GetComponent<CarController>();

        CarRb = GetComponent<Rigidbody>();

        frontRightWheel = FrontRight.GetComponent<MyWheel>();
        backRightWheel = BackRight.GetComponent<MyWheel>();
        frontLeftWheel = FrontLeft.GetComponent<MyWheel>();
        backLeftWheel = BackLeft.GetComponent<MyWheel>();

        MeshGen = GetComponentInChildren<MeshGenerator>();
    }

    /// <summary>
    /// 開始時に呼び出される初期化処理
    /// </summary>
    private void Start() {
        StartPosition = transform.position;
        StartRotation = transform.rotation;
        DriveTime = 0;
        DriveTimeLimit = 0;
        MaxDistance = 0;
        MaxRotation = 0;
        MaxHealth = 10;
        Health = MaxHealth;
        CarRb.isKinematic = false;

        /*
        StreamWriter file = new StreamWriter(@"test/record.csv", false, Encoding.UTF8);
        file.WriteLine(string.Format("{0},{1},{2},{3}", "Generation", "Best Record", "Best this gen", "Average"));
        Console.WriteLine("ファイルの作成");
        file.Close();
        */

        GameObject now_env = GameObject.Find("Environment");
        ga = now_env.GetComponent<GaEnvironment>();
    }

    // 新しい固体の初期化
    public override void AgentReset() {
        Controller.Stop();
        CarRb.ResetInertiaTensor();
        transform.position = StartPosition;
        transform.rotation = StartRotation;
        DriveTime = 0;
        MaxDistance = 0;
        MaxRotation = 0;
        Health = MaxHealth;

        SetFitness(0);
        SetDistance(0);

        CarRb.isKinematic = false;
    }

    public override void Stop() {
        Controller.Stop();
    }

    // Agentの更新. 終了判定と報酬の更新
    public override void AgentUpdate() {
        DriveTime += Time.fixedDeltaTime;
        
        // 最大到達距離を更新できないとき体力が減る
        if(MaxDistance + 0.01 < CarRb.transform.position.x){//ちょっと進んだ
            Health += 3*Time.fixedDeltaTime;
            Health = Mathf.Min(Health,MaxHealth);
        }else{
            Health -= Time.fixedDeltaTime;
        }

        MaxDistance = Mathf.Max(MaxDistance,CarRb.transform.position.x);

        if(StatusText != null) {
            StatusText.text = "Max Distance : " + MaxDistance + "\nHealth : " + Health + "\n";
        }

        // ゴール時に終了処理
        if(DriveTime > 50) {
            Controller.Stop();
            Done();
            Quaternion zRotation = CarRb.transform.rotation;
            Quaternion sRotation = StartRotation;
            MaxRotation = (zRotation.eulerAngles.z - sRotation.eulerAngles.z) / 90;
            if(ga.RollOverAffectFitness){
                AddFitness(MaxDistance - MaxRotation);
            }else{
                AddFitness(MaxDistance);
            }
            AddDistance(MaxDistance);
            return;
        }

        if(Health < 0) {
            Controller.Stop();
            Done();
            Quaternion zRotation = CarRb.transform.rotation;
            Quaternion sRotation = StartRotation;
            MaxRotation = (zRotation.eulerAngles.z - sRotation.eulerAngles.z) / 90;
            if(ga.RollOverAffectFitness){
                AddFitness(MaxDistance - MaxRotation);
            }else{
                AddFitness(MaxDistance);
            }
            AddDistance(MaxDistance);
            return;
        }
    }

    // Geneに対応した形状に変形
    public override void ApplyGene(Gene gene){
        int vertex = (gene.data.Count - 6)/2;
        List<float> BodyRadius = new List<float>();
        List<float> BodyAngle = new List<float>();
        for(int i = 0;i < vertex;i++){
            BodyRadius.Add(gene.data[6+i]);
            BodyAngle.Add(gene.data[vertex+6+i]);
        }
        MeshGen.CreateBody(BodyRadius,BodyAngle);
        int FrontPos = (int)gene.data[0];
        int BackPos = (int)gene.data[1];
        float FrontRadius = BodyRadius[FrontPos];
        float FrontAngle = BodyAngle[FrontPos];
        float BackRadius = BodyRadius[BackPos];
        float BackAngle = BodyAngle[BackPos];

        float FrontScale = gene.data[2];
        float BackScale = gene.data[3];
        float FT = gene.data[4];
        float BT = gene.data[5];

        frontRightWheel.ChangeWheel(FrontRadius,FrontAngle,FrontScale,1.0f,FT);
        frontLeftWheel.ChangeWheel(FrontRadius,FrontAngle,FrontScale,-1.0f,FT);

        backRightWheel.ChangeWheel(BackRadius,BackAngle,BackScale,1.0f,BT);
        backLeftWheel.ChangeWheel(BackRadius,BackAngle,BackScale,-1.0f,BT);
    }
}

