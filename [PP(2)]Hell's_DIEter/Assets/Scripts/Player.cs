using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	private Animator anim;
    private Camera cam;
    private Rigidbody rb;
    private ParticleSystem particle;

    [SerializeField] private bool isJetpackOn;       // ��Ʈ�� ��� Ȯ��
    [SerializeField] private float fuel = 10.0f;      // ���� ����
    [SerializeField] private bool isGrounded;
    private float gravity = 14.0f;
    private float speed = 5.0f;
    private float jumpForce = 10.0f;     // ���� ��
    private float maximumFuel;

    private void Awake()
    {
        cam = Camera.main;
    }

    void Start () {
		anim = gameObject.GetComponentInChildren<Animator>();
        rb = gameObject.GetComponentInParent<Rigidbody>();
        maximumFuel = fuel;
        particle = gameObject.GetComponentInChildren<ParticleSystem>();
        if (particle == null)
        {
            Debug.Log("CANNOT FIND JETPACK PARTICLES.");
        }
    }

    void Update ()
    {
        MoveCharacter();
    }

    private void UpgradeFuelLimits()
    {
        maximumFuel += 5.0f;
        return;
    }

    private void MoveCharacter()
    {
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        bool isMove = moveInput.magnitude != 0;

        // ���� ó��
        if (isGrounded)  // �������� �ƴ�
        {
            // ���ϸ��̼� ó��
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) // Walk foward, left, right.
                anim.SetInteger("WalkParam", 1);
            else if (Input.GetKey(KeyCode.S)) // Walk backward
                anim.SetInteger("WalkParam", 2);
            else // Idle
                anim.SetInteger("WalkParam", 0);

            anim.SetInteger("JumpParam", 0);
            anim.SetBool("UsingJetpack", false);

            if (Input.GetKeyDown(KeyCode.Space)) // ����
            {
                isGrounded = false;
                anim.SetInteger("JumpParam", 1);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
        else // ���� �Ǵ� ���� ��
        {
            anim.SetInteger("WalkParam", 0);
            // ���ᰡ ���������� ��Ʈ�� ���
            if (Input.GetKey(KeyCode.Space) && fuel > 0.0f)
            {
                isJetpackOn = true;
            }
            else
            {
                isJetpackOn = false;
            }
        }
        // ��Ʈ�� ��� ó��
        UsingJetpack();

        // �̵� ó��
        if (isMove)
        {
            Vector3 lookForward = new Vector3(cam.transform.forward.x, 0f, cam.transform.forward.z).normalized;
            Vector3 lookRight = new Vector3(cam.transform.right.x, 0f, cam.transform.right.z).normalized;
            Vector3 moveDir = lookForward * moveInput.y + lookRight * moveInput.x;

            this.transform.forward = lookForward;
            this.transform.position += moveDir * Time.deltaTime * 5f;
        }
    }

    private void UsingJetpack()
    {
        switch (isJetpackOn)
        {
            case true:
                anim.SetBool("UsingJetpack", true);
                particle.Play();
                fuel -= Time.deltaTime;
                rb.AddForce(Vector3.up * jumpForce * 2, ForceMode.Force);
                break;
            case false:
                anim.SetBool("UsingJetpack", false);
                particle.Stop();
                // ���� ȸ��
                if (fuel < maximumFuel)
                {
                    fuel += Time.deltaTime * 0.1f;
                    if (fuel > maximumFuel)
                        fuel = maximumFuel;
                }
                
                break;
        }
        
        return;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            anim.SetInteger("JumpParam", 0);    // ���� ���ϸ��̼� OFF
            anim.SetBool("UsingJetpack", false);    // ��Ʈ�� ���ϸ��̼� OFF
            isJetpackOn = false;                // ��Ʈ�� ��� �� �ƴ�
            isGrounded = true;
        }

        return;
    }
}
