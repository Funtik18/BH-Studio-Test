using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions
{
	public static Transform DestroyChildren(this Transform transform)
	{
		foreach (Transform child in transform)
		{
			GameObject.Destroy(child.gameObject);
		}
		return transform;
	}
}