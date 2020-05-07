using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scorpion : MonoBehaviour
{
	private float time = 0.0f;
	private float rot = 0.0f;

	void Start ()
	{
		rot = Random.Range(0.0f, Mathf.PI * 2.0f);
		transform.localRotation = Quaternion.Euler(0.0f, rot * 180.0f / Mathf.PI, 0.0f);
	}
	
	void FixedUpdate ()
	{
		rot += Mathf.PI / 512.0f;
		transform.localRotation = Quaternion.Euler(0.0f, rot * 180.0f / Mathf.PI, 0.0f);
		Vector3 pos = transform.localPosition;
		Vector3 dir = new Vector3(Mathf.Sin(rot), 0.0f, Mathf.Cos(rot)) / 128.0f;
		pos += dir;
		pos.y = HeightMap.GetHeight(pos.x, pos.z) * 2.0f - 1.0f - 1.0f/32.0f;
		transform.localPosition = pos;
		time += 0.08f;
		Vector3 scale = new Vector3(1.0f, 1.0f, 1.0f);
		scale.x = 0.75f + (Mathf.Sin(time) + 1.0f) * 0.5f * 0.25f;
		scale.y = 0.75f + (Mathf.Cos(time) + 1.0f) * 0.5f * 0.25f;
		transform.localScale = scale;
	}
}
