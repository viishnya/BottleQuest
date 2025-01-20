using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    public GameObject PausePanel;
    public Joystick joystick;
    public float speed;
    public float health;
    static public int Score;
    public TextMeshProUGUI textScore;
    public GameObject[] hearts;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 moveVelocity;
    private Animator anim;
    private bool facingRight = false;
    public float thealth = 0;
    private Enemy enemy;
    private Transform playerTransform = null;
    public Vector3 start = Vector3.zero;

    void Start()
    {
        start = transform.position;
        thealth = health;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        for(int i = 0; i < hearts.Length; i++)
        {
            if(i >= health / 2)
            {
                hearts[i].gameObject.SetActive(false);
            }
            else
            {
                hearts[i].gameObject.SetActive(true);
            }
        }
        moveInput = new Vector2(joystick.Horizontal, joystick.Vertical);
        moveVelocity = moveInput.normalized * speed;
        if(moveInput.x == 0 && moveInput.y == 0)
        {
            anim.SetBool("isRunning", false);
        }
        else
        {
            anim.SetBool("isRunning", true);
        }

        if(!facingRight && moveInput.x > 0)
        {
            Flip();
        }
        else if(facingRight && moveInput.x < 0)
        {
            Flip();
        }
        if(health <= 0)
        {
            health = thealth;
            PausePanel.SetActive(true);
            textScore.text = Score.ToString();
            Time.timeScale = 0;
            Score = 0;
            transform.position = start;
            SpawnEnemy.nowEnemies = 0;
            SpawnEnemy.numberEnemies = 4;
            foreach(var obj in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                Destroy(obj);
            }
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveVelocity * Time.fixedDeltaTime);
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector2 Scaler = transform.localScale;
        Scaler.x *= -1;
        transform.localScale = Scaler;
    }
}
