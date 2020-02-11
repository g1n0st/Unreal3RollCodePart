using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulMotion : MonoBehaviour
{
	const float SOUL_MAX_MOVE_DISTANCE = 3.0f;
	const float SOUL_MAX_STAY_DISTANCE = 4.0f;
	Vector3 STANDARD_OFFSET = new Vector3(0.5f, 1.1f, 0.2f);
	//const float SOUL_MOVE_BACK_SPEED = 0.1f;
	//const float SOUL_MOVE_TO_SPEED = 0.1f;

	bool on_shot = false;
	float shot_interval = 0.0f;
	const float SHOT_INTERVAL = 0.1f;
	const float SHOT_SPEED = 10.0f;
	public GameObject barrage;

	enum SoulState
	{
		//ST_MOVE_TO,
		//ST_MOVE_BACK,
		ST_FOLLOW,
		ST_STAY
	};
	SoulState current_state = SoulState.ST_FOLLOW;
	//Vector3 destination;
	float Length(Vector3 vec)
	{
		return Mathf.Sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z);
	}
	Vector3 getLookAt()
	{
		Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
		Vector3 mousePositionOnScreen = Input.mousePosition;
		mousePositionOnScreen.z = screenPosition.z;
		Vector3 mousePositionInWorld = Camera.main.ScreenToWorldPoint(mousePositionOnScreen);
		Vector3 look_at = new Vector3(mousePositionInWorld.x, transform.position.y, mousePositionInWorld.z);
		return look_at;
	}
    void Start()
    {
        
    }
	void HandleShoot(Vector3 role_pos)
	{
		if (Input.GetMouseButtonDown(0))
		{
			on_shot = true;
		}
		if (Input.GetMouseButtonUp(0))
		{
			on_shot = false;
			shot_interval = 0.0f;
		}

		shot_interval += Time.deltaTime;
		if (on_shot && shot_interval > SHOT_INTERVAL)
		{
			shot_interval = 0.0f;
			GameObject b = GameObject.Instantiate(barrage, transform.position, transform.rotation);
			Vector3 dir = (getLookAt() - transform.position).normalized;
			b.GetComponent<FlyRollBarrage>().SetDirection(dir);
		}
	}
	void Update()
	{
		Vector3 role_pos = GameObject.Find("Youmu Konpaku").transform.position;

		if (Input.GetKeyDown(KeyCode.Q))
		{
			Vector3 offset = getLookAt() - role_pos;

			offset.y = 0;
			if (Length(offset) > SOUL_MAX_MOVE_DISTANCE) offset = offset.normalized * SOUL_MAX_MOVE_DISTANCE;
			offset.y = 1.1f;

			//current_state = Soulstate.ST_MOVE_TO;
			current_state = SoulState.ST_STAY;
			transform.position = role_pos + offset;
		}

		Vector3 distance = role_pos - transform.position;
		if (Length(distance) > SOUL_MAX_STAY_DISTANCE) current_state = SoulState.ST_FOLLOW;
		if (current_state == SoulState.ST_FOLLOW) transform.position = role_pos + STANDARD_OFFSET;

		HandleShoot(role_pos);
	}
}
