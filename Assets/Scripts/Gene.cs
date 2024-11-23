// SerialID: [1ba2ce2c-2b2a-4e6d-9764-8ce1f38e28f0]
using System.Collections;
using System.Collections.Generic;

public class Gene
{
    public float Fitness { get; set; }
    public float Distance {get; set;}
    public List<float> data = new List<float>();
}

public class GroundGene
{
    public float Fitness { get; set; }
    //public float length;
    public List<float> xpos = new List<float>();
    public List<float> ypos = new List<float>();
    public float bottom;
    public List<int> label = new List<int>(); // 地形の各部をラベル付け
}