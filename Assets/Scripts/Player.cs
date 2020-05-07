using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Level level;
    private bool[] keysDown = new bool[] { false, false, false, false, false, false };
    private CharacterController controller;
    private Vector3 motion = new Vector3();
    private float xRot = 0.0f, yRot = 0.0f;
    private bool onGround = false;

    public GameObject shovel;
    private Vector3 shovelPos;
    private float bob = 0.0f, bobPhase = 0.0f;
    private int attackTime = 0;
    private Chunk selectedChunk = null;

    public AudioClip[] stepClips = new AudioClip[4];
    private AudioSource[] stepSources = new AudioSource[4];

    void Start ()
    {
        level = GameObject.Find("Level").GetComponent<Level>();
        controller = GetComponent<CharacterController>();
        shovelPos = shovel.transform.localPosition;
        for (int i = 0; i < stepSources.Length; i++)
        {
            stepSources[i] = gameObject.AddComponent<AudioSource>();
            stepSources[i].clip = stepClips[i];
            stepSources[i].playOnAwake = false;
        }
    }

    void FixedUpdate ()
    {
        if (keysDown[5])
        {
            attackTime = 12;
            if (selectedChunk != null) selectedChunk.Dig();
        }
        Vector3 impulse = new Vector3();
        if (keysDown[0]) impulse.z++;
        if (keysDown[1]) impulse.x--;
        if (keysDown[2]) impulse.z--;
        if (keysDown[3]) impulse.x++;
        motion += Quaternion.Euler(0.0f, yRot, 0.0f) * impulse.normalized / 128.0f;
        if (keysDown[4] && onGround)
        {
            motion.y = 0.125f;
            onGround = false;
        }
        controller.Move(motion);
        float dist = Mathf.Sqrt(motion.x * motion.x + motion.z * motion.z);
        bob += dist * 2.0f;
        float bobPhaseOld = bobPhase;
        bobPhase += dist * 2.0f;
        if (onGround && (int) bobPhase / 3 > (int) bobPhaseOld / 3)
        {
            AudioSource playSource = stepSources[Random.Range(0, stepSources.Length - 1)];
            float volume = (motion.x * motion.x + motion.z * motion.z) * 200.0f;
            if (volume > 1.0f) volume = 1.0f;
            playSource.volume = volume;
            playSource.Play();
        }
        bob *= 0.6f;
        float groundHeight = HeightMap.GetHeight(transform.localPosition.x, transform.localPosition.z) * 2.0f - 1.0f;
        Vector3 newPos = transform.localPosition;
        if (newPos.y <= groundHeight + 1.0f)
        {
            newPos.y = groundHeight + 1.0f;
            motion.y = 0.0f;
            onGround = true;
            transform.localPosition = newPos;
        }
        motion.x *= 0.875f;
        motion.z *= 0.875f;
        if (Mathf.Abs(motion.x) < 1.0f/1024.0f) motion.x = 0.0f;
        if (Mathf.Abs(motion.z) < 1.0f/1024.0f) motion.z = 0.0f;
        motion.y -= 1.0f / 128.0f;
    }

    void Update ()
    {
        if (Cursor.lockState == CursorLockMode.None)
        {
            if (Input.GetMouseButtonDown(0)) Cursor.lockState = CursorLockMode.Locked;
        }
        else if (Cursor.lockState == CursorLockMode.Locked)
        {
            keysDown[0] = Input.GetKey(KeyCode.W);
            keysDown[1] = Input.GetKey(KeyCode.A);
            keysDown[2] = Input.GetKey(KeyCode.S);
            keysDown[3] = Input.GetKey(KeyCode.D);
            keysDown[4] = Input.GetKey(KeyCode.Space);
            keysDown[5] = Input.GetMouseButtonDown(0);
            float xd = Input.GetAxis("Mouse X");
            float yd = Input.GetAxis("Mouse Y");
            xRot -= yd;
            if (xRot < -90.0f) xRot = -90.0f;
            else if (xRot > 90.0f) xRot = 90.0f;
            yRot += xd;
            Camera.main.transform.localRotation = Quaternion.Euler(xRot, yRot, 0.0f);
            if (xRot > 0.0f)
            {
                int xCell = 0;
                int zCell = 0;
                Vector3 origin = Camera.main.transform.position;
                Vector3 camNormal = Camera.main.transform.localRotation * Vector3.forward;
                float scaleFactor = 1.0f;
                float minScale = 0.0f;
                float maxScale = 0.0f;
                const float epsilon = 0.0625f;
                while (true)
                {
                    Vector3 pos = origin + camNormal * scaleFactor;
                    xCell = (int) pos.x;
                    zCell = (int) pos.z;
                    if (pos.x < 0.0f) xCell--;
                    if (pos.z < 0.0f) zCell--;
                    float hd = pos.y - (HeightMap.GetHeight(pos.x, pos.z) * 2.0f - 1.0f);
                    if (Mathf.Abs(hd) < epsilon) break;
                    if (hd < 0.0f)
                    {
                        // Point is under the ground height, scaleFactor is too large.
                        maxScale = scaleFactor;
                        scaleFactor = minScale + (scaleFactor - minScale) * 0.5f;
                    }
                    else
                    {
                        // Point is above the ground height, scaleFactor is too small.
                        minScale = scaleFactor;
                        if (maxScale == 0.0f)
                        {
                            scaleFactor *= 2.0f;
                            if (scaleFactor > 128.0f) break;
                        }
                        else scaleFactor = maxScale - (maxScale - scaleFactor) * 0.5f;
                    }
                }
                int xChunk = xCell / Level.CHUNK_SIZE;
                int zChunk = zCell / Level.CHUNK_SIZE;
                if (xCell < 0) xChunk--;
                if (zCell < 0) zChunk--;
                Chunk lastSelectedChunk = selectedChunk;
                selectedChunk = level.GetChunk(xChunk, zChunk);
                if (selectedChunk != null)
                {
                    if (lastSelectedChunk != null && selectedChunk != lastSelectedChunk) lastSelectedChunk.UnsetSelectedCell();
                    int xChunkCell = xCell % Level.CHUNK_SIZE;
                    int zChunkCell = zCell % Level.CHUNK_SIZE;
                    if (xChunkCell < 0) xChunkCell += Level.CHUNK_SIZE;
                    if (zChunkCell < 0) zChunkCell += Level.CHUNK_SIZE;
                    selectedChunk.SetSelectedCell(xChunkCell, zChunkCell);
                }
                else if (lastSelectedChunk != null) lastSelectedChunk.UnsetSelectedCell();
            }
            else if (selectedChunk != null)
            {
                selectedChunk.UnsetSelectedCell();
                selectedChunk = null;
            }
            if (Input.GetKeyDown(KeyCode.Escape)) Cursor.lockState = CursorLockMode.None;
        }
        Camera.main.transform.localPosition = new Vector3(0.0f, 1.0f + Mathf.Sin(bobPhase * 2.0f) * bob * 0.5f, 0.0f);
        if (attackTime == 0) shovel.transform.localPosition = shovelPos + new Vector3(Mathf.Sin(bobPhase), Mathf.Cos(bobPhase * 2.0f), 0.0f) * 0.125f * bob;
        else
        {
            float pr = attackTime / 12.0f;
            shovel.transform.localPosition = shovelPos + new Vector3(-0.25f, 0.0f, 0.5f) * pr;
            attackTime--;
        }
    }
}