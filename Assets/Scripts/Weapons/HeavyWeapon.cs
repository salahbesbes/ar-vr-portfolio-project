using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class HeavyWeapon : Weapon
{
	public GrenadeLuncher weaponType;
	public LineRenderer lr;
	public int resolution = 30;

	private void Start()
	{
		//bulletLeft = maxMagazine;
		Vector3 fwd = transform.TransformDirection(Vector3.forward);
		Debug.DrawRay(startPoint.position, fwd, Color.green);
		lr = GetComponent<LineRenderer>();
	}
	private void OnEnable()
	{
		lr.enabled = true;
	}
	private void OnDisable()
	{
		lr.positionCount = 0;
		lr.enabled = false;

	}
	public override async Task Reload(ReloadAction reload)
	{
		Debug.Log($"start reloading");
		//yield return new WaitForSeconds(2f);
		weaponType.bulletLeft = weaponType.maxMagazine;
		weaponType.reloading = false;
		Debug.Log($"finish reloading");
		player.FinishAction(reload);
		await Task.Yield();
	}

	private void lunchToWard(Rigidbody ball, Vector3 targetPos, float weaponMaxAltitude, float ammoSpeed = 1)
	{
		//Vector3 tmpGravity = Physics.gravity;
		Physics.gravity = -Vector3.up * ammoSpeed;
		LunchData lunchData = calculateLunchVelocity(targetPos, weaponMaxAltitude, Physics.gravity.y);
		ball.velocity = lunchData.initialVelocity;
		//Physics.gravity = tmpGravity;
	}

	private LunchData calculateLunchVelocity(Vector3 target, float weaponMaxAltitude, float gravity)
	{
		float displacmentY = target.y - startPoint.position.y;
		Vector3 displacementXZ = new Vector3(target.x - startPoint.position.x, 0, target.z - startPoint.position.z);
		Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * weaponMaxAltitude);
		float timeToreachDestination = (Mathf.Sqrt(-2 * weaponMaxAltitude / gravity) + Mathf.Sqrt(2 * (displacmentY - weaponMaxAltitude) / gravity));
		Vector3 velocityXZ = displacementXZ / timeToreachDestination;
		return new LunchData(velocityXZ + velocityY, timeToreachDestination);
	}

	public async Task rotateWeaponAndLunch(Transform partToRotate, float angle, float timeToSpentTurning = 0.5f)
	{
		float speed = 3;
		float timeElapsed = 0, lerpDuration = timeToSpentTurning;

		if (partToRotate == null) return;

		//Quaternion targetRotation = player.transform.rotation * Quaternion.Euler(dir);
		//Quaternion targetRotation = Quaternion.LookRotation(dir);

		while (timeElapsed < lerpDuration)
		{
			float res;
			if (angle < 0) res = Mathf.Lerp(angle, 0, timeElapsed / lerpDuration);
			else res = Mathf.Lerp(0, angle, timeElapsed / lerpDuration);
			timeElapsed += (speed * Time.deltaTime);
			//Debug.Log($"rotating");
			await Task.Yield();
			partToRotate.Rotate(Vector3.right, res);
		}
	}

	public void DrowTrajectory(Vector3 targetPos)
	{
		lr.positionCount = resolution;
		Physics.gravity = -Vector3.up * weaponType.ammoSpeed;
		LunchData data = calculateLunchVelocity(targetPos, weaponType.bouncingForce, Physics.gravity.y);
		Vector3 previousDrowPoint = startPoint.position;
		for (int i = 0; i < resolution; i++)
		{
			float simulationTime = i / (float)resolution * data.timeToTarget;
			Vector3 displacement = data.initialVelocity * simulationTime + Physics.gravity * simulationTime * simulationTime / 2f;
			Vector3 drowPoint = startPoint.position + displacement;
			lr.SetPosition(i, drowPoint);
			Debug.DrawLine(previousDrowPoint, drowPoint, Color.green);
			previousDrowPoint = drowPoint;
		}
	}

	public async void lunchGrenade(LunchGrenadeAction action, Node dest)
	{
		Quaternion Ori = transform.rotation;
		await rotateWeaponAndLunch(transform, -10);
		Grenade grenade = Instantiate(weaponType.ammo, startPoint.position, Quaternion.identity);
		Rigidbody rb = grenade.GetComponent<Rigidbody>();
		lunchToWard(rb, dest.coord, weaponType.bouncingForce, weaponType.ammoSpeed);
		transform.rotation = Ori;
		player.FinishAction(action);
	}

	public override void onHover()
	{
		Node potentialDestination = NodeGrid.Instance.getNodeFromMousePosition(player.secondCam);
		if (potentialDestination != null && potentialDestination != player.destination && potentialDestination != player.currentPos)
		{
			//lineConponent.SetUpLine(turnPoints);

			potentialDestination.tile.hightLight(Color.blue);
			DrowTrajectory(potentialDestination.coord);
			if (Input.GetMouseButtonDown(0))
			{
				ActionData action = player.actions.FirstOrDefault((el) => el is LunchGrenadeData);
				if (action == null || player.stats.unit.ActionPoint <= 1)
				{
					GameStateManager.Instance.notifyCanvas.Raise("No Action Point left");
					return;
				}
				player.currentActionAnimation = AnimationType.shoot;
				player.SwitchState(player.doingAction);
				action?.Actionevent?.Raise();
			}
		}
	}

	public override void onUpdate()
	{
		onHover();
	}

	public struct LunchData
	{
		public readonly Vector3 initialVelocity;
		public readonly float timeToTarget;

		public LunchData(Vector3 initialVelocity, float time)
		{
			this.initialVelocity = initialVelocity;
			timeToTarget = time;
		}
	}
}