// SerialID: [1ba2ce2c-2b2a-4e6d-9764-8ce1f38e28f0]
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[CreateAssetMenu]
public class CarGeneOperator : GeneOperator
{
    // 変数の範囲
    // 必要に応じてこの部分を変更してもよいが、GAのパラメータではなく
    // 問題側のパラメーターであることに注意
    
    // body size
    protected float MinRadius = 0.7f;
    protected float MaxRadius = 1.6f;

    // wheel size
    protected float MinWheelSize = 0.6f;
    protected float MaxWheelSize = 1.8f;
    
    // power
    protected float MinTorque = 2.0f;
    protected float MaxTorque = 30.0f;

    protected int VertexesNumber = 12;

    [Header("Mutation Variables"),SerializeField] private float PosProb = 0.5f;
    [SerializeField] private float RadiusProb = 0.5f;
    [SerializeField] private float RadiusMutationSize = 0.5f;

    [SerializeField] private float AngleProb = 0.5f;
    [SerializeField] private float AngleMutationSize = 0.5f;

    [SerializeField] private float WheelSizeProb = 0.5f;
    [SerializeField] private float WheelSizeMutationSize = 0.5f;

    [SerializeField] private float TorqueProb = 0.5f;
    [SerializeField] private float TorqueMutationSize = 0.5f;
    protected float theta;
    protected float eps = 0.00001f;

    void OnEnable()
    {
        // 角度に関する定数
        theta = Mathf.PI/VertexesNumber;
    }

     /*
        Gene.data[i] は以下の各要素に対応している
        0 : 前輪位置　([0,頂点数)、タイヤが付く頂点の番号)
        1 : 後輪位置
        2 : 前輪大きさ 
        3 : 後輪大きさ
        4 : 前輪トルク (タイヤには常にトルクが与えられ、これによって前進する)
        5 : 後輪トルク
        6~5+VertexesNumber  : 車体の各頂点の中心からの距離
        6+VertexesNumber~5+2*VertexesNumber : 車体の各頂点の角度
        

     - 車体について
    車体の形は多角柱を横にしたもので表現される
    底面の多角形の頂点の位置を極座標形式で指定する
    頂点数を N, t = PI/Nとおくと、i番目の頂点の角度は [2ti - t, 2ti + t] の範囲内

     - タイヤ位置について
    いずれかの頂点に位置する.
    Gene.dataがfloat型なので実際に使う際はintにキャストする(CarAgent.cs ApplyGene())

     - トルクについて
    常に一定のトルクを与えることで前進する(MyWheel.cs FixedUpdate参照)
    */

    // 交叉と複製は基本的なものがGeneOperator.csに実装されている。
    // overrideして新しいものを実装してもよい。

    // Geneの初期化
    public override Gene Init(){
        var gene = new Gene();
        for(int i = 0;i < 2;i++)gene.data.Add(UnityEngine.Random.Range(0.0f,VertexesNumber - eps));
        for(int i = 0;i < 2;i++)gene.data.Add(UnityEngine.Random.Range(MinWheelSize,MaxWheelSize));
        for(int i = 0;i < 2;i++)gene.data.Add(UnityEngine.Random.Range(MinTorque,MaxTorque));
        for(int i = 0;i < VertexesNumber;i++)gene.data.Add(UnityEngine.Random.Range(MinRadius,MaxRadius));
        for(int i = 0;i < VertexesNumber;i++)gene.data.Add(2*theta*i + UnityEngine.Random.Range(-theta,theta));
        return gene;
    }

    // 突然変異
    // 各変数について、変異のおこる確率、変異の大きさを決定するパラメーターをInspectorから設定できる
    public override Gene Mutate(Gene gene,int generation){
        var mutated_gene = new Gene();
        mutated_gene.data = new List<float>(gene.data);

        float mutrate = MutRate(generation);
        float r,p;
        p = mutrate * PosProb;
        for(int i = 0;i <= 1;i++){
            if(UnityEngine.Random.value < p)mutated_gene.data[i] = UnityEngine.Random.Range(0.0f,VertexesNumber - eps);
        }
        p = mutrate * WheelSizeProb;
        r = mutrate * WheelSizeMutationSize;
        for(int i = 2;i <= 3;i++){
            if(UnityEngine.Random.value < p)mutated_gene.data[i] = MutateClamp(mutated_gene.data[i],r,MinWheelSize,MaxWheelSize);
        }
        p = mutrate * TorqueProb;
        r = mutrate * TorqueMutationSize;
        for(int i = 4;i <= 5;i++){
            if(UnityEngine.Random.value < p)mutated_gene.data[i] = MutateClamp(mutated_gene.data[i],r,MinTorque,MaxTorque);
        }
        p = mutrate * RadiusProb;
        r = mutrate * RadiusMutationSize;
        for(int i = 0;i < VertexesNumber;i++){
            if(UnityEngine.Random.value < p)mutated_gene.data[6+i] = MutateClamp(mutated_gene.data[6+i],r,MinRadius,MaxRadius);
        }
        p = mutrate * AngleProb;
        r = mutrate * AngleMutationSize;
        for(int i = 0;i < VertexesNumber;i++){
            if(UnityEngine.Random.value < p)mutated_gene.data[6+VertexesNumber+i] = MutateClamp(mutated_gene.data[6+VertexesNumber+i],r,2*theta*i - theta, 2*theta*i + theta);
        }
        return mutated_gene;
    }

    // 世代によって変化する値 
    private float MutRate(int generation) {
        // 0世代目 1, b世代目に a になる値
        // [0,1] の範囲
        float a = 0.05f;
        float b = 30.0f;
        return a + (1.0f - a) * Mathf.Max(0f, 1.0f - generation / b);
    }
    
    private float MutateClamp(float x, float p, float min, float max){
        //　値がとりうる範囲の 100p% の範囲で摂動を与える。
        // 0 <= p <= 1
        float r = (max - min)*p*0.5f;
        x += UnityEngine.Random.Range(-r, r);
        x = Mathf.Max(x, min);
        x = Mathf.Min(x, max);
        return x;
    }

    //BLX-α交叉を定義
    public override (Gene,Gene) BrendCrossover(Gene gene1,Gene gene2,int generation){
        int Length = gene1.data.Count;
        var child_gene1 = new Gene();
        var child_gene2 = new Gene();
        float alpha = 0.3f;
        child_gene1.data = new List<float>(gene1.data);
        child_gene2.data = new List<float>(gene2.data);
        for(int i = 0;i < Length;i++){
            float min_data = Math.Min(gene1.data[i], gene2.data[i]) - alpha * Math.Abs(gene1.data[i] - gene2.data[i]);
            float max_data = Math.Max(gene1.data[i], gene2.data[i]) + alpha * Math.Abs(gene1.data[i] - gene2.data[i]);
            child_gene1.data[i] = UnityEngine.Random.Range(min_data, max_data);
            child_gene2.data[i] = UnityEngine.Random.Range(min_data, max_data);
            child_gene1.data[i] = CheckGene(child_gene1.data[i], i, min_data, max_data, gene1.data[i]);
            child_gene2.data[i] = CheckGene(child_gene2.data[i], i, min_data, max_data, gene2.data[i]);
        }
        return (child_gene1,child_gene2);
    }

    public float CheckGene(float data, int i, float min, float max, float parent){

        // body size
        float MinRadius = 0.7f;
        float MaxRadius = 1.6f;

        // wheel size
        float MinWheelSize = 0.6f;
        float MaxWheelSize = 1.8f;
        
        // power
        float MinTorque = 2.0f;
        float MaxTorque = 30.0f;

        int VertexesNumber = 12;
        float eps = 0.00001f;
        float theta = Mathf.PI/VertexesNumber;

        float New;

       
        if(i == 0 || i == 1){
            if(data < 0.0f || VertexesNumber <= data){
                New = (data - min) / (max - min) * (VertexesNumber - eps);
                if(0.0f <= New && New < VertexesNumber){
                    return New;
                }
                else{
                    return parent;
                }
                
            }
            else{
                return data;
            }
        }
        else if(i == 2 || i == 3){
            if(data < MinWheelSize || MaxWheelSize <= data){
                New = (data - min) / (max - min) * (MaxWheelSize - eps - MinWheelSize) + MinWheelSize;
                if(MinWheelSize <= New && New < MaxWheelSize){
                    return New;
                }
                else{
                    return parent;
                }   
            }
            else{
                return data;
            }
        }
        else if(i == 4 || i == 5){
            if(data < MinTorque || MaxTorque <= data){
                New = (data - min) / (max - min) * (MaxTorque - eps - MinTorque) + MinTorque;
                if(MinTorque <= New && New < MaxTorque){
                    return New;
                }
                else{
                    return parent;
                }
            }
            else{
                return data;
            }
        }
        else if(6 <= i && i < 6+VertexesNumber){
            if(data < MinRadius || MaxRadius <= data){
                New = (data - min) / (max - min) * (MaxRadius - eps - MinRadius) + MinRadius;
                if(MinRadius <= New && New < MaxRadius){
                    return New;
                }
                else{
                    return parent;
                }
            }
            else{
                return data;
            }
        }
        else{
            i -= 12;
            if(data < 2*theta*i - theta || 2*theta*i + theta <= data){
                New = (data - min) / (max - min) * (2 * theta - eps) + 2*theta*i - theta;
                if(2*theta*i - theta <= New && New < 2*theta*i + theta){
                    return New;
                }
                else{
                    return parent;
                }
            }
            else{
                return data;
            }
        }
    }
}
