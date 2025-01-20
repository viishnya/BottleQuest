using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public GunType gunType;
    public float offset;
    public GameObject bullet;
    public Transform shotPoint;
    public float startTimeBtwShots;
    public Joystick joystick;
    public enum GunType {Default, Enemy};

    private float timeBtwShots;
    private Player player;
    private float rotZ;
    private Vector3 difference;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    void Update()
    {
        if(gunType == GunType.Default && (Mathf.Abs(joystick.Vertical) > 0 || Mathf.Abs(joystick.Horizontal) > 0))
        {
            rotZ = Mathf.Atan2(joystick.Vertical, joystick.Horizontal) * Mathf.Rad2Deg;
        }
        else if (gunType == GunType.Enemy)
        {
            difference = player.transform.position - transform.position;
            rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        }
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ + offset);

        if (timeBtwShots <= 0)
        {
            if(gunType == GunType.Enemy)
            {
                Shoot();
            }
            else if (joystick.Vertical != 0 || joystick.Horizontal != 0)
            {
                Shoot();
            }
        }
        else
        {
            timeBtwShots -= Time.deltaTime;
        }
    }

    public void Shoot()
    {
        Instantiate(bullet, shotPoint.position, shotPoint.rotation);
        timeBtwShots = startTimeBtwShots;
    }
}
