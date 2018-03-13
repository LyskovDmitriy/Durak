using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UsedCardsHolder : MonoBehaviour 
{

	public static UsedCardsHolder Instance { get; private set; }
	public Vector3 startingPositionForCards;
	public float distanceBetweenCardsX;
	public float distanceBetweenCardsY;
	public float cardMovementSpeed;
	public event System.Action onCardMoved;


	private RectTransform rTrans;


	public void AddCard(RectTransform cardTransform)
	{
		cardTransform.SetParent(rTrans);
		StartCoroutine(MoveTowardsCenter(cardTransform, CalculatePositionForLastChild()));
	}


	public void ClearUsedCards()
	{
		while (rTrans.childCount > 0)
		{
			rTrans.GetChild(0).GetComponent<PoolSignature>().ReturnToPool();
		}
	}


	void Awake () 
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else if (Instance != this)
		{
			Destroy(gameObject);
		}

		rTrans = GetComponent<RectTransform>();
	}


	IEnumerator MoveTowardsCenter(RectTransform objectToMove, Vector3 finalPosition)
	{
		while ((finalPosition - objectToMove.localPosition).sqrMagnitude > 0.0001f)
		{
			objectToMove.localPosition = Vector3.MoveTowards(objectToMove.localPosition, finalPosition, cardMovementSpeed * Time.deltaTime);
			yield return null;
		}

		if (onCardMoved != null)
		{
			onCardMoved();
		}
	}


	Vector3 CalculatePositionForLastChild()
	{
		bool isAttacker = (rTrans.childCount % 2 == 1);
		Vector3 position = startingPositionForCards;
		position += new Vector3(distanceBetweenCardsX * Mathf.FloorToInt((rTrans.childCount - 1) / 2), 
			(isAttacker ? 0.0f : -distanceBetweenCardsY), 0.0f);
		return position;
	}
}
