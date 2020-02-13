using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
	public static ObjectPool SharedInstancePool;

	public GameObject objectToPool;
	public int amountToPool;

	List<GameObject> pooledObjects;

	void Start()
	{
		SharedInstancePool = this;

		pooledObjects = new List<GameObject>();
		for (int i = 0; i < amountToPool; i++)
		{
			GameObject obj = (GameObject)Instantiate(objectToPool);
			obj.SetActive(false);
			pooledObjects.Add(obj);
		}

	}

	public GameObject GetPooledObject()
	{
		for (int i = 0; i < pooledObjects.Count / 20; i++)
		{
			int k = Random.Range(0, pooledObjects.Count);
			if (!pooledObjects[k].activeInHierarchy)
			{
				return pooledObjects[k];
			}
		}

		GameObject obj = (GameObject)Instantiate(objectToPool);
		obj.SetActive(false);
		pooledObjects.Add(obj);

		return null;
	}

	void Update()
	{

	}
}
