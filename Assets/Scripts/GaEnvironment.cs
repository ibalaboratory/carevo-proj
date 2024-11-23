// SerialID: [1ba2ce2c-2b2a-4e6d-9764-8ce1f38e28f0]
using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Linq;
using UnityEngine.UI;
using System.Diagnostics;

public class GaEnvironment : MonoBehaviour
{
    [Header("Settings"), SerializeField] private int totalPopulation = 200;//元は100
    private int TotalPopulation { get { return totalPopulation; } }

    [SerializeField] private int tournamentSelection = 85;
    private int TournamentSelection { get { return tournamentSelection; } }

    [SerializeField] private int eliteSelection = 4;//元は4
    private int EliteSelection { get { return eliteSelection; } }


    [SerializeField][Range(1,100)] private int nAgents = 4;
    private int NAgents { get { return nAgents; } }

    //追加
    [SerializeField] private int nGrounds = 5;
    private int NGrounds { get { return nGrounds; } }

    [SerializeField] private bool isBLXBlend;
    [SerializeField] private bool landscapeChange;
    [SerializeField] private bool rollOverAffectFitness;
    public bool RollOverAffectFitness{ get { return rollOverAffectFitness; } }

    [Header("Agent Prefab"), SerializeField] private GameObject GObjectAgent = null;

    //追加
    [SerializeField] private GameObject GObjectGround = null;

    [Header("UI References"), SerializeField] private Text populationText = null;
    private Text PopulationText { get { return populationText; } }

    //private float GenBestRecord { get; set; }
    private List<float> GenBestRecordList { get; } = new List<float>();

    //private float SumFitness { get; set; }
    private List<float> SumFitnessList { get; } = new List<float>();
    //private float AvgFitness { get; set; }
    private List<float> AvgFitnessList { get; } = new List<float>();

    //private float SumDistance { get; set; }
    private List<float> SumDistanceList { get; } = new List<float>();
    //private float AvgDistance { get; set; }
    private List<float> AvgDistanceList { get; } = new List<float>();

    private List<GameObject> GObjects { get; } = new List<GameObject>();
    //private List<Agent> Agents { get; } = new List<Agent>();
    private List<List<Agent>> AgentsList { get; } = new List<List<Agent>>();    
    //private List<Gene> Genes { get; } = new List<Gene>();
    private List<List<Gene>> GenesList { get; } = new List<List<Gene>>();
    public int Generation { get; set; } = 0;
    private int prev_gen = 0;

    //追加
    private List<bool> FinishList { get; set; } = new List<bool>();

    //private float BestRecord { get; set; }
    private List<float> BestRecordList { get; } = new List<float>();

    //private List<AgentPair> AgentsSet { get; set;} = new List<AgentPair>();
    private List<List<AgentPair>> AgentsSetList { get; } = new List<List<AgentPair>>();
    //private Queue<Gene> CurrentGenes { get; set; }
    private List<Queue<Gene>> CurrentGenesList { get; } = new List<Queue<Gene>>();

    [Header("Gene"),SerializeField] private GeneOperator Operator = null;
    [SerializeField] private GroundGeneOperator ggOperator = null;
    //private GroundGenerator groundGenerator {get; set;}
    private List<GroundGenerator> GrGens {get;} = new List<GroundGenerator>();
    private List<float> GroundFitnessList {get;} = new List<float>();


    // 個体オブジェクトと遺伝子を生成
    // 個体オブジェクトはNAgentsコだけ作って使いまわす
    void Awake() {
        for(int i = 0; i < NGrounds; i++) {
            var Genes = new List<Gene>();
            for(int j = 0; j < TotalPopulation; j++) {
                Gene gene = Operator.Init();
                Genes.Add(gene);
            }
            GenesList.Add(Genes);


            var objg = Instantiate(GObjectGround);
            objg.transform.position = objg.transform.position + new Vector3(0, 0, -10*i);
            objg.SetActive(true);
            GObjects.Add(objg);
            GrGens.Add(objg.GetComponent<GroundGenerator>());


            var Agents = new List<Agent>();
            for(int j = 0; j < NAgents; j++) {
                var obj = Instantiate(GObjectAgent);
                obj.transform.position = obj.transform.position + new Vector3(0, 0, -10*i);
                obj.SetActive(true);
                GObjects.Add(obj);
                Agents.Add(obj.GetComponent<Agent>());
            }
            AgentsList.Add(Agents);


            AgentsSetList.Add(new List<AgentPair>());
            CurrentGenesList.Add(new Queue<Gene>());

            GenBestRecordList.Add(0.0f);
            SumFitnessList.Add(0.0f);
            AvgFitnessList.Add(0.0f);
            SumDistanceList.Add(0.0f);
            AvgDistanceList.Add(0.0f);
            BestRecordList.Add(0.0f);

            FinishList.Add(false);
            GroundFitnessList.Add(0.0f);
        }
    }

    void Start()
    {
        for (int k = 0; k < NGrounds; k++) {
            SetStartAgents(k);

            string recordfilepath = string.Format("mytest/record{0}.csv", k);
            StreamWriter file = new StreamWriter(recordfilepath, false, Encoding.UTF8);
            file.WriteLine(string.Format("{0},{1},{2},{3},{4}", "Generation", "Best Record", "Best this gen", "Average", "Ground Fitness"));
            UnityEngine.Debug.Log("ファイルの作成");
            file.Close();
        }
        //GameObject grounds = GameObject.Find("Grounds");
        //groundGenerator = grounds.GetComponent<GroundGenerator>();
    }

    // Agent,Geneを組としてAgentsSetにいれる
    // AgentsSetは生きているAgentとGeneの組を扱うList
    // GeneをAgentに適用する
    void SetStartAgents(int k) {
        //for(int k = 0; k < NGrounds; k++) {
            var Agents = AgentsList[k];
            var Genes = GenesList[k];

            var CurrentGenes = new Queue<Gene>(Genes);
            //AgentsSet.Clear();
            var AgentsSet = new List<AgentPair>();

            var size = Math.Min(NAgents, TotalPopulation);
            for(var i = 0; i < size; i++) {
                AgentsSet.Add(new AgentPair {
                    agent = Agents[i],
                    gene = CurrentGenes.Dequeue()
                });
            }
            foreach(var pair in AgentsSet){
                pair.agent.ApplyGene(pair.gene);
            }
            CurrentGenesList[k] = CurrentGenes;
            AgentsSetList[k] = AgentsSet;
        //}
    }

    // 生きているAgentを更新
    // 死んでしまったAgentは報酬の処理をして除去
    // 次の世代を生成、もしくは次のAgent,Geneの組を追加
    void FixedUpdate() {
        if (FinishList.All(b => b)) {
            Generation++;
            if (Generation > prev_gen + 2) {
                SetNextGroundGeneration();
                prev_gen = Generation;
            }

            for(int k = 0; k < NGrounds; k++) {
                SetNextGeneration(k);
                FinishList[k] = false;
            }
        }

        for(int k = 0; k < NGrounds; k++){
            var AgentsSet = AgentsSetList[k];
            var CurrentGenes = CurrentGenesList[k];

            foreach(var pair in AgentsSet.Where(p => !p.agent.IsDone)) {
                pair.agent.AgentUpdate();
            }

            AgentsSet.RemoveAll(p => {
                if(p.agent.IsDone) {
                    float r = p.agent.Fitness;
                    float d = p.agent.Distance;
                    BestRecordList[k] = Mathf.Max(d, BestRecordList[k]);
                    GenBestRecordList[k] = Mathf.Max(d, GenBestRecordList[k]);
                    p.gene.Fitness = r;
                    p.gene.Distance = d;
                    SumFitnessList[k] += r;
                    SumDistanceList[k] += d;
                }
                return p.agent.IsDone;
            });

            if(CurrentGenes.Count == 0 && AgentsSet.Count == 0) {
                FinishList[k] = true;
                //SetNextGeneration(k);
            }
            else {
                SetNextAgents(k);
            }
        }
    }

    private void SetNextAgents(int k) {
        //for(int k = 0; k < NGrounds; k++) {
            var AgentsSet = AgentsSetList[k];
            var CurrentGenes = CurrentGenesList[k];
            var Agents = AgentsList[k];

            int size = Math.Min(NAgents - AgentsSet.Count, CurrentGenes.Count);
            for(var i = 0; i < size; i++) {
                var nextAgent = Agents.First(a => a.IsDone);
                var nextGene = CurrentGenes.Dequeue();
                nextAgent.Reset();
                nextAgent.ApplyGene(nextGene);
                AgentsSet.Add(new AgentPair {
                    agent = nextAgent,
                    gene = nextGene
                });
            }
            UpdateText();
        //}
    }

    private void SetNextGeneration(int k) {
        //for(int k = 0; k < NGrounds; k++) {
            var Agents = AgentsList[k];

            AvgFitnessList[k] = SumFitnessList[k] / TotalPopulation;
            AvgDistanceList[k] = SumDistanceList[k] / TotalPopulation;
            //new generation
            GenPopulation(k);
            SumFitnessList[k] = 0;
            SumDistanceList[k] = 0;
            GenBestRecordList[k] = 0;
            Agents.ForEach(a => a.Reset());
            SetStartAgents(k);
            UpdateText();
        //}
    }





    private void SetNextGroundGeneration() {
        for (int k = 0; k < NGrounds; k++) SetGroundFitness(k);

        var children = new List<GroundGene>();
        var parents = new List<GroundGene>();
        float mutate_only = 0.3f;
        int ntournament = (int)(NGrounds * 0.85f);
        int nelite = 1;
        int nnewcomer = 1;
        float length = GrGens[0].length;

        for (int k = 0; k < NGrounds; k++) {
            parents.Add(GrGens[k].groundgene);
        }
        parents.Sort(CompareGroundGenes);
        
        
        //Elite selection
        for(int i = 0; i < nelite; i++){
            children.Add(ggOperator.Clone(parents[i]));//エリートなものはそのまま残す
        }

        // Newcomer
        for (int i = 0; i < nnewcomer; i++) {
            children.Add(ggOperator.Init(length));
        }
            
        // トーナメント選択 + 突然変異
        while(children.Count < NGrounds/* * mutate_only*/) {
            var tournamentMembers = parents.AsEnumerable().OrderBy(x => Guid.NewGuid()).Take(ntournament).ToList();
            tournamentMembers.Sort(CompareGroundGenes);
            children.Add(ggOperator.Mutate(tournamentMembers[0]));
            if(children.Count < NGrounds * mutate_only) children.Add(ggOperator.Mutate(tournamentMembers[1]));
        }
        
        // トーナメント選択 + 交叉
        while(children.Count < NGrounds) {
            var tournamentMembers = parents.AsEnumerable().OrderBy(x => Guid.NewGuid()).Take(ntournament).ToList();
            tournamentMembers.Sort(CompareGroundGenes);
            GroundGene child1,child2;
            (child1,child2) = ggOperator.Crossover(tournamentMembers[0],tournamentMembers[1]);
            children.Add(child1);
            if(children.Count < NGrounds)children.Add(child2);
        }
        
        var random = new System.Random();
        children = children.OrderBy(x => random.Next()).ToList();

        for (int k = 0; k < NGrounds; k++) {
            GrGens[k].groundgene = children[k];
            GrGens[k].CreateMesh();
        }
    }
    private void SetGroundFitness(int k) {
        var genbest = GenBestRecordList[k];
        var genavg = AvgDistanceList[k];
        var fitness = genbest - genavg;
        //var fitness = -genbest;
        GrGens[k].groundgene.Fitness = fitness;
        GroundFitnessList[k] = fitness;
    }
    private static int CompareGroundGenes(GroundGene a, GroundGene b) {
        if(a.Fitness > b.Fitness) return -1;
        if(b.Fitness > a.Fitness) return 1;
        return 0;
    }





    // 適応度で降順ソートするための関数
    private static int CompareGenes(Gene a, Gene b) {
        if(a.Fitness > b.Fitness) return -1;
        if(b.Fitness > a.Fitness) return 1;
        return 0;
    }


    // 選択、交叉、突然変異といった遺伝的操作をくわえて次の世代を生成する
    private void GenPopulation(int k) {
        //for(int k = 0; k < NGrounds; k++) {
            var Genes = GenesList[k];

            var children = new List<Gene>();
            var bestGenes = Genes.ToList();
            //Elite selection
            bestGenes.Sort(CompareGenes);
            for(int i = 0; i < EliteSelection;i++){
                children.Add(Operator.Clone(bestGenes[i]));//エリートなものはそのまま残す
            }
            float mutate_only = 0.3f;
            
            // トーナメント選択 + 突然変異
            while(children.Count < TotalPopulation * mutate_only) {
                var tournamentMembers = Genes.AsEnumerable().OrderBy(x => Guid.NewGuid()).Take(tournamentSelection).ToList();
                tournamentMembers.Sort(CompareGenes);
                children.Add(Operator.Mutate(tournamentMembers[0],Generation));
                if(children.Count < TotalPopulation * mutate_only) children.Add(Operator.Mutate(tournamentMembers[1],Generation));
            }
        
            // トーナメント選択 + (交叉,BLX-α(ブレンド交叉))
            while(children.Count < TotalPopulation) {
                var tournamentMembers = Genes.AsEnumerable().OrderBy(x => Guid.NewGuid()).Take(tournamentSelection).ToList();
                tournamentMembers.Sort(CompareGenes);
                Gene child1,child2;
                //インスペクタにおける指定によって交叉方法を変更
                if(isBLXBlend){
                    (child1,child2) = Operator.BrendCrossover(tournamentMembers[0],tournamentMembers[1],Generation); //ここを変更
                }else{
                    (child1,child2) = Operator.Crossover(tournamentMembers[0],tournamentMembers[1],Generation);
                }    
                children.Add(child1);
                if(children.Count < TotalPopulation)children.Add(child2);
            }

            GenesList[k] = children;
            //Generation++;
            WriteRecord(k);
            //インスペクタから地形変動を指定した場合、地形の変化を行う。
            /*if(landscapeChange){
                GrGens[k].ChangeLandscape();
            }*/     
        //}
    }

    private void UpdateText() {
        var PopuNumText = "";
        var BestRecordText = "";
        var BestThisGenText = "";
        var AverageText = "";

        for(int k=0; k < NGrounds; k++) {
            PopuNumText += "i" + k + " -> " + (TotalPopulation - CurrentGenesList[k].Count) + "/" + TotalPopulation + ", ";
            BestRecordText += "i" + k + " -> " + BestRecordList[k] + ", ";
            BestThisGenText += "i" + k + " -> " + GenBestRecordList[k] + ", ";
            AverageText += "i" + k + " -> " + AvgDistanceList[k] + ", ";
        }

            PopulationText.text = "Population: " + PopuNumText
                + "\nGeneration: " + (Generation + 1)
                + "\nBest Record: " + BestRecordText
                + "\nBest this gen: " + BestThisGenText
                + "\nAverage: " + AverageText;
    }

    private struct AgentPair
    {
        public Gene gene;
        public Agent agent;
    }

    private void WriteRecord(int k) {
        string recordfilepath = string.Format("mytest/record{0}.csv", k);
        StreamWriter file = new StreamWriter(recordfilepath, true, Encoding.UTF8);
        file.WriteLine(string.Format("{0},{1},{2},{3},{4}", Generation, BestRecordList[k], GenBestRecordList[k], AvgFitnessList[k], GroundFitnessList[k]));
        file.Close();
    }
}
