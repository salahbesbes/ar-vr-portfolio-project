using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class CoverLogic : MonoBehaviour
{
	private PlayerStateManager unit;
	private GameStateManager gameManger;
	private Node selectedUnitLastPosition;
	private Node myLastPosition;
	private NodeGrid grid;
	public Node front;
	public Node back;
	public Node right;
	public Node left;

	private List<Cover> covers = new List<Cover>();
	internal bool alreadyFluncked = false;

	public int CoverValue
	{
		get
		{
			int sum = covers.Select(el => el.Value).DefaultIfEmpty()
					.Aggregate((total, next) => total + next);

			return sum;
		}
	}

	private async void Start()
	{
		grid = NodeGrid.Instance;
		gameManger = GameStateManager.Instance;
		unit = GetComponent<PlayerStateManager>();
		unit.CoverBihaviour = this;
		myLastPosition = unit.currentPos;
		//	if (gameManger.SelectedUnit != unit)
		//	{
		//		await RotateToward(gameManger.SelectedUnit, 1, 1);
		//		selectedUnitLastPosition = gameManger.SelectedUnit.currentPos;
		//		UpdateNorthPositionTowardTarget(gameManger.SelectedUnit);
		//		CalculateCoverValue();
		//		unit.UpdateDirectionTowardTarget(gameManger.SelectedUnit);
		//	}
	}

	public async Task RotateToward(Unit target, float time = 2, float speed = 5)
	{
		Vector3 dir = target.transform.position - unit.transform.position;
		await Rotate(unit.partToRotate, dir, time, speed);
	}

	private async void Update()
	{
		if (gameManger.SelectedUnit != unit && unit.State is Idel)
		{
			if (gameManger.SelectedUnit.currentPos != selectedUnitLastPosition)
			{
				await RotateToward(gameManger.SelectedUnit);
				UpdateNorthPositionTowardTarget(gameManger.SelectedUnit);
				CalculateCoverValue();
				unit.UpdateDirectionTowardTarget(gameManger.SelectedUnit);
				selectedUnitLastPosition = gameManger.SelectedUnit.currentPos;
				//Debug.Log($"we totate {name } and calculate covers toward the selected unit {gameManger.SelectedUnit} ");
			}
		}

	}

	public void UpdateNorthPositionTowardTarget(Unit target)
	{
		unit.currentPos = unit.currentPos ?? grid.getNodeFromTransformPosition(unit.transform);

		if (target == null || unit.currentPos == null)
		{
			Debug.Log($"{unit.stats.name} has {target} or {unit.currentPos} null");
			return;
		}
		Transform points = unit.partToRotate.Find("points");

		front = NodeGrid.Instance.getNodeFromTransformPosition(points.GetChild(0));
		back = NodeGrid.Instance.getNodeFromTransformPosition(points.GetChild(1));
		right = NodeGrid.Instance.getNodeFromTransformPosition(points.GetChild(2));
		left = NodeGrid.Instance.getNodeFromTransformPosition(points.GetChild(3));

		if (checkForDiagonal(front))
			front = grid.getNode(unit.currentPos.x, front.y);

		if (checkForDiagonal(back))
			back = grid.getNode(unit.currentPos.x, back.y);

		if (checkForDiagonal(right))
			right = grid.getNode(right.x, unit.currentPos.y);

		if (checkForDiagonal(left))
			left = grid.getNode(left.x, unit.currentPos.y);

		if (front != null)
		{
			//Debug.Log($"front {front}");
			front.tile.GetComponent<Renderer>().material.color = Color.yellow;
		}

		if (back != null)
		{
			//Debug.Log($"back {back}");
			back.tile.GetComponent<Renderer>().material.color = Color.red;
		}

		if (right != null)
		{
			//Debug.Log($"right {right}");
			right.tile.GetComponent<Renderer>().material.color = Color.blue;
		}

		if (left != null)
		{
			//Debug.Log($"left {left}");
			left.tile.GetComponent<Renderer>().material.color = Color.green;
		}
		//Debug.Log($"front {front}, back {back}, right {right}, left {left} player {currentPos}");
	}

	public void CalculateCoverValue()
	{
		// clearn previous Covers
		covers.Clear();

		// add new Covers
		if (front != null && front.tile.colliderOnTop != null)
		{
			if (front.tile.colliderOnTop.CompareTag("LowObstacle"))
			{
				covers.Add(new Cover(CoverDirection.front, CoverType.low, front));
			}
			else if (front.tile.colliderOnTop.CompareTag("HighObstacle"))
			{
				covers.Add(new Cover(CoverDirection.front, CoverType.high, front));
			}
		}
		if (right != null && right.tile.colliderOnTop != null)
		{
			if (right.tile.colliderOnTop.CompareTag("LowObstacle"))
			{
				covers.Add(new Cover(CoverDirection.right, CoverType.low, right));
			}
			else if (right.tile.colliderOnTop.CompareTag("HighObstacle"))
			{
				covers.Add(new Cover(CoverDirection.right, CoverType.high, right));
			}
			//Debug.Log($"{unit.name} has cover on the RIGHT node ");
		}
		if (left != null && left.tile.colliderOnTop != null)
		{
			if (left.tile.colliderOnTop.CompareTag("LowObstacle"))
			{
				covers.Add(new Cover(CoverDirection.left, CoverType.low, left));
			}
			else if (left.tile.colliderOnTop.CompareTag("HighObstacle"))
			{
				covers.Add(new Cover(CoverDirection.left, CoverType.high, left));
			}
			//Debug.Log($"{unit.name} has cover on the LEFT node ");
		}
		//Debug.Log($" {unit.name} has Cover Value => {CoverValue}");
	}

	private bool checkForDiagonal(Node node)
	{
		if (node == null) return false;
		return Mathf.Abs(unit.currentPos.x - node.x) == Mathf.Abs(unit.currentPos.y - node.y);
	}

	public async Task Rotate(Transform partToRotate, Vector3 dir, float timeToSpentTurning = 2, float speed = 1)
	{
		float timeElapsed = 0, lerpDuration = timeToSpentTurning;

		if (partToRotate == null) return;
		Quaternion startRotation = partToRotate.rotation;

		Quaternion targetRotation = Quaternion.LookRotation(dir);
		while (timeElapsed < lerpDuration)
		{
			Vector3 rotation = Quaternion.Lerp(partToRotate.rotation,
				    targetRotation,
				     timeElapsed / lerpDuration
				    )
				    .eulerAngles;
			//partToRotate.rotation = Quaternion.Slerp(startRotation, targetRotation, timeElapsed / lerpDuration);
			timeElapsed += (speed * Time.deltaTime);
			await Task.Yield();

			partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);
		}
	}
}