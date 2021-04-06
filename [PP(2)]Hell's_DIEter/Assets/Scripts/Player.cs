using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    #region Variables
    private Animator anim;
    private Camera cam;
    private Rigidbody rb;
    private ParticleSystem particle;
    private Color originColor;
    public SkinnedMeshRenderer body;

    private bool isJetpackOn = false;       // ��Ʈ�� ��� Ȯ��
    public bool IsJetpackOn
    {
        get { return isJetpackOn; }
    }

    private bool isImmortal = false;       // ��Ʈ�� ��� Ȯ��
    public bool IsImmortal
    {
        get { return isImmortal; }
    }

    private float fuel = 100.0f;    // ���� ����
    public float Fuel
    {
        get { return fuel; }
    }

    private float maxFuel = 1.0f;   // ���ᷮ
    public float MaxFuel
    {
        get { return (int)maxFuel; }
        set { maxFuel += value; }
    }

    private float weight = 100;  // ������
    public float Weight
    {
        get { return weight; }
        set { weight += value; }
    }

    private int maxWeight = 100;  // ���� ���� ������
    public int MaxWeight
    {
        get { return maxWeight; }
    }

    private int minWeight = 100;  // ���� ���� ������
    public int MinWeight
    {
        get { return minWeight; }
        set { minWeight -= value; }
    }

    private float hp = 100.0f;                // ü��
    public float Hp
    {
        get { return (int)hp; }
        set
        {
            if (isImmortal) { return; }
            else {
                hp -= value;
                StartCoroutine(SetImmortalTimer());
            }
        }
    }

    private bool isGrounded;                 // ���鿡 �ִ���
    private int dumCounts = 0;              // ���� ����
    private float moveSpeed = 5.0f;     // ĳ���� �̵� �ӵ�
    private float rotSpeed = 10.0f;      // ĳ���� ȸ�� �ӵ�
    private float jumpForce = 10.0f;    // ���� ��
    #endregion

    void Start ()
    {
        cam = Camera.main;
        anim = gameObject.GetComponentInChildren<Animator>();
        rb = gameObject.GetComponentInParent<Rigidbody>();
        fuel = maxFuel;
        particle = gameObject.GetComponentInChildren<ParticleSystem>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        EventManager.Instance.AddListener(TUTORIAL_TYPE.MOVEMENTS, OnEvent);

        originColor = body.materials[1].color;
    }

    void Update ()
    {
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
                        this.gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                        break;
                    case 40:
                        this.gameObject.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                        break;
                    case 70:
                        this.gameObject.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
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
    }

    private void FixedUpdate()
    {
        // �÷��̾� ü�� Ȯ�� �� ȸ��
        if (hp > 0 && hp < 100)
        {
            hp += 1 * Time.deltaTime;
        }

        MoveCharacter();    // �÷��̾� �̵�

        UsingJetpack();     // ��Ʈ�� ��� ó��
    }

    #region Methods
    public void OnEvent(TUTORIAL_TYPE eventType, bool flag)
    {
        switch (eventType)
        {
            case TUTORIAL_TYPE.MOVEMENTS:
                break;
            case TUTORIAL_TYPE.DECREASE:
                break;
            case TUTORIAL_TYPE.INCREASE:
                break;
            default:
                break;
        }
    }
    
    private void MoveCharacter()
    {
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        bool isMove = moveInput.magnitude != 0; // �̵� �Է� �Ǻ�(True�� �̵�, False�� ����)

        // ���� �Ǻ�
        if (isGrounded)  // �������� �ƴ�
        {
            // ���ϸ��̼� ó��
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))          // ������, ����, ���������� �ȱ�
                anim.SetInteger("WalkParam", 1);

            else if (Input.GetKey(KeyCode.S))           // �ڷΰ���
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

        UsingJetpack();

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

    // ��Ʈ�� ���� �� �ִϸ��̼� ó��
    private void UsingJetpack()
    {
        switch (isJetpackOn)
        {
            case true:
                anim.SetBool("UsingJetpack", true);                         // ��Ʈ�� ��� �ִϸ��̼� ����
                particle.Play();                                            // �Ҳ� ��ƼŬ ����
                fuel -= Time.deltaTime * 1.5f;                                     // ���� ���
                rb.AddForce(Vector3.up * jumpForce*1.5f, ForceMode.Force);   // ��Ʈ�� �۵�(���) �κ�
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
            minWeight = maxWeight - 10 * dumCounts;  // ü�� ���� �缳��
            Debug.Log("Minimum weights: " + minWeight + "Kg");

            Destroy(collision.gameObject);
        }
        // ���� ȹ�� �� ���ᷮ ���� �缳��
        else if (collision.gameObject.CompareTag("Fuel"))
        {
            maxFuel += 5.0f;
        }
        // ���� ȹ�� �� ���� ���� ü�� �缳��
        else if (collision.gameObject.CompareTag("Food"))
        {
            weight = maxWeight;
            this.gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            Destroy(collision.gameObject);
        }
    }

    // ���� Ÿ�̸� �۵� �ڷ�ƾ 
    IEnumerator SetImmortalTimer()
    {
        if(!isImmortal)
            yield return null;

        float timer = 0;
        isImmortal = true;

        while (timer<3)
        {
            float flick = Mathf.Abs(Mathf.Sin(Time.time * 100));    // �� ����, 0�� 1�� �ݺ������� ��ȯ
            body.materials[1].color = originColor * flick;
            yield return new WaitForSeconds(0.05f);
            timer += 0.05f;
        }

        body.materials[1].color = originColor;
        isImmortal = false;
        yield return null;
    }
}
#endregion