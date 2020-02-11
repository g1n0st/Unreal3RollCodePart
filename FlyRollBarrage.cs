using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyRollBarrage : MonoBehaviour
{
	// Start is called before the first frame update
	public Vector3 direction;

	float dis_counter = 0.0f;
	float MAX_COUNTER = 3.0f;
	float ROLL_BARRAGE_SPEED = 3.0f;

	void Start()
    {
        
    }
	public void SetDirection(Vector3 d)
	{
		direction = d;
	}

    void Update()
    {
		transform.position += direction * Time.deltaTime * ROLL_BARRAGE_SPEED;
		dis_counter += Time.deltaTime;
		if (dis_counter > MAX_COUNTER) Destroy(gameObject);
    }
}
