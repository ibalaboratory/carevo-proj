// SerialID: [1ba2ce2c-2b2a-4e6d-9764-8ce1f38e28f0]
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
//地形の生成を行う。動的な変更を行う際もここを参照している。
public class GroundGenerator : MonoBehaviour
{
    //[SerializeField] private int seed = 0;
    [SerializeField] public float length = 80.0f;
    //private float bottom ;

    //private List<float> xpos = new List<float>();
    //private List<float> ypos = new List<float>();
    public GroundGene groundgene;
    [Header("Gene"), SerializeField] private GroundGeneOperator ggOperator = null;

    //private int prev_gen = 0;
    //private GaEnvironment ga;

    void Start()
    {
        //MakeGroundData();
        groundgene = ggOperator.Init(length, 0);
        CreateMesh();
        //GameObject now_env = GameObject.Find("Environment");
        //ga = now_env.GetComponent<GaEnvironment>();
    }

    /*
    public void ChangeLandscape(){
        //3世代毎に変わる
        if (ga.Generation > prev_gen + 2){
            seed += 1;
            //MakeNextGroundData();
            groundgene = ggOperator.Init(length, seed);
            CreateMesh();
            prev_gen = ga.Generation;
        }
    }
    */

    /*
    private void MakeGroundData(){
        System.Random rnd = new System.Random(seed);
        // 初期地点は固定
        float min_y = 0.0f;
        xpos.Add(0.0f);
        xpos.Add(6.0f);
        ypos.Add(0.0f);
        ypos.Add(0.0f);
        // x座標がlengthを超える程度まで地面を伸ばす
        // 直前の頂点に対してdx,dyを与えて座標を決定する。
        while(xpos[ypos.Count-1] < length){
            // 徐々に道を険しくしていく
            float r = 0.3f + xpos[ypos.Count-1]/length*0.7f; 
            float dx = 0.5f;
            float dy = ((float)rnd.NextDouble() - 0.5f) * r;
            xpos.Add(xpos[xpos.Count-1]+dx);
            ypos.Add(ypos[ypos.Count-1]+dy);
            min_y = Mathf.Min(min_y,ypos[ypos.Count-1]);
        }
        xpos.Add(xpos[xpos.Count-1]+0.001f);
        ypos.Add(ypos[ypos.Count-1]+5);
        xpos.Add(xpos[xpos.Count-1]+1);
        ypos.Add(ypos[ypos.Count-1]);
        bottom = min_y - 3.0f;
    }
    */

    /*
    public void MakeNextGroundData(){
        xpos = new List<float>();
        ypos = new List<float>();
        bottom = 0.0f;

        System.Random rnd = new System.Random(seed);
        // 初期地点は固定
        float min_y = 0.0f;
        xpos.Add(0.0f);
        xpos.Add(6.0f);
        ypos.Add(0.0f);
        ypos.Add(0.0f);
        // x座標がlengthを超える程度まで地面を伸ばす
        // 直前の頂点に対してdx,dyを与えて座標を決定する。
        while(xpos[ypos.Count-1] < length){
            // 徐々に道を険しくしていく
            float r = 0.3f + xpos[ypos.Count-1]/length*0.7f; 
            float dx = 0.5f;
            float dy = ((float)rnd.NextDouble() - 0.5f) * r;
            xpos.Add(xpos[xpos.Count-1]+dx);
            ypos.Add(ypos[ypos.Count-1]+dy);
            min_y = Mathf.Min(min_y,ypos[ypos.Count-1]);
        }
        xpos.Add(xpos[xpos.Count-1]+0.001f);
        ypos.Add(ypos[ypos.Count-1]+5);
        xpos.Add(xpos[xpos.Count-1]+1);
        ypos.Add(ypos[ypos.Count-1]);
        bottom = min_y - 3.0f;
    }
    */

    public void CreateMesh(){
        var xpos = groundgene.xpos;
        var ypos = groundgene.ypos;
        var bottom = groundgene.bottom;

        int N = xpos.Count;
        int z = 2;

        Vector3[] vertices = new Vector3[24*(N-1)+12];
        int[] triangles = new int[24*(N-1)+12];

        // 頂点の座標
        // 側面、上面、底面
        for(int i = 0;i < N-1;i++){
            float x1 = xpos[i];
            float x2 = xpos[i+1];
            float y1 = ypos[i];
            float y2 = ypos[i+1];
            Vector3[] v = new Vector3[]{
                new Vector3(x1,y1,-z),
                new Vector3(x1,bottom,-z),
                new Vector3(x1,bottom,z),
                new Vector3(x1,y1,z),
                new Vector3(x2,y2,-z),
                new Vector3(x2,bottom,-z),
                new Vector3(x2,bottom,z),
                new Vector3(x2,y2,z)
            };
            for(int j = 0;j < 4;j++){
                vertices[24*i+6*j] = v[(0+j)%4];
                vertices[24*i+6*j+1] = v[(0+j)%4+4];
                vertices[24*i+6*j+2] = v[(1+j)%4];
                vertices[24*i+6*j+3] = v[(1+j)%4];
                vertices[24*i+6*j+4] = v[(0+j)%4+4];
                vertices[24*i+6*j+5] = v[(1+j)%4+4];
            }
        }
        // 前後の面
        vertices[24*(N-1)] = new Vector3(xpos[0],ypos[0],-z);
        vertices[24*(N-1)+1] = new Vector3(xpos[0],bottom,-z);
        vertices[24*(N-1)+2] = new Vector3(xpos[0],bottom,z);
        vertices[24*(N-1)+3] = new Vector3(xpos[0],bottom,z);
        vertices[24*(N-1)+4] = new Vector3(xpos[0],ypos[0],z);
        vertices[24*(N-1)+5] = new Vector3(xpos[0],ypos[0],-z);

        vertices[24*(N-1)+6] = new Vector3(xpos[N-1],ypos[N-1],-z);
        vertices[24*(N-1)+7] = new Vector3(xpos[N-1],bottom,z);
        vertices[24*(N-1)+8] = new Vector3(xpos[N-1],bottom,-z);
        vertices[24*(N-1)+9] = new Vector3(xpos[N-1],bottom,z);
        vertices[24*(N-1)+10] = new Vector3(xpos[N-1],ypos[N-1],-z);
        vertices[24*(N-1)+11] = new Vector3(xpos[N-1],ypos[N-1],z);

        for(int i = 0;i <24*(N-1)+12;i++)triangles[i] = i;

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        mesh.RecalculateNormals();
    }
}
