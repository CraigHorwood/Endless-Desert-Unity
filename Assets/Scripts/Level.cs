using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{

    public const int CHUNK_SIZE = 8;
	private const int CHUNK_RADIUS = 4;
	private const int MAX_SCORPIONS = 32;
	public Mesh chunkMesh;
	public Material sandMaterial, grassMaterial;
	public GameObject scorpionPrefab;
	public Player player;

	private List<Chunk> activeChunks = new List<Chunk>();
	private int xChunk = -1, zChunk = -1;
	private List<Scorpion> scorpions = new List<Scorpion>();

	// Use this for initialization
	void Start ()
	{
		Application.targetFrameRate = 60;
		const int vertexCellSize = CHUNK_SIZE << 1; // Vertices form 0.5x0.5 unit cells.
        int vertexCount = (vertexCellSize + 1) * (vertexCellSize + 1);
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int[] triangles = new int[vertexCellSize * vertexCellSize * 6];
        for (int z = 0; z <= vertexCellSize; z++)
        {
            for (int x = 0; x <= vertexCellSize; x++)
            {
                int i = x + z * (vertexCellSize + 1);
                vertices[i] = new Vector3(x * 0.5f, 0.0f, z * 0.5f);
                uvs[i] = new Vector2((float) x / vertexCellSize, (float) z / vertexCellSize);
            }
        }
        int j = 0;
        for (int z = 0; z < vertexCellSize; z++)
        {
            for (int x = 0; x < vertexCellSize; x++)
            {
                int offs = x + z * (vertexCellSize + 1);
                triangles[j++] = offs;
                triangles[j++] = offs + vertexCellSize + 1;
                triangles[j++] = offs + vertexCellSize + 2;
                triangles[j++] = offs + vertexCellSize + 2;
                triangles[j++] = offs + 1;
                triangles[j++] = offs;
            }
        }
        chunkMesh = new Mesh();
        chunkMesh.vertices = vertices;
        chunkMesh.uv = uvs;
        chunkMesh.triangles = triangles;
        chunkMesh.RecalculateNormals();
	}

	public Chunk GetChunk(int x, int z)
	{
		foreach (Chunk chunk in activeChunks)
		{
			if (chunk.x == x && chunk.z == z) return chunk;
		}
		return null;
	}

	private void AddChunk(int x, int z)
	{
		GameObject go = new GameObject("Chunk " + x + "," + z);
		go.transform.parent = transform;
		Chunk chunk = go.AddComponent<Chunk>();
		chunk.Init(x, z, chunkMesh, sandMaterial, grassMaterial);
		activeChunks.Add(chunk);
	}

	private bool IsChunkActive(int x, int z)
	{
		foreach (Chunk chunk in activeChunks)
		{
			if (chunk.x == x && chunk.z == z) return true;
		}
		return false;
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Player's current chunk.
		int xChunkNew = (int) player.transform.localPosition.x / CHUNK_SIZE;
		int zChunkNew = (int) player.transform.localPosition.z / CHUNK_SIZE;
		if (xChunk != xChunkNew || zChunk != zChunkNew)
		{
			for (int i = 0; i < activeChunks.Count; i++)
			{
				Chunk chunk = activeChunks[i];
				int xd = chunk.x - xChunkNew;
				int zd = chunk.z - zChunkNew;
				if (Mathf.Abs(xd) > CHUNK_RADIUS || Mathf.Abs(zd) > CHUNK_RADIUS)
				{
					Destroy(chunk.gameObject);
					activeChunks.RemoveAt(i--);
				}
			}
			for (int z = -CHUNK_RADIUS; z <= CHUNK_RADIUS; z++)
			{
				for (int x = -CHUNK_RADIUS; x <= CHUNK_RADIUS; x++)
				{
					int xc = xChunk + x;
					int zc = zChunk + z;
					if (!IsChunkActive(xc, zc)) AddChunk(xc, zc);
				}
			}
			xChunk = xChunkNew;
			zChunk = zChunkNew;
		}
		TrySpawnAnimal();
	}

	private void TrySpawnAnimal()
	{
		if (Random.Range(1, 56) == 1)
		{
			Vector3 pos = new Vector3();
			pos.x = player.transform.localPosition.x + Random.Range((float) -CHUNK_SIZE, (float) CHUNK_SIZE);
			pos.z = player.transform.localPosition.z + Random.Range((float) -CHUNK_SIZE, (float) CHUNK_SIZE);
			pos.y = HeightMap.GetHeight(pos.x, pos.z) * 2.0f - 1.0f;
			Vector3 playerFacing = Camera.main.transform.localRotation * Vector3.forward;
			Vector3 toNormal = (pos - player.transform.localPosition).normalized;
			float dot = Vector3.Dot(playerFacing, toNormal);
			if (dot < 0.0f)
			{
				if (scorpions.Count < MAX_SCORPIONS) scorpions.Add(Instantiate(scorpionPrefab, pos, Quaternion.identity).GetComponent<Scorpion>());
			}
		}
	}
}
