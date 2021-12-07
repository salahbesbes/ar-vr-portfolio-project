using System.Linq;
using UnityEngine;

public class PlayerTurn : AnyState<GameStateManager>
{
	private Color InitColor;

	public override AnyClass EnterState(GameStateManager gameManager)
	{
		gameManager.SelectedPlayer = gameManager.players.FirstOrDefault();
		gameManager.SelectedEnemy = gameManager.enemies.FirstOrDefault();

		gameManager.SelectedPlayer.enabled = true;
		gameManager.SelectedPlayer.fpsCam.enabled = true;

		//Debug.Log($" PlayerTurn Start ,  {gameManager.selectedPlayer.transform.name} is selected");
		gameManager.UpdateSelectedPlayerResponse(gameManager.SelectedPlayer);

		return gameManager.SelectedPlayer;
	}

	public override void Update(GameStateManager gameManager)
	{
		//currentPos = grid.getNodeFromTransformPosition(transform);

		gameManager.SelectedPlayer.currentPos = gameManager.grid.getNodeFromTransformPosition(gameManager.SelectedPlayer.transform);
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			SelectNextPlayer(gameManager);
		}

		gameManager.SelectedPlayer.LockOnTarger();
		gameManager.SelectedPlayer.checkFlank(gameManager?.SelectedEnemy?.currentPos);
		//gameManager.SelectedPlayer.CheckMovementRange();
		//gameManager.SelectedPlayer.onNodeHover();
	}

	public override void ExitState(GameStateManager gameManager)
	{
		if (gameManager.SelectedPlayer != null)
		{
			//gameManager.SelectedPlayer.enabled = false;
			//gameManager.SelectedPlayer = null;
			Debug.Log($"exit State {nameof(PlayerTurn)}");
		}
	}

	public void SelectNextPlayer(GameStateManager gameManager)
	{
		int nbPlayers = gameManager.players.Count;

		if (gameManager != null)
		{
			gameManager.SelectedPlayer.enabled = false;
			gameManager.SelectedPlayer.SwitchState(gameManager.SelectedPlayer.idelState);
			gameManager.SelectedPlayer.fpsCam.enabled = false;
			int currentPlayerIndex = gameManager.players.FindIndex(instance => instance == gameManager.SelectedPlayer);

			gameManager.SelectedPlayer = gameManager.players[(currentPlayerIndex + 1) % nbPlayers];
			gameManager.SelectedPlayer.enabled = true;
			gameManager.SelectedPlayer.fpsCam.enabled = true;

			gameManager.UpdateSelectedPlayerResponse(gameManager.SelectedPlayer);

			Debug.Log($"Selected  {gameManager.SelectedPlayer} ");
		}
	}
}

public class EnemyTurn : AnyState<GameStateManager>
{
	public override AnyClass EnterState(GameStateManager gameManager)
	{
		gameManager.SelectedPlayer = gameManager.players.FirstOrDefault();
		gameManager.SelectedEnemy = gameManager.enemies.FirstOrDefault();

		gameManager.SelectedEnemy.enabled = true;
		//Debug.Log($" EnemyTurn Start ,  {gameManager.selectedEnemy.transform.name} is selected");
		return gameManager.SelectedEnemy;
	}

	public override void Update(GameStateManager gameManager)
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			Debug.Log($"tab clicked");
			SelectNextEnemy(gameManager);
		}

		if (Input.GetKeyDown(KeyCode.LeftShift))
		{
			Debug.Log($"shoft clicked");

			gameManager.SelectedEnemy.SelectNextPlayer();
		}

		if (Input.GetMouseButtonDown(0))
		{
			//gameManager.selectedEnemy.CreateNewMoveAction();
		}
		if (Input.GetMouseButtonDown(1))
		{
			//gameManager.selectedEnemy.CreateNewShootAction();

			gameManager.GetComponent<PlayerEventListener>();
		}
		if (Input.GetKeyDown(KeyCode.R))
		{
			gameManager.SelectedEnemy.CreateNewReloadAction();
		}

		gameManager.SelectedEnemy.CheckMovementRange();

		gameManager.SelectedEnemy.LockOnTarger();
		//gameManager.selectedEnemy.checkFlank(gameManager.selectedPlayer.currentPos);
	}

	public override void ExitState(GameStateManager gameManager)
	{
		if (gameManager.SelectedEnemy != null)
		{
			//gameManager.SelectedEnemy.enabled = false;
			//gameManager.SelectedEnemy = null;
			Debug.Log($"exit State {nameof(EnemyTurn)}");
		}
	}

	public void SelectNextEnemy(GameStateManager gameManager)
	{
		int nbEnemies = gameManager.enemies.Count;

		if (gameManager != null)
		{
			gameManager.SelectedEnemy.enabled = false;
			int currentEnemyIndex = gameManager.enemies.FindIndex(instance => instance == gameManager.SelectedEnemy);

			gameManager.SelectedEnemy = gameManager.enemies[(currentEnemyIndex + 1) % nbEnemies];
			gameManager.SelectedEnemy.enabled = true;

			gameManager.UpdateSelectedPlayerResponse(gameManager.SelectedEnemy);
			Debug.Log($" {gameManager.SelectedEnemy.transform.name} is Selected  {gameManager.SelectedEnemy} ");
		}
	}
}