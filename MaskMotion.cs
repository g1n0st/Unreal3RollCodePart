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

	void InitBarrage(ref GameObject o, Color color, Vector3 dir, Vector3 pos, float sp = 2.0f, float ma = 5)
	{
		o.SetActive(true);
		o.transform.position = pos;
		o.transform.rotation = transform.rotation;
		o.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
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
		if (mask_blood <= 0)
		{
			flame.SetActive(false);
		}

		GameObject boss = GameObject.Find("Hata no Kokoro");
		KokoroMotion.KokoruState boss_state = boss.GetComponent<KokoroMotion>().current_state;
		KokoroMotion.StageState boss_stage = boss.GetComponent<KokoroMotion>().current_stage;
		int boss_runtime = boss.GetComponent<KokoroMotion>().runtime;

		float y_r = (transform.rotation.eulerAngles.y + boss.transform.rotation.eulerAngles.y) / 360.0f * 2 * Mathf.PI;
		float x = Mathf.Cos(y_r);
		float z = Mathf.Sin(y_r);
		Vector3 dir = new Vector3(x, 0, -z), homo = new Vector3(z, 0, x);
		if (boss_stage == KokoroMotion.StageState.STAGE_2 && mask_blood > 0)
		{
			if (boss_runtime % 4 == 0)
			{
				GameObject b = ObjectPool.SharedInstancePool.GetPooledObject();
				InitBarrage(ref b, color, dir, transform.position + new Vector3(0, 3.2f, 0) + homo * Mathf.Sin(boss_runtime / 75.0f * Mathf.PI) * 1.8f, 5f, 2f);
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
