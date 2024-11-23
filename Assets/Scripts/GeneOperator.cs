// SerialID: [1ba2ce2c-2b2a-4e6d-9764-8ce1f38e28f0]
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public abstract class GeneOperator : ScriptableObject
{   
    public abstract Gene Init();
    public virtual Gene Clone(Gene gene){
        var cloned_gene = new Gene();
        cloned_gene.data = new List<float>(gene.data);
        return cloned_gene;
    }
    public abstract Gene Mutate(Gene gene,int generation);
    public virtual (Gene,Gene) Crossover(Gene gene1,Gene gene2,int generation){
        int Length = gene1.data.Count;
        var child_gene1 = new Gene();
        var child_gene2 = new Gene();
        child_gene1.data = new List<float>(gene1.data);
        child_gene2.data = new List<float>(gene2.data);
        for(int i = 0;i < Length;i++){
            if(UnityEngine.Random.value < 0.5f){
                child_gene1.data[i] = gene2.data[i];
                child_gene2.data[i] = gene1.data[i];
            }
        }
        return (child_gene1,child_gene2);
    }

    public abstract (Gene,Gene) BrendCrossover(Gene gene1,Gene gene2,int generation);
}
