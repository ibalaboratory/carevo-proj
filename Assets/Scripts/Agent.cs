// SerialID: [1ba2ce2c-2b2a-4e6d-9764-8ce1f38e28f0]
using System.Collections.Generic;
using UnityEngine;

//CarAgentの継承元。適合度を設定したりするための基底クラス
public abstract class Agent : MonoBehaviour
{
    public bool IsDone { get; private set; }
    public bool IsFrozen { get; private set; }

    public float Fitness { get; private set; }
    public float Distance { get; private set; }

    public void SetFitness(float fitness) { //最大距離 - 回転数を適合度とする用
        Fitness = fitness;
    }

    public void AddFitness(float fitness) {
        Fitness += fitness;
    }

    public void SetDistance(float distance) { //最大距離を適合度とする用
        Distance = distance;
    }

    public void AddDistance(float distance) {
        Distance += distance;
    }

    public abstract void AgentUpdate();

    public abstract void AgentReset();

    public abstract void ApplyGene(Gene gene);


    public abstract void Stop();

    public void Done()
    {
        IsDone = true;
    }

    public void Freeze()
    {
        Stop();
        IsFrozen = true;
        //gameObject.SetActive(false);
    }

    public void Reset()
    {
        //gameObject.SetActive(true);
        Stop();
        AgentReset();
        IsDone = false;
        IsFrozen = false;
    }
}
