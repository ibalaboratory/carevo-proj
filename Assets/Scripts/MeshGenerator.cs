// SerialID: [1ba2ce2c-2b2a-4e6d-9764-8ce1f38e28f0]
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
//車のメッシュを生成する
public class MeshGenerator : MonoBehaviour
{
    // Create boby mesh 
    public void CreateBody(List<float> BodyRadius,List<float> BodyAngle)
    {
        int N = BodyRadius.Count;
        float Z = 0.6f;
        
        Vector3[] vertices = new Vector3[12*N];
        Vector3 center1 = new Vector3 (0, 0, -Z);
        Vector3 center2 = new Vector3 (0, 0,  Z);
        for(int i = 0;i < N;i++){
            Vector3 v1 = new Vector3(BodyRadius[i] * Mathf.Cos(-BodyAngle[i]), BodyRadius[i] * Mathf.Sin(-BodyAngle[i]), -Z);
            Vector3 v2 = new Vector3(BodyRadius[i] * Mathf.Cos(-BodyAngle[i]), BodyRadius[i] * Mathf.Sin(-BodyAngle[i]),  Z);
            Vector3 v3 = new Vector3(BodyRadius[(i+1)%N] * Mathf.Cos(-BodyAngle[(i+1)%N]), BodyRadius[(i+1)%N] * Mathf.Sin(-BodyAngle[(i+1)%N]), -Z);
            Vector3 v4 = new Vector3(BodyRadius[(i+1)%N] * Mathf.Cos(-BodyAngle[(i+1)%N]), BodyRadius[(i+1)%N] * Mathf.Sin(-BodyAngle[(i+1)%N]),  Z);
            vertices[12*i] = center1;
            vertices[12*i+1] = v1;
            vertices[12*i+2] = v3;

            vertices[12*i+3] = center2;
            vertices[12*i+4] = v4;
            vertices[12*i+5] = v2;

            vertices[12*i+6] = v1;
            vertices[12*i+7] = v4;
            vertices[12*i+8] = v3;

            vertices[12*i+9] = v4;
            vertices[12*i+10] = v1;
            vertices[12*i+11] = v2;
        }

        int[] triangles = new int[12*N];
        for(int i = 0;i < 12*N;i++)triangles[i] = i;

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}