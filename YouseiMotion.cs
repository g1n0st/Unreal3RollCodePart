using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YouseiMotion : MonoBehaviour
{
	const float ROLE_BARRAGE_DAMAGE = 10;
	const float ROLE_SWORD_DAMAGE = 30;
	const float BLOODBAR_LENGTH = 2f;

	float sword_hit_interval = 0.0f;
	const float SWORD_HIT_INTERVAL = 0.1f;
	const float HIT_BACK_DISTANCE = 0.03f;
	public float WALK_SPEED = 1.5f;
	public float RUN_SPEED = 2f;

	public float ATTACK_A_SPEED = 1.9f;
	public float ATTACK_A_WIDTH = 1.7f;
	public int ATTACK_B_INTERVAL_SIZE = 24;
	public int ATTACK_B_INTERVAL_TIME = 25;
	public float ATTACK_B_SPEED = 2.0f;
	public int ATTACK_C_INTERVAL = 6;
	public float ATTACK_C_SPEED = 2.0f;
	
	public Slider bloodbar;

	CharacterController cc;

	private Animator animator;
	AnimatorStateInfo info;

	int ani_idle = Animator.StringToHash("Idle");
	int ani_run = Animator.StringToHash("Run");
	int ani_walk = Animator.StringToHash("Walk");
	int ani_dead = Animator.StringToHash("Dead");
	int ani_attack = Animator.StringToHash("Attack");

	int idle_state = Animator.StringToHash("Later.idle");
	int walk_state = Animator.StringToHash("Layer.walk");
	int run_state = Animator.StringToHash("Layer.rush");
	int dead_state = Animator.StringToHash("Layer.dead");
	int attack_state = Animator.StringToHash("Layer.attack");

	public float npc_blood = 100;

	float runtime = 0;
	const float WALK_RUNTIME = 1, 
		RUN_RUNTIME = 1, 
		ATTACK_A_RUNTIME = 5, 
		ATTACK_B_RUNTIME = 5, 
		ATTACK_C_RUNTIME = 5;

	enum YouseiState
	{
		ST_IDLE,
		ST_WALK,
		ST_RUN,
		ST_ATTACK_A,
		ST_ATTACK_B,
		ST_ATTACK_C,
		ST_DEAD
	};
	YouseiState current_state = YouseiState.ST_IDLE;

	public Vector3 trigger_place;
	public float trigger_radius;

	// color var
	public Color grd;
	public Color brd;
	public Color crd;

	void InitBarrage(ref GameObject o, Color color, Vector3 dir, Vector3 pos, float sp = 2.0f, int ma = 5)
	{
		o.SetActive(true);
		o.transform.position = pos;
		o.transform.rotation = transform.rotation;
		o.GetComponent<Renderer>().material.SetColor("_TintColor", color);
		//o.GetComponent<Light>().color = color;
		o.GetComponent<FlyBarrage>().SetDirection(dir);
		o.GetComponent<FlyBarrage>().SetMaxcount(ma);
		o.GetComponent<FlyBarrage>().SetSpeed(sp);
	}
	void SetAnimState(YouseiState state, int trigger)
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
		}
	}
	void WalkMotion(float dist, float delx, float delz)
	{
		SetAnimState(YouseiState.ST_WALK, ani_walk);

		transform.forward = new Vector3(delx, 0, delz);
		cc.Move(new Vector3(delx / dist, 0, delz / dist) * WALK_SPEED * Time.deltaTime);
		runtime -= Time.deltaTime;
	}
	void RunMotion(float dist, float delx, float delz)
	{
		SetAnimState(YouseiState.ST_RUN, ani_run);

		transform.forward = new Vector3(delx, 0, delz);
		cc.Move(new Vector3(delx / dist, 0, delz / dist) * RUN_SPEED * Time.deltaTime);
		runtime -= Time.deltaTime;
	}
	void AttackAMotion(float dist, float delx, float delz)
	{
		SetAnimState(YouseiState.ST_ATTACK_A, ani_attack);

		delx /= dist; delz /= dist;
		transform.forward = new Vector3(delx, 0, delz);
		int rt = (int)((runtime / ATTACK_A_RUNTIME) * 150);
		if (rt % 30 == 0)
		{
			float width = ATTACK_A_WIDTH;
			float speed = ATTACK_A_SPEED;
			GameObject a, b, c;
			a = ObjectPool.SharedInstancePool.GetPooledObject();
			InitBarrage(ref a, brd, new Vector3(delx * speed, 0, delz * speed), transform.position + new Vector3(-delz * width, 0.7f, delx * width), 7.0f);

			b = ObjectPool.SharedInstancePool.GetPooledObject();
			InitBarrage(ref b, brd, new Vector3(delx * speed, 0, delz * speed), transform.position + new Vector3(delz * width, 0.7f, -delx * width), 7.0f);

			c = ObjectPool.SharedInstancePool.GetPooledObject();
			InitBarrage(ref c, brd, new Vector3(delx * speed, 0, delz * speed), transform.position + new Vector3(0, 0.7f, 0), 7.0f);
		}
		runtime -= Time.deltaTime;
	}
	void AttackBMotion(float dist, float delx, float delz)
	{
		SetAnimState(YouseiState.ST_ATTACK_B, ani_attack);
		transform.forward = new Vector3(delx, 0, delz);

		int rt = (int)((runtime / ATTACK_A_RUNTIME) * 150);
		if (rt % ATTACK_B_INTERVAL_TIME == 0)
		{
			GameObject b;
			for (int degree = (rt % 40) / 4; degree < 360; degree += ATTACK_B_INTERVAL_SIZE)
			{
				float x = Mathf.Cos(degree * Mathf.Deg2Rad);
				float z = Mathf.Sin(degree * Mathf.Deg2Rad);
				b = ObjectPool.SharedInstancePool.GetPooledObject();
				InitBarrage(ref b, grd, new Vector3(x, 0, z), transform.position + new Vector3(0, 0.7f, 0), ATTACK_B_SPEED);
			}
		}
		runtime -= Time.deltaTime;
	}
	void AttackCMotion(float dist, float delx, float delz)
	{
		SetAnimState(YouseiState.ST_ATTACK_C, ani_attack);
		transform.forward = new Vector3(delx, 0, delz);

		int rt = (int)((runtime / ATTACK_A_RUNTIME) * 150);
		if (rt % ATTACK_C_INTERVAL == 0)
		{
			GameObject b;
			float degree = Random.Range(0.0f, 1.0f) * 360;
			float x = Mathf.Cos(degree * Mathf.Deg2Rad);
			float z = Mathf.Sin(degree * Mathf.Deg2Rad);
			b = ObjectPool.SharedInstancePool.GetPooledObject();
			InitBarrage(ref b, crd, new Vector3(x, 0, z), transform.position + new Vector3(0, 0.7f, 0), ATTACK_C_SPEED);
		}

		runtime -= Time.deltaTime;
	}
	public bool permit_attack_A = true;
	public bool permit_attack_B = true;
	public bool permit_attack_C = true;
	void AttackChoose(float dist, float delx, float delz)
	{
		if (!permit_attack_A && !permit_attack_B && !permit_attack_C) return;
		while (true)
		{
			int tr = Random.Range(0, 3);
			if (tr == 0 && permit_attack_A)
			{
				runtime = ATTACK_A_RUNTIME;
				AttackAMotion(dist, delx, delz);
				break;
			}
			else if (tr == 1 && permit_attack_B)
			{
				runtime = ATTACK_B_RUNTIME;
				AttackBMotion(dist, delx, delz);
				break;
			}
			else if (permit_attack_C)
			{
				runtime = ATTACK_C_RUNTIME;
				AttackCMotion(dist, delx, delz);
				break;
			}
		}
	}

	void ShowBloodbar()
	{
		Vector3 worldPos = new Vector3(transform.position.x, transform.position.y + BLOODBAR_LENGTH, transform.position.z);
		Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
		bloodbar.transform.position = new Vector3(screenPos.x, screenPos.y, screenPos.z);
		bloodbar.GetComponent<RectTransform>().sizeDelta = new Vector2(npc_blood * 2f, 30);
	}
	void Start()
	{
		cc = GetComponent<CharacterController>();
		animator = this.GetComponent<Animator>();
	}
	int dead_countdown = 0;
	void Update()
	{
		ShowBloodbar();
		GameObject role = GameObject.Find("Youmu Konpaku");
		info = animator.GetCurrentAnimatorStateInfo(0);
		sword_hit_interval += SWORD_HIT_INTERVAL;

		if (npc_blood <= 0 && current_state != YouseiState.ST_DEAD)
		{
			// HP++
			bloodbar.gameObject.SetActive(false);
			Destroy(gameObject.GetComponent<Rigidbody>());
			Destroy(gameObject.GetComponent<CapsuleCollider>());
			current_state = YouseiState.ST_DEAD;
			animator.SetTrigger(ani_dead);
			dead_countdown = 200;
		}
		if (current_state == YouseiState.ST_DEAD)
		{
			if (--dead_countdown == 0) Destroy(gameObject);
			return;
		}

		float delx = role.transform.position.x - transform.position.x,
			  delz = role.transform.position.z - transform.position.z;
		float dist = Mathf.Sqrt(delx * delx + delz * delz);

		if (current_state != YouseiState.ST_IDLE)
		{
			if (runtime > 0)
			{
				if (current_state == YouseiState.ST_WALK) WalkMotion(dist, delx, delz);
				else if (current_state == YouseiState.ST_RUN) RunMotion(dist, delx, delz);
				else if (current_state == YouseiState.ST_ATTACK_A) AttackAMotion(dist, delx, delz);
				else if (current_state == YouseiState.ST_ATTACK_B) AttackBMotion(dist, delx, delz);
				else if (current_state == YouseiState.ST_ATTACK_C) AttackCMotion(dist, delx, delz);
			}
			else
			{
				if (dist <= 3) AttackChoose(dist, delx, delz);
				else if (dist >= 5)
				{
					runtime = RUN_RUNTIME;
					RunMotion(dist, delx, delz);
				}
				else
				{
					float lerp = (dist - 3.0f) / 2.0f;
					if (Random.Range(0.0f, 1.0f) <= lerp)
					{
						if (Random.Range(0.0f, 1.0f) <= lerp)
						{
							runtime = RUN_RUNTIME;
							RunMotion(dist, delx, delz);
						}
						else
						{
							runtime = WALK_RUNTIME;
							WalkMotion(dist, delx, delz);
						}
					}
					else AttackChoose(dist, delx, delz);
				}
			}
		}
		else
		{
			float delx1 = role.transform.position.x - trigger_place.x,
				  delz1 = role.transform.position.z - trigger_place.z;
			float dist1 = Mathf.Sqrt(delx1 * delx1 + delz1 * delz1);
			if (dist1 <= trigger_radius || dist <= trigger_radius)
			{
				current_state = YouseiState.ST_WALK;
				runtime = -1;
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		GameObject role = GameObject.Find("Youmu Konpaku");
		if (other.name.Length >= 12 && other.name.Substring(0, 12) == "roll_barrage")
		{
			Destroy(other.transform.gameObject);
			npc_blood -= ROLE_BARRAGE_DAMAGE;

			float delx = role.transform.position.x - transform.position.x,
			  delz = role.transform.position.z - transform.position.z;
			float dist = Mathf.Sqrt(delx * delx + delz * delz);

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

				cc.Move(new Vector3(-delx / dist, 0, -delz / dist) * HIT_BACK_DISTANCE * 20);
			}
		}
	}
}
