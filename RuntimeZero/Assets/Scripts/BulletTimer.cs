using UnityEngine;
using System.Collections;

public class BulletTimer : MonoBehaviour
{
    public float Duration = 3.0f;
    public int BulletSpeed = 100;

	void Start ()
    {
       GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * BulletSpeed);
        Destroy(gameObject, Duration);
    }

}
