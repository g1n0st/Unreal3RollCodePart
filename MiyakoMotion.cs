using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiyakoMotion : MonoBehaviour
{
	const float ROLE_BARRAGE_DAMAGE = 10;
	const float ROLE_SWORD_DAMAGE = 30;
	const float CHASE_SPEED = 3f;
	const float ATTACK_RADIUS = 0.9f;
	const int DEAD_COUNT_DOWN = 500;
	const float BLOODBAR_LENGTH = 2f;

	float sword_hit_interval = 0.0f;
	const float SWORD_HIT_INTERVAL = 0.1f;
	const float HIT_BACK_DISTANCE = 0.03f;

	public Vector3 trigger_place;
	public float trigger_radius;

	public float npc_blood;
	float MAX_BLOOD;
	public Slider bloodbar;

	CharacterController cc;

	enum MiyakoState
	{
		ST_ATTACK,
		ST_CHASE,
		ST_DEAD,
		ST_IDLE,
		ST_HURT
	};
	MiyakoState current_state = MiyakoState.ST_IDLE;
	private Animator animator;
	public static AnimatorStateInfo info;
	private int anim_chase = Animator.StringToHash("Chase");
	private int anim_dead = Animator.StringToHash("Dead");
	private int anim_idle = Animator.StringToHash("Idle");
	private int anim_attack = Animator.StringToHash("Attack");
	private int anim_hurt = Animator.StringToHash("Hurt");
	private int chase_state = Animator.StringToHash("Layer.chase");
	private int dead_state = Animator.StringToHash("Layer.dead");
	private int idle_state = Animator.StringToHash("Layer.idle");
	private int attack_state = Animator.StringToHash("Layer.attack");
	private int hurt_state = Animator.StringToHash("Layer.hurt");

	void SetAnimState(MiyakoState state, int trigger, float interval = 1.0f)
	{
		if (current_state == state)
		{
			if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= interval)
				animator.SetTrigger(trigger);
		}
		else
		{
			current_state = state;
			animator.SetTrigger(trigger);
		}
	}
	void Start()
    {
		MAX_BLOOD = npc_blood;
		animator = this.GetComponent<Animator>();
		cc = GetComponent<CharacterController>();
	}
	void ShowBloodbar()
	{
		Vector3 worldPos = new Vector3(transform.position.x, transform.position.y + BLOODBAR_LENGTH, transform.position.z);
		Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
		bloodbar.transform.position = new Vector3(screenPos.x, screenPos.y, screenPos.z);
		bloodbar.GetComponent<RectTransform>().sizeDelta = new Vector2(npc_blood * 2f, 30);
	}

	int dead_count_down = -10;
    void Update()
    {
		ShowBloodbar();

		if (npc_blood <= 0.0f)
		{
			if (dead_count_down == -10)
			{
				dead_count_down = DEAD_COUNT_DOWN;
				animator.SetTrigger(anim_dead);
				Destroy(gameObject.GetComponent<Rigidbody>());
				Destroy(gameObject.GetComponent<CharacterController>());
				Destroy(gameObject.GetComponent<CapsuleCollider>());
				current_state = MiyakoState.ST_DEAD;
			}
			else if (dead_count_down <= 10) Destroy(gameObject);
			else --dead_count_down;
			return;
		}
		sword_hit_interval += Time.deltaTime;
		GameObject role = GameObject.Find("Youmu Konpaku");
		if (current_state == MiyakoState.ST_IDLE)
		{
			float sub_x = role.transform.position.x - trigger_place.x;
			float sub_z = role.transform.position.z - trigger_place.z;
			if (Mathf.Sqrt(sub_x * sub_x + sub_z * sub_z) <= trigger_radius)
			{
				current_state = MiyakoState.ST_CHASE;
				animator.SetTrigger(anim_chase);
			}
		}
		else if (current_state != MiyakoState.ST_HURT)
		{
			if (current_state == MiyakoState.ST_CHASE || current_state == MiyakoState.ST_ATTACK)
			{
				float sub_x = role.transform.position.x - transform.position.x;
				float sub_z = role.transform.position.z - transform.position.z;
				float dist = Mathf.Sqrt(sub_x * sub_x + sub_z * sub_z);

				if (Mathf.Sqrt(sub_x * sub_x + sub_z * sub_z) <= ATTACK_RADIUS)
				{
					if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
						transform.forward = new Vector3(sub_x, 0, sub_z);
					SetAnimState(MiyakoState.ST_ATTACK, anim_attack);
				}
				else if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f || current_state == MiyakoState.ST_CHASE)
				{
					transform.forward = new Vector3(sub_x, 0, sub_z);
					SetAnimState(MiyakoState.ST_CHASE, anim_chase);
					cc.Move(new Vector3(sub_x / dist, 0, sub_z / dist) * CHASE_SPEED * Time.deltaTime);
				}
			}
		}
		else
		{
			if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f)
			{
				current_state = MiyakoState.ST_CHASE;
				if (npc_blood > 0)
					animator.SetTrigger(anim_chase);
				else
					animator.SetTrigger(anim_dead);
			}
		}
	}
	private void OnTriggerEnter(Collider other)
	{
		if (npc_blood > 0)
		{
			GameObject role = GameObject.Find("Youmu Konpaku");
			if (other.name.Length >= 12 && other.name.Substring(0, 12) == "roll_barrage")
			{
				Destroy(other.transform.gameObject);
				npc_blood -= ROLE_BARRAGE_DAMAGE;

				float delx = role.transform.position.x - transform.position.x,
				  delz = role.transform.position.z - transform.position.z;
				float dist = Mathf.Sqrt(delx * delx + delz * delz);

				if (current_state != MiyakoState.ST_DEAD && npc_blood > 0)
					SetAnimState(MiyakoState.ST_HURT, anim_hurt, 0.5f);
				cc.Move(new Vector3(-delx / dist, 0, -delz / dist) * HIT_BACK_DISTANCE);
			}
			else if ((other.name.Length >= 9 && other.name.Substring(0, 9) == "LeftSword") ||
				(other.name.Length >= 10 && other.name.Substring(0, 10) == "RightSword"))
			{
				RoleMotion.RoleState role_state = role.GetComponent<RoleMotion>().current_state;
				if (sword_hit_interval > SWORD_HIT_INTERVAL
					&& (role_state == RoleMotion.RoleState.ST_ATTACK || role_state == RoleMotion.RoleState.ST_RUSH))
				{
					sword_hit_interval = 0;

					npc_blood -= ROLE_SWORD_DAMAGE;

					float delx = role.transform.position.x - transform.position.x,
					  delz = role.transform.position.z - transform.position.z;
					float dist = Mathf.Sqrt(delx * delx + delz * delz);

					if (current_state != MiyakoState.ST_DEAD && npc_blood > 0)
						SetAnimState(MiyakoState.ST_HURT, anim_hurt, 0.5f);
					cc.Move(new Vector3(-delx / dist, 0, -delz / dist) * HIT_BACK_DISTANCE * 20);
				}
			}
		}
	}
}
