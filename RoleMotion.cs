using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleMotion : MonoBehaviour {

	const float role_speed = 2f;
	const float role_rush_speed = 7f;

	// this part in charge of the animation of the role Youmu
	public enum RoleState {
		ST_ATTACK,
		ST_DEFEND,
		ST_RUSH,
        ST_MOVE,
        ST_DEAD,
        ST_IDLE
    };
	float[] unlock_ar1 = new float[]{ 0.8f, 0.8f, 0.8f };
	float[] unlock_ar2 = new float[] { 0.2f, 0.1f, 0.2f };
	public RoleState current_state = RoleState.ST_IDLE;
    private Animator animator;
    public static AnimatorStateInfo info;
    private int anim_rush = Animator.StringToHash("Rush");
    private int anim_move = Animator.StringToHash("Move");
    private int anim_dead = Animator.StringToHash("Dead");
    private int anim_idle = Animator.StringToHash("Idle");
	private int anim_attack = Animator.StringToHash("Attack");
	private int anim_defend = Animator.StringToHash("Defend");
	private int rush_state = Animator.StringToHash("Layer.rush");
    private int move_state = Animator.StringToHash("Layer.move");
    private int dead_state = Animator.StringToHash("Layer.dead");
    private int idle_state = Animator.StringToHash("Layer.idle");
    private int attack_state = Animator.StringToHash("Layer.attack");
	private int defend_state = Animator.StringToHash("Layer.defend");

	void SetAnimState(RoleState state, int trigger, string fade, float duration = 0.1f)
	{
		if (current_state == state)
		{
			if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
				animator.SetTrigger(trigger);
		}
		else
		{
			current_state = state;
			animator.CrossFade(fade, duration);
		}
	}
	// this part in charge of the movement
	CharacterController cc;
	float getAngle(Vector3 fromVector, Vector3 toVector) {
        float angle = Vector3.Angle(fromVector, toVector);
        Vector3 normal = Vector3.Cross(fromVector, toVector);
        angle *= Mathf.Sign(Vector3.Dot(normal, new Vector3(0, 1, 0)));
        return angle;
    }

    Vector3 last_direct;
    bool HandleMovement() {
        float h = Input.GetAxis("Horizontal");
        float w = Input.GetAxis("Vertical");
		if (Mathf.Abs(h) < 0.01f && Mathf.Abs(w) < 0.01f)
		{
			HandleLookAt();
			return false;
		}
		else
		{
			Vector3 direct = new Vector3(h, 0, w).normalized;
			last_direct = direct;

			cc.Move(direct * role_speed * Time.deltaTime);
			transform.rotation = Quaternion.Euler(0, getAngle(new Vector3(0, 0, 1), direct), 0);
		}
		HandleLookAt();
		return true;
    }
    void HandleLookAt() {
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1)) {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
            Vector3 mousePositionOnScreen = Input.mousePosition;
            mousePositionOnScreen.z = screenPosition.z;
            Vector3 mousePositionInWorld = Camera.main.ScreenToWorldPoint(mousePositionOnScreen);
            Vector3 look_at = new Vector3(mousePositionInWorld.x, transform.position.y, mousePositionInWorld.z);
			last_direct = (look_at - transform.position).normalized;
			transform.rotation = Quaternion.Euler(0, getAngle(new Vector3(0, 0, 1), last_direct), 0);
        }
    }

    void Start() {
        animator = this.GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
    }

    void Update() {
		GameObject.Find("Camera").transform.position = transform.position + new Vector3(0, 5.5f, -2f);
		if (Input.GetMouseButtonDown(1) && current_state != RoleState.ST_ATTACK && current_state != RoleState.ST_RUSH)
		{
			current_state = RoleState.ST_ATTACK;
			animator.SetTrigger(anim_attack);
		}
		if (Input.GetKeyDown(KeyCode.Space) && current_state != RoleState.ST_ATTACK && current_state != RoleState.ST_RUSH)
		{
			current_state = RoleState.ST_RUSH;
			animator.SetTrigger(anim_rush);
		}

		if (current_state == RoleState.ST_RUSH && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.8f)
		{
			cc.Move(last_direct * role_rush_speed * Time.deltaTime);
		}
		if (current_state == RoleState.ST_ATTACK || current_state == RoleState.ST_DEFEND || current_state == RoleState.ST_RUSH)
			if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= unlock_ar1[(int)current_state])
			{
				animator.CrossFade("idle", unlock_ar2[(int)current_state]);
				current_state = RoleState.ST_IDLE;
			}

		//if (current_state != RoleState.ST_RUSH && Input.GetKey(KeyCode.R))
		//	SetAnimState(RoleState.ST_DEFEND, anim_defend, "defend", 0.05f);

		if (current_state != RoleState.ST_RUSH) {
			if (HandleMovement() && current_state != RoleState.ST_ATTACK)
				//SetAnimState(RoleState.ST_MOVE, anim_move, move_state);
				SetAnimState(RoleState.ST_MOVE, anim_move, "move");
			else if (current_state != RoleState.ST_ATTACK)
				//SetAnimState(RoleState.ST_IDLE, anim_idle, idle_state);
				SetAnimState(RoleState.ST_IDLE, anim_idle, "idle");
		}

		if (current_state != RoleState.ST_RUSH && Input.GetKey(KeyCode.R))
			SetAnimState(RoleState.ST_DEFEND, anim_defend, "defend", 0.05f);
	}
}
