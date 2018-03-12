using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolSignature : MonoBehaviour
{

	public ObjectPool originalPool;


	public void ReturnToPool()
	{
		if (originalPool == null)
		{
			Destroy(gameObject);
		}
		else
		{
			originalPool.ReturnObject(gameObject);
		}
	}
}
