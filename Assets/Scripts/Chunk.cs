using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public int x, z;
    private Texture2D heightMap, heightNormalMap, holeFlags;
    private MeshRenderer mr;
    private Material material;
    private int selectedCell = -1;

    public void Init(int x, int z, Mesh mesh, Material sandMaterial, Material grassMaterial)
    {
        this.x = x;
        this.z = z;
        float xPos = x * Level.CHUNK_SIZE;
        float zPos = z * Level.CHUNK_SIZE;
        transform.position = new Vector3(xPos, 0.0f, zPos);

        const int resolution = (Level.CHUNK_SIZE << 1) + 1;
        // So that edge vertices are sampled by chunks that share edges.
        heightMap = new Texture2D(resolution, resolution, TextureFormat.RFloat, false);
        heightNormalMap = new Texture2D(resolution, resolution, TextureFormat.RGB24, false);
        heightMap.wrapMode = heightNormalMap.wrapMode = TextureWrapMode.Clamp;
        for (int zp = 0; zp < resolution; zp++)
        {
            for (int xp = 0; xp < resolution; xp++)
            {
                float xWorld = xPos + xp / (resolution - 1.0f) * Level.CHUNK_SIZE;
                float zWorld = zPos + zp / (resolution - 1.0f) * Level.CHUNK_SIZE;
                // xp / (resolution - 1.0f) ranges from 0.0f to 1.0f
                float sample = HeightMap.GetHeight(xWorld, zWorld);
                heightMap.SetPixel(xp, zp, new Color(sample, sample, sample));
                Vector3 normal = HeightMap.GetNormal(xWorld, zWorld);
                heightNormalMap.SetPixel(xp, zp, new Color(normal.x, normal.y, normal.z));
            }
        }
        heightMap.Apply();
        heightNormalMap.Apply();
        holeFlags = new Texture2D(Level.CHUNK_SIZE, Level.CHUNK_SIZE, TextureFormat.R8, false);
        holeFlags.filterMode = FilterMode.Point;
        for (int zp = 0; zp < Level.CHUNK_SIZE; zp++)
        {
            for (int xp = 0; xp < Level.CHUNK_SIZE; xp++)
            {
                holeFlags.SetPixel(xp, zp, Color.black);
            }
        }
        holeFlags.Apply();

        mr = gameObject.AddComponent<MeshRenderer>();
        material = new Material(sandMaterial);
        material.SetTexture("_HeightMap", heightMap);
        material.SetTexture("_HeightNormalMap", heightNormalMap);
        material.SetTexture("_HoleFlags", holeFlags);
        mr.sharedMaterial = material;
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        int chunkSeed = x * x * 0x4c19 + x * 0x5ac0 + z * z * 0x4307 + ((z * 0x5f24) ^ 0x3ad8);
        Random.InitState(chunkSeed);
        int grassCount = Random.Range(2, 6);
        for (int i = 0; i < grassCount; i++)
        {
            int xg = Random.Range(0, 7);
            int zg = Random.Range(0, 7);
            GameObject go = new GameObject("Grass " + i);
            go.transform.parent = transform;
            Grass grass = go.AddComponent<Grass>();
            float y = HeightMap.GetHeight(xPos + xg + 0.5f, zPos + zg + 0.5f) * 2.0f - 1.0f;
            grass.Init(new Vector3(xg, y, zg), grassMaterial);
        }
    }

    public void SetSelectedCell(int xCell, int zCell)
    {
        selectedCell = xCell + zCell * Level.CHUNK_SIZE;
        material.SetInt("_SelectedCell", selectedCell);
        mr.sharedMaterial = material;
    }

    public void UnsetSelectedCell()
    {
        selectedCell = -1;
        material.SetInt("_SelectedCell", selectedCell);
        mr.sharedMaterial = material;
    }

    public void Dig()
    {
        if (selectedCell >= 0)
        {
            int xp = selectedCell % Level.CHUNK_SIZE;
            int zp = selectedCell / Level.CHUNK_SIZE;
            holeFlags.SetPixel(xp, zp, Color.white);
            holeFlags.Apply();
            material.SetTexture("_HoleFlags", holeFlags);
            mr.sharedMaterial = material;
        }
    }

    void Update()
    {
    }
}