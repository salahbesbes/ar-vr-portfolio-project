public class PlayerStateManager : AnyClass
{
	public SelectingEnemy selectingEnemy = new SelectingEnemy();
	public Idel idelState = new Idel();
	public DoingAction doingAction = new DoingAction();
	public Dead dead = new Dead();

	public AnimationType currentActionAnimation = AnimationType.idel;

	//private void Awake()
	//{
	//	//unit = GetComponent<AnyClass>();
	//	SwitchState(idelState);

	// //Debug.Log($"start of player state manager ");

	//}
	private void OnEnable()
	{
		SwitchState(idelState);
	}

	private BaseState<PlayerStateManager> _State;

	public BaseState<PlayerStateManager> State
	{
		get => _State;
		private set
		{
			_State = value;
		}
	}

	private void Awake()
	{
	}

	private void Update()
	{
		//currentPos = grid.getNodeFromTransformPosition(transform);
		State.Update(this);
	}

	public void SwitchState(BaseState<PlayerStateManager> newState, AnimationType? anim = null)
	{
		State?.ExitState(this);
		State = newState;
		State.EnterState(this);
	}
}