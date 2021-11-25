using UnityEngine;

public class DoingAction : AnyState<PlayerStateManager>
{
	public override void EnterState(PlayerStateManager player)
	{
		player.State.name = "doingAction";
		Debug.Log($"current state : {player.State.name}");
	}

	public override void Update(PlayerStateManager player)
	{
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			player.SwitchState(player.idelState);
		}
	}

	public override void ExitState(PlayerStateManager player)
	{
	}
}