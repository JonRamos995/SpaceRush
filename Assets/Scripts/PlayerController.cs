using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private float timeDelay;
    private bool center = true;
    private bool left = false;
    private bool right = false;

    private Light lighting;
    private Animator anim;
    private PowerUpController powerUpController;

    void Start()
    {
        timeDelay = 999;
        anim = GetComponent<Animator>();
        lighting = GetComponent<Light>();
        powerUpController = GameObject.FindGameObjectWithTag("PowerUpController").GetComponent<PowerUpController>();

        if (powerUpController == null)
            Debug.Log("Cannot find 'PowerUpController' script");
    }

    void Update()
    {
        timeDelay += Time.deltaTime;
        MovePlayer();

        if (powerUpController.IsShieldActive())
            lighting.range = 3;
        else
            lighting.range = 0;
    }

    void MovePlayer()
    {
#if !UNITY_EDITOR
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            timeDelay = 0;

            if (touch.phase == TouchPhase.Began)
            {
                if (touch.position.normalized.x < 0.5f && timeDelay > .13f)
                {
                    timeDelay = 0;

                    if (center)
                    {
                        center = false;
                        right = true;
                        anim.Play("RightSpinPlayerShip");
                    }
                    else if (left)
                    {
                        left = false;
                        center = true;
                        anim.Play("LeftToScenterPlayerShip");
                    }
                }
                else if (touch.position.normalized.x > 0.5f && timeDelay > .13f)
                {
                    timeDelay = 0;

                    if (center)
                    {
                        center = false;
                        left = true;
                        anim.Play("LeftSpinPlayerShip");
                    }
                    else if (right)
                    {
                        right = false;
                        center = true;
                        anim.Play("RightToScenterPlayerShip");
                    }
                }
            }
        }
#endif
#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
            {
                if ((Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) && timeDelay > .13f)
                {
                    timeDelay = 0;

                    if (center)
                    {
                        center = false;
                        right = true;
                        anim.Play("RightSpinPlayerShip");
                    }
                    else if (left)
                    {
                        left = false;
                        center = true;
                        anim.Play("LeftToScenterPlayerShip");

                    }
                }
                else if ((Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) && timeDelay > .13f)
                {
                    timeDelay = 0;

                    if (center)
                    {
                        center = false;
                        left = true;
                        anim.Play("LeftSpinPlayerShip");

                    }
                    else if (right)
                    {
                        right = false;
                        center = true;
                        anim.Play("RightToScenterPlayerShip");

                    }
                }
            }
        }
#endif
    }
}