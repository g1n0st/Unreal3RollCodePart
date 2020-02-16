using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaskMotion : MonoBehaviour
{
	public Color color = new Color(1, 1, 1);
	public GameObject flame;
	public float mask_blood = 100;
	const float ROLE_BARRAGE_DAMAGE = 10;
	const float ROLE_SWORD_DAMAGE = 40;

	void InitBarrage(ref GameObject o, Color color, Vector3 dir, Vector3 pos, float sp = 2.0f, float scale = 0.5f, float ma = 2)
	{
		o.SetActive(true);
		o.transform.position = pos;
		o.transform.rotation = transform.rotation;
		o.transform.localScale = new Vector3(scale, scale, scale);
		o.GetComponent<Renderer>().material.SetColor("_TintColor", color);
		//o.GetComponent<Light>().color = color;
		o.GetComponent<FlyBarrage>().SetDirection(dir);
		o.GetComponent<FlyBarrage>().SetMaxcount(ma);
		o.GetComponent<FlyBarrage>().SetSpeed(sp);
	}
	void Start()
    {
    }
    void Update()
    {
		GameObject boss = GameObject.Find("Hata no Kokoro");
		KokoroMotion.KokoruState boss_state = boss.GetComponent<KokoroMotion>().current_state;
		KokoroMotion.StageState boss_stage = boss.GetComponent<KokoroMotion>().current_stage;
		int boss_runtime = boss.GetComponent<KokoroMotion>().runtime;

		if (mask_blood <= 0 || boss_stage == KokoroMotion.StageState.STAGE_3)
		{
			flame.SetActive(false);
		}
		float y_r = (transform.rotation.eulerAngles.y + boss.transform.rotation.eulerAngles.y) / 360.0f * 2 * Mathf.PI;
		float x = Mathf.Cos(y_r);
		float z = Mathf.Sin(y_r);
		Vector3 dir = new Vector3(x, 0, -z), homo = new Vector3(z, 0, x);
		if (boss_stage == KokoroMotion.StageState.STAGE_2 && mask_blood > 0)
		{
			if (boss_runtime % 4 == 0)
			{
				GameObject b = ObjectPool.SharedInstancePool.GetPooledObject();
				InitBarrage(ref b, color, dir, transform.position + new Vector3(0, 3.2f, 0) + homo * Mathf.Sin(boss_runtime / 75.0f * Mathf.PI) * 1.8f, 5f);
			}
		}
		if (boss_stage == KokoroMotion.StageState.STAGE_3)
		{
			if (boss_runtime >= 200)
			{
				float speed = (boss_runtime - 200) / 1200.0f;
				transform.position += dir * speed + homo * 0.01f;
			}
			else if (boss_runtime >= 180)
			{
				if (boss_runtime % 5 == 0)
				{
					GameObject b = ObjectPool.SharedInstancePool.GetPooledObject();
					InitBarrage(ref b, color, dir, transform.position + new Vector3(0, 3.2f, 0), 12f, 2.5f);
					GameObject c = ObjectPool.SharedInstancePool.GetPooledObject();
					InitBarrage(ref c, color, -dir, transform.position + new Vector3(0, 3.2f, 0), 12f, 2.5f);
				}
			}
			else if (boss_runtime > 100)
			{
				if (boss_runtime % 10 == 0)
				{
					int start = Random.Range(0, 40);
					for (int i = start; i < start + 360; i += 120)
					{
						float xx = Mathf.Cos(i);
						float zz = Mathf.Sin(i);
						GameObject b = ObjectPool.SharedInstancePool.GetPooledObject();
						InitBarrage(ref b, color, new Vector3(xx, 0, zz), transform.position + new Vector3(0, 3.2f, 0), 5f, 0.5f, 5);
					}
				}
			}
			else
			{
				float speed = boss_runtime / 1200.0f;
				transform.position -= dir * speed + homo * 0.01f;
			}
		}
	}
	private void OnTriggerEnter(Collider other)
	{
		if (mask_blood > 0)
		{
			GameObject role = GameObject.Find("Youmu Konpaku");
			if (other.name.Length >= 12 && other.name.Substring(0, 12) == "roll_barrage")
			{
				Destroy(other.transform.gameObject);
				mask_blood -= ROLE_BARRAGE_DAMAGE;
			}
			else if ((other.name.Length >= 9 && other.name.Substring(0, 9) == "LeftSword") ||
				(other.name.Length >= 10 && other.name.Substring(0, 10) == "RightSword"))
			{
				RoleMotion.RoleState role_state = role.GetComponent<RoleMotion>().current_state;
				if ((role_state == RoleMotion.RoleState.ST_ATTACK || role_state == RoleMotion.RoleState.ST_RUSH))
				{
					mask_blood -= ROLE_SWORD_DAMAGE;
				}
			}
		}
	}
}
