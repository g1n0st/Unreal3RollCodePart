using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KokoroMotion : MonoBehaviour
{
	public float ROLE_BARRAGE_DAMAGE_1 = 0.25f;
	public float ROLE_SWORD_DAMAGE_1 = 9;
	public float ROLE_BARRAGE_DAMAGE_2 = 2;
	public float ROLE_SWORD_DAMAGE_2 = 7;
	public float ROLE_BARRAGE_DAMAGE_3 = 1;
	public float ROLE_SWORD_DAMAGE_3 = 7;

	const float SWORD_HIT_INTERVAL = 0.1f;
	const float WALK_SPEED = 0.5f;
	const float RUN_SPEED = 5f;
	const float STEP_SPEED = 3f;

	public Vector3 trigger_place;
	public Vector3 stage_middle_place;
	public float boss_blood = 300, 
		stage2_blood = 200, 
		stage3_blood = 100;

	public GameObject[] masks;

	public enum KokoruState
	{
		ST_ATTACK1,
		ST_ATTACK2,
		ST_ATTACK3,
		ST_SUSPEND,
		ST_WALK,
		ST_CHARM,
		ST_RUN,
		ST_STEP,
		ST_DEAD,
		ST_IDLE
	};
	public enum StageState
	{
		STAGE_1,
		STAGE_2,
		STAGE_3,
		STAGE_0
	};
	public int runtime = 0;
	public KokoruState current_state = KokoruState.ST_IDLE;
	public StageState current_stage = StageState.STAGE_0;
	Animator animator;
	AnimatorStateInfo info;
	CharacterController cc;
	int anim_attack1 = Animator.StringToHash("Attack1");
	int anim_attack2 = Animator.StringToHash("Attack2");
	int anim_attack3 = Animator.StringToHash("Attack3");
	int anim_dead = Animator.StringToHash("Dead");
	int anim_idle = Animator.StringToHash("Idle");
	int anim_walk = Animator.StringToHash("Walk");
	int anim_step = Animator.StringToHash("Step");
	int anim_run = Animator.StringToHash("Run");
	int anim_charm = Animator.StringToHash("Charm");

	int attack1_state = Animator.StringToHash("Layer.attack1");
	int attack2_state = Animator.StringToHash("Layer.attack2");
	int attack3_state = Animator.StringToHash("Layer.attack3");
	int dead_state = Animator.StringToHash("Layer.dead");
	int idle_state = Animator.StringToHash("Layer.idle");
	int walk_state = Animator.StringToHash("Layer.walk");
	int step_state = Animator.StringToHash("Layer.step");
	int run_state = Animator.StringToHash("Layer.run");
	int charm_state = Animator.StringToHash("Layer.charm");

	void SetAnimState(KokoruState state, int trigger)
	{
		if (current_state == state)
		{
			if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
				animator.SetTrigger(trigger);
		}
		else
		{
			current_state = state;
			animator.SetTrigger(trigger);
			//animator.CrossFade(fade, duration);
		}
	}

	const float ATTACK_RANGE_0 = 3.5f;
	const float ATTACK_RANGE_1 = 0.8f;
	const float ATTACK1_SPEED = 2f;
	const float ATTACK2_SPEED = 0.5f;
	const float ATTACK3_SPEED = 0.5f;
	void Stage1(float delx, float delz, float dist)
	{
		float duration = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		if ((dist > ATTACK_RANGE_0 && duration >= 0.98f) ||
			(current_state == KokoruState.ST_RUN && duration < 0.1f))
		{
			transform.forward = new Vector3(delx, 0, delz);
			cc.Move(new Vector3(delx / dist, 0, delz / dist) * RUN_SPEED * Time.deltaTime);
			if (current_state != KokoruState.ST_RUN)
			{
				current_state = KokoruState.ST_RUN;
				animator.SetTrigger(anim_run);
			}
		}
		else if ((dist < ATTACK_RANGE_1 && Random.Range(0, 2) == 1 && duration >= 1.0f) || 
			(current_state == KokoruState.ST_STEP && duration < 1.0f))
		{
			transform.forward = new Vector3(delx, 0, delz);
			cc.Move(new Vector3(-delx / dist, 0, -delz / dist) * STEP_SPEED * Time.deltaTime);
			if (current_state != KokoruState.ST_STEP)
			{
				current_state = KokoruState.ST_STEP;
				animator.SetTrigger(anim_step);
			}
		}
		else
		{
			if ((current_state == KokoruState.ST_RUN && duration >= 0.1f) || duration >= 0.99f)
			{
				int attack = Random.Range(0, 3);
				transform.forward = new Vector3(delx, 0, delz);
				if (attack == 0) { current_state = KokoruState.ST_ATTACK1; animator.SetTrigger(anim_attack1); }
				if (attack == 1) { current_state = KokoruState.ST_ATTACK2; animator.SetTrigger(anim_attack2); }
				if (attack == 2) { current_state = KokoruState.ST_ATTACK3; animator.SetTrigger(anim_attack3); }
			}
			if (current_state == KokoruState.ST_ATTACK1) cc.Move(new Vector3(delx / dist, 0, delz / dist) * ATTACK1_SPEED * Time.deltaTime);
			if (current_state == KokoruState.ST_ATTACK2) cc.Move(new Vector3(delx / dist, 0, delz / dist) * ATTACK2_SPEED * Time.deltaTime);
			if (current_state == KokoruState.ST_ATTACK3) cc.Move(new Vector3(delx / dist, 0, delz / dist) * ATTACK3_SPEED * Time.deltaTime);
		}
	}
	void Stage2(float delx, float delz, float dist)
	{
		if (--runtime <= 0)
		{
			runtime = 300;
			transform.position = stage_middle_place + new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
			transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
		}

	}
	void Stage3(float delx, float delz, float dist)
	{
		if (--runtime <= 0)
		{
			runtime = 300;
		}
	}

	float sword_hit_interval = 0;
	void Start()
    {
		animator = GetComponent<Animator>();
		cc = GetComponent<CharacterController>();
	}
    void Update()
    {
		GameObject role = GameObject.Find("Youmu Konpaku");
		info = animator.GetCurrentAnimatorStateInfo(0);
		float delx = role.transform.position.x - transform.position.x,
			  delz = role.transform.position.z - transform.position.z;
		float dist = Mathf.Sqrt(delx * delx + delz * delz);

		if (boss_blood < 0)
		{
			if (current_state != KokoruState.ST_DEAD)
			{
				current_state = KokoruState.ST_DEAD;
				for (int i = 0; i < 8; i++)
					masks[i].SetActive(false);
				Destroy(gameObject.GetComponent<Rigidbody>());
				Destroy(gameObject.GetComponent<CapsuleCollider>());
				animator.SetTrigger(anim_dead);
			}
		}
		else if (current_state == KokoruState.ST_IDLE)
		{
			float delx1 = role.transform.position.x - trigger_place.x,
				dely1 = role.transform.position.y - trigger_place.y,
			delz1 = role.transform.position.z - trigger_place.z;
			float dist1 = Mathf.Sqrt(delx1 * delx1 + dely1 * dely1 + delz1 * delz1);
			if (dist1 <= 3 || dist <= 3)
			{
				current_state = KokoruState.ST_RUN;
				animator.SetTrigger(anim_run);
			}
		}
		else
		{
			if (boss_blood > stage2_blood)
			{
				if (current_stage != StageState.STAGE_1)
				{
					current_stage = StageState.STAGE_1;
					runtime = 0;
				}
				Stage1(delx, delz, dist);
			}
			else if (boss_blood > stage3_blood)
			{
				if (current_stage != StageState.STAGE_2)
				{
					current_stage = StageState.STAGE_2;
					runtime = 0;
					animator.SetTrigger(anim_charm);
					for (int i = 0; i < 8; i++)
						masks[i].SetActive(true);
				}
				if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f) animator.SetTrigger(anim_charm);
				int dead_masks = 0;
				for (int i = 0; i < 8; i++)
					if (masks[i].GetComponent<MaskMotion>().mask_blood <= 0) dead_masks++;
				if (dead_masks == 8) boss_blood = stage3_blood - 1;
				else Stage2(delx, delz, dist);
			}
			else
			{
				if (current_stage != StageState.STAGE_3)
				{
					current_stage = StageState.STAGE_3;
					runtime = 0;
				}
				if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f) animator.SetTrigger(anim_charm);
				Stage3(delx, delz, dist);
			}
		}
		sword_hit_interval += Time.deltaTime;
	}
	private void OnTriggerEnter(Collider other)
	{
		GameObject role = GameObject.Find("Youmu Konpaku");
		if (other.name.Length >= 12 && other.name.Substring(0, 12) == "roll_barrage")
		{
			Destroy(other.transform.gameObject);
			if (current_stage == StageState.STAGE_1) boss_blood -= ROLE_BARRAGE_DAMAGE_1;
			if (current_stage == StageState.STAGE_2) boss_blood -= ROLE_BARRAGE_DAMAGE_2;
			if (current_stage == StageState.STAGE_3) boss_blood -= ROLE_BARRAGE_DAMAGE_3;

			float delx = role.transform.position.x - transform.position.x,
			  delz = role.transform.position.z - transform.position.z;
			float dist = Mathf.Sqrt(delx * delx + delz * delz);

			//cc.Move(new Vector3(-delx / dist, 0, -delz / dist) * HIT_BACK_DISTANCE);
		}
		else if ((other.name.Length >= 9 && other.name.Substring(0, 9) == "LeftSword") ||
			(other.name.Length >= 10 && other.name.Substring(0, 10) == "RightSword"))
		{
			RoleMotion.RoleState role_state = role.GetComponent<RoleMotion>().current_state;
			if (sword_hit_interval > SWORD_HIT_INTERVAL
				&& (role_state == RoleMotion.RoleState.ST_ATTACK || role_state == RoleMotion.RoleState.ST_RUSH))
			{
				sword_hit_interval = 0;

				if (current_stage == StageState.STAGE_1) boss_blood -= ROLE_SWORD_DAMAGE_1;
				if (current_stage == StageState.STAGE_2) boss_blood -= ROLE_SWORD_DAMAGE_2;
				if (current_stage == StageState.STAGE_3) boss_blood -= ROLE_SWORD_DAMAGE_3;

				float delx = role.transform.position.x - transform.position.x,
				  delz = role.transform.position.z - transform.position.z;
				float dist = Mathf.Sqrt(delx * delx + delz * delz);

				//cc.Move(new Vector3(-delx / dist, 0, -delz / dist) * HIT_BACK_DISTANCE * 20);
			}
		}
	}
}
