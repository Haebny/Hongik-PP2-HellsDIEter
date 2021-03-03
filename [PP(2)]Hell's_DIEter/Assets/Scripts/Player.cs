using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	private Animator anim;
    private CharacterController controller; 

    public float speed = 600.0f;
	public float turnSpeed = 400.0f;
	private Vector3 moveDirection = Vector3.zero;

    public bool isJetpackOn;       // ��Ʈ�� ��� Ȯ��

    private float verticalVelocity;
    public float gravity = 14.0f;
    public float jumpForce = 3.0f;     // ���� ��
    private float maximumFuel;
    public float fuel = 3.0f;      // ���� ����


    void Start () {
		anim = gameObject.GetComponentInChildren<Animator>();
        controller = GetComponent<CharacterController>();
        maximumFuel = fuel;
	}

    void Update ()
    {
        // �÷��̾� �ִϸ��̼� ����
        // Set the animation to walk, jump or idle.
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) // Go Foward
        {
            anim.SetInteger("WalkParam", 1);
        }
        else if (Input.GetKey(KeyCode.S)) // Go Backward
        {
            anim.SetInteger("WalkParam", 2);
        }
        else // Idle
        {
            anim.SetInteger("WalkParam", 0);
            anim.SetInteger("JumpParam", 0);
        }

        if (controller.isGrounded)  // ����
        {
            verticalVelocity = -gravity * Time.deltaTime;
            if(Input.GetKeyDown(KeyCode.Space)) // ����
            {
                verticalVelocity = jumpForce;
                anim.SetInteger("JumpParam", 1);
            }
            anim.SetBool("UsingJetpack", false);
        }
        else // ����
        {
            if(Input.GetKey(KeyCode.Space) && fuel > 0.0f)
            {
                isJetpackOn = true;
                UsingJetpack();
                anim.SetBool("UsingJetpack", true);
                fuel -= Time.deltaTime;
            }
            else
            {
                isJetpackOn = false;
                verticalVelocity -= gravity * Time.deltaTime;

                // ���� ȸ��
                if(fuel < maximumFuel)
                {
                    fuel += Time.deltaTime * 0.5f;
                }
            }
        }

        Vector3 moveVector = Vector3.zero;
        moveVector.x = Input.GetAxis("Horizontal") * 5.0f;
        moveVector.y = verticalVelocity;
        moveVector.z = Input.GetAxis("Vertical") * 5.0f;
        controller.Move(moveVector * Time.deltaTime);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            anim.SetInteger("JumpParam", 0);
            isJetpackOn = false;
            verticalVelocity = -gravity * Time.deltaTime;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        return;
    }

    private void UsingJetpack()
    {
        Vector3 jetpackOn = new Vector3(0.0f, verticalVelocity * 0.5f, 0.0f);
        controller.Move(jetpackOn * Time.deltaTime);

        return;
    }

    private void UpgradeFuelLimits()
    {
        maximumFuel += 5.0f;
        return;
    }
}
