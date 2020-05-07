using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Grass : MonoBehaviour
{
    public void Init(Vector3 pos, Material grassMaterial)
    {
        gameObject.transform.localPosition = pos;
        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        mr.sharedMaterial = grassMaterial;
        mr.shadowCastingMode = ShadowCastingMode.Off;
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        const float scale = 1.0f;
        const float halfScale = scale * 0.5f;
        const float halfScaleRot = halfScale * 0.7071068f;
        Vector3[] vertices = new Vector3[] {
            new Vector3(-halfScale, 0.0f, 0.0f), new Vector3(-halfScale, scale, 0.0f), new Vector3(halfScale, scale, 0.0f), new Vector3(halfScale, 0.0f, 0.0f),
            new Vector3(-halfScaleRot, 0.0f, -halfScaleRot), new Vector3(-halfScaleRot, scale, -halfScaleRot), new Vector3(halfScaleRot, scale, halfScaleRot), new Vector3(halfScaleRot, 0.0f, halfScaleRot),
            new Vector3(-halfScaleRot, 0.0f, halfScaleRot), new Vector3(-halfScaleRot, scale, halfScaleRot), new Vector3(halfScaleRot, scale, -halfScaleRot), new Vector3(halfScaleRot, 0.0f, -halfScaleRot)
        };
        Vector2[] uvs = new Vector2[] {
            new Vector2(0.0f, 0.0f), new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f), new Vector2(1.0f, 0.0f),
            new Vector2(0.0f, 0.0f), new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f), new Vector2(1.0f, 0.0f),
            new Vector2(0.0f, 0.0f), new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f), new Vector2(1.0f, 0.0f)
        };
        int[] triangles = new int[] {
            0, 1, 2, 2, 3, 0,
            4, 5, 6, 6, 7, 4,
            8, 9, 10, 10, 11, 8,
            0, 3, 2, 2, 1, 0,
            4, 7, 6, 6, 5, 4,
            8, 11, 10, 10, 9, 8
        };
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mf.sharedMesh = mesh;
    }
}