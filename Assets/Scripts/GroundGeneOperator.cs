using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu]
public class GroundGeneOperator : ScriptableObject
{
    [SerializeField] private float MutProb = 0.5f;
    [SerializeField] private float MutSize = 0.05f;

    public GroundGene Init(float length, int seed = 0){
        System.Random rnd;
        if (seed > 0) rnd = new System.Random(seed);
        else rnd = new System.Random();

        var groundgene = new GroundGene();
        //groundgene.length = length;

        // 初期地点は固定
        float min_y = 0.0f;
        groundgene.xpos.Add(0.0f);
        groundgene.xpos.Add(6.0f);
        groundgene.ypos.Add(0.0f);
        groundgene.ypos.Add(0.0f);
        groundgene.label.Add(-1);
        groundgene.label.Add(-1);
        // x座標がlengthを超える程度まで地面を伸ばす
        // 直前の頂点に対してdx,dyを与えて座標を決定する。
        int i = 0;
        int lbl = 0;
        while(groundgene.xpos[groundgene.ypos.Count-1] < length){
            // 徐々に道を険しくしていく
            float r = 0.3f + groundgene.xpos[groundgene.ypos.Count-1]/length*0.7f; 
            float dx = 0.5f;
            float dy = ((float)rnd.NextDouble() - 0.5f) * r;
            groundgene.xpos.Add(groundgene.xpos[groundgene.xpos.Count-1]+dx);
            groundgene.ypos.Add(groundgene.ypos[groundgene.ypos.Count-1]+dy);
            min_y = Mathf.Min(min_y,groundgene.ypos[groundgene.ypos.Count-1]);
            if (i == 10) {
                lbl++;
                i = 0;
            }
            groundgene.label.Add(lbl);
            i++;
        }
        groundgene.xpos.Add(groundgene.xpos[groundgene.xpos.Count-1]+0.001f);
        groundgene.ypos.Add(groundgene.ypos[groundgene.ypos.Count-1]+5);
        groundgene.xpos.Add(groundgene.xpos[groundgene.xpos.Count-1]+1);
        groundgene.ypos.Add(groundgene.ypos[groundgene.ypos.Count-1]);
        groundgene.label.Add(-2);
        groundgene.label.Add(-2);
        groundgene.bottom = min_y - 3.0f;

        return groundgene;
    }

    public GroundGene Clone(GroundGene gene){
        var cloned_gene = new GroundGene();
        cloned_gene.xpos = new List<float>(gene.xpos);
        cloned_gene.ypos = new List<float>(gene.ypos);
        cloned_gene.label = new List<int>(gene.label);
        cloned_gene.bottom = gene.bottom;
        return cloned_gene;
    }

    public GroundGene Mutate(GroundGene gene/*,int generation*/){
        var mutated_gene = Clone(gene);
        int Length = gene.label.Max() + 1;
        for (int i = 0; i < Length; i++) {
            if (UnityEngine.Random.value < MutProb) {
                int firstIdx = gene.label.FindIndex(lbl => lbl == i);
                int lastIdx = gene.label.FindLastIndex(lbl => lbl == i);
                for (int idx = firstIdx; idx <= lastIdx; idx++) {
                    mutated_gene.ypos[idx] += (UnityEngine.Random.value - 0.5f) * MutSize;
                }
            }
        }
        return mutated_gene;
    }

    public (GroundGene,GroundGene) Crossover(GroundGene gene1,GroundGene gene2/*,int generation*/){
        int Length = gene1.label.Max() + 1;
        var child_gene1 = Clone(gene1);
        var child_gene2 = Clone(gene2);
        child_gene1.ypos = new List<float>(gene1.ypos);
        child_gene2.ypos = new List<float>(gene2.ypos);
        for(int i = 0;i < Length;i++){
            if(UnityEngine.Random.value < 0.5f){
                int firstIdx = gene1.label.FindIndex(lbl => lbl == i);
                int lastIdx = gene1.label.FindLastIndex(lbl => lbl == i);
                for (int idx = firstIdx; idx <= lastIdx; idx++) {
                    child_gene1.ypos[idx] = gene2.ypos[idx];
                    child_gene2.ypos[idx] = gene1.ypos[idx];
                }
            }
        }
        child_gene1.bottom = Mathf.Min(gene1.bottom, gene2.bottom);
        child_gene2.bottom = Mathf.Min(gene1.bottom, gene2.bottom);
        //child_gene1.Fitness = 0.0f;
        //child_gene2.Fitness = 0.0f;
        return (child_gene1,child_gene2);
    }
}
