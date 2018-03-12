using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour 
{

	public GameObject objectPrefab;


	private Queue<GameObject> objects;


	public void ClearPool()
	{
		while (objects.Count > 0)
		{
			Destroy(objects.Dequeue());
		}
	}


	public GameObject GetObject()
	{
		if (objects.Count > 0)
		{
			return objects.Dequeue();
		}
		else
		{
			GameObject newObject = Instantiate(objectPrefab, transform);
			newObject.SetActive(false);
			PoolSignature signature = newObject.AddComponent<PoolSignature>();
			signature.originalPool = this;
			return newObject;
		}
	}


	public void ReturnObject(GameObject returnedObject)
	{
		returnedObject.SetActive(false);
		returnedObject.transform.SetParent(transform);
		objects.Enqueue(returnedObject);
	}


	void Start()
	{
		objects = new Queue<GameObject>();
	}
}
