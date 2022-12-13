using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StringExtensions
{
	public static bool IsEmpty(this string s)
	{
		return string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s);
	}
}