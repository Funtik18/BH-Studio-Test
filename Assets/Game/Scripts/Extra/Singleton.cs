using Mirror;

using UnityEngine;

public abstract class LazySingleton<T> where T : class, new()
{
	public static T Instance
	{
		get
		{
			if(instance == null)
			{
				instance = new T();
			}

			return instance;
		}
	}
	private static T instance;
}

public abstract class LazySingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				instance = GameObject.FindObjectOfType<T>();
				if(instance == null)
				{
					instance = new GameObject($"Instance {typeof(T).Name}").AddComponent<T>();
				}
			}

			return instance;
		}
	}
	private static T instance;
}

public abstract class LazySingletonMonoDontDestroyOnLoad<T> : MonoBehaviour where T : MonoBehaviour
{
	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				instance = GameObject.FindObjectOfType<T>();
				if (instance == null)
				{
					instance = new GameObject($"Instance {typeof(T).Name}").AddComponent<T>();
				}

				DontDestroyOnLoad(instance);
			}

			return instance;
		}
	}
	private static T instance;
}

public abstract class LazySingletonNetMono<T> : NetworkBehaviour where T : NetworkBehaviour
{
	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				instance = GameObject.FindObjectOfType<T>();
				if (instance == null)
				{
					instance = new GameObject($"Instance {typeof(T).Name}").AddComponent<T>();
				}
			}

			return instance;
		}
	}
	private static T instance;
}