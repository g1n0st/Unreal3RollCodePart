using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyBarrage : MonoBehaviour
{
	Vector3 dir;
	float dis_counter = 0;
	float speed = 0;
	float max_counter = 5;

	public void SetDirection(Vector3 d)
	{
		dir = d;
		dis_counter = 0;
	}
	public void SetMaxcount(float ma)
	{
		max_counter = ma;
	}
	public void SetSpeed(float sp)
	{
		speed = sp;
	}

    void Update()
    {
		GameObject role = GameObject.Find("Youmu Konpaku");

		float delx = role.transform.position.x - transform.position.x,
			  delz = role.transform.position.z - transform.position.z;

		float dist2 = delx * delx + delz * delz;
		if (dist2 <= (0.15f + transform.localScale.x / 2.0f) * (0.15f + transform.localScale.x / 2.0f))
		{
			if (role.GetComponent<RoleMotion>().current_state != RoleMotion.RoleState.ST_RUSH)
				;//role.SendMessageUpwards("BeAttacked");

			gameObject.SetActive(false);
			return;
		}

		transform.position += dir * Time.deltaTime * speed;
		dis_counter += Time.deltaTime;
		if (dis_counter > max_counter) gameObject.SetActive(false);
	}
}
