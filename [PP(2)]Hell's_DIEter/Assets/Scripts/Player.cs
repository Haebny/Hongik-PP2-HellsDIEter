using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	private Animator anim;
    private Camera cam;
    private Rigidbody rb;
    private ParticleSystem particle;

    [SerializeField] private bool isJetpackOn = false;       // ��Ʈ�� ��� Ȯ��
    [SerializeField] private bool isImmortal = false;       // ��Ʈ�� ��� Ȯ��
    [SerializeField] private float fuel = 100.0f;    // ���� ����
    [SerializeField] private float weight = 100;  // ������
    private int limit = 10;
    private int dumCounts = 0;
    
    [SerializeField] private float hp = 100.0f;      // ü��
    [SerializeField] private bool isGrounded;
    private bool isVisible;
    private float moveSpeed = 5.0f;
    private float rotSpeed = 10.0f;
    private float jumpForce = 10.0f;     // ���� ��
    private float maxFuel = 1.0f;
    private int maxWeight = 100;  // ������
    private int minWeight = 100;  // ������

    private void Awake()
    {
        cam = Camera.main;
    }

    void Start () {
		anim = gameObject.GetComponentInChildren<Animator>();
        rb = gameObject.GetComponentInParent<Rigidbody>();
        fuel = maxFuel;
        particle = gameObject.GetComponentInChildren<ParticleSystem>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update ()
    {
        // �÷��̾� ü�� Ȯ�� �� ȸ��
        if (hp <= 0)
        {
            hp = 0;
            return;
        }
        if (hp > 0 && hp < 100)
        {
            hp += 1 * Time.deltaTime;
        }

        // ���콺 On/Off
        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (Cursor.visible == false)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        // ü�� ������ �ִϸ��̼�
        if (Input.GetMouseButtonDown(1))
        {
            if (dumCounts != 0)
            {
                weight += -dumCounts;
                anim.SetBool("DoingExercise", true);
                if (weight < minWeight)
                    weight = minWeight;
                switch (weight)
                {
                    case 0:
                        break;
                    case 40:
                        break;
                    case 70:
                        break;
                    case 90:
                        this.gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                        break;
                    case 100:
                        this.gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                        break;
                }

                cam.GetComponent<ThirdPersonCamera>().SetDistance((int)weight);
            }
        }
        else if (Input.GetMouseButton(1) || Input.GetMouseButtonUp(1))
        {
            anim.SetBool("DoingExercise", false);
        }

        MoveCharacter();    // �÷��̾� �̵�
    }

    private void UpgradeFuelLimits()
    {
        maxFuel += 5.0f;
        return;
    }

    private void MoveCharacter()
    {
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        bool isMove = moveInput.magnitude != 0; // �̵� �Է� �Ǻ�(True�� �̵�, False�� ����)

        // ���� �Ǻ�
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
            // ���� �ִϸ��̼� �� ���ᰡ ���������� ��Ʈ�� ���
            if (Input.GetKey(KeyCode.Space) && fuel > 0.0f &&
                (anim.GetCurrentAnimatorStateInfo(0).IsName("Jump Up") ||
                anim.GetCurrentAnimatorStateInfo(0).IsName("Float")) &&
                anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                isJetpackOn = true;
            }
            else if (Input.GetKeyUp(KeyCode.Space) || fuel <= 0.0f)
            {
                isJetpackOn = false;
            }
        }
        
        UsingJetpack();     // ��Ʈ�� ��� ó��
        
        if (isMove)         // �̵� ó��
        {
            Vector3 lookForward = new Vector3(cam.transform.forward.x, 0f, cam.transform.forward.z).normalized; // ������ Ȯ��
            Vector3 lookRight = new Vector3(cam.transform.right.x, 0f, cam.transform.right.z).normalized;       // ����� Ȯ��
            Vector3 moveDir = lookForward * moveInput.y + lookRight * moveInput.x;                              // ĳ���Ͱ� ������ ���� ���

            Quaternion rotateFoward = Quaternion.LookRotation(lookForward);     // �÷��̾ ���� �ٶ󺸰�
            rb.rotation = Quaternion.Slerp(rb.rotation, rotateFoward, rotSpeed * Time.deltaTime);    // �ε巴�� ȸ��
            this.transform.position += moveDir * Time.deltaTime * moveSpeed;    // ������ ������ ���� ���ư�
        }
    }

    private void UsingJetpack()
    {
        switch (isJetpackOn)
        {
            case true:
                anim.SetBool("UsingJetpack", true);                         // ��Ʈ�� ��� �ִϸ��̼� ����
                particle.Play();                                            // �Ҳ� ��ƼŬ ����
                fuel -= Time.deltaTime;                                     // ���� ���
                rb.AddForce(Vector3.up * jumpForce * 2, ForceMode.Force);   // ��Ʈ�� �۵�(���) �κ�
                break;
                
            case false:
                anim.SetBool("UsingJetpack", false);                        // ��Ʈ�� ��� �ִϸ��̼� �ߴ�
                particle.Stop();                                            // �Ҳ� ��ƼŬ �ߴ�
                if (fuel < maxFuel)                                     // ���� ȸ��
                {
                    fuel += Time.deltaTime * 0.5f;
                    if (fuel > maxFuel)                                 // ���ᰡ �ִ뷮���� ũ�� �ִ뷮�� �°� ����
                    {
                        fuel = maxFuel;
                    }
                }
                break;
        }
        
        return;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other)
        {
            anim.SetInteger("JumpParam", 0);    // ���� ���ϸ��̼� OFF
            anim.SetBool("UsingJetpack", false);    // ��Ʈ�� ���ϸ��̼� OFF
            isJetpackOn = false;                // ��Ʈ�� ��� �� �ƴ�
            isGrounded = true;
        }

        return;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // ���� ȹ�� �� ���� ���� ü�� �缳��
        if(collision.gameObject.CompareTag("Dumbbell"))
        {
            dumCounts += 1; // ȹ���� ���� ���� �߰�
            minWeight = maxWeight - limit * dumCounts;  // ü�� ���� �缳��
            Debug.Log("Minimum weights: " + minWeight + "Kg");

            Destroy(collision.gameObject);
        }
        // ���� ȹ�� �� ���� ���� ü�� �缳��
        if (collision.gameObject.CompareTag("Food"))
        {
            weight = maxWeight;
            this.gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            Destroy(collision.gameObject);
        }
    }

    public float GetFuel()
    {
        return fuel;
    }

    public int GetMaxFuel()
    {
        return (int)maxFuel;
    }

    public float GetWeight()
    {
        return weight;
    }

    public int GetMaxWeight()
    {
        return maxWeight;
    }

    public int GetHP()
    {
        return (int)hp;
    }

    public void SetHP(int damage)
    {
        if (isImmortal)
            return;

        hp -= damage;
        return;
    }

    IEnumerator SetImmortalTimer()
    {
        int timer = 0;
        isImmortal = true;
        while (timer<5)
        {
            yield return new WaitForSeconds(1.0f);
            timer++;
        }

        isImmortal = false;
        yield return null;
    }

    public bool IsStateImmortal()
    {
        return isImmortal;
    }

    public bool IsJetpackOn()
    {
        return isJetpackOn;
    }
}
