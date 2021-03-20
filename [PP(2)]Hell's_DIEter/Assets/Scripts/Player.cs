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
    [SerializeField] private int weight = 100;  // ������
    
    [SerializeField] private float hp = 100.0f;      // ü��
    [SerializeField] private bool isGrounded;
    private float moveSpeed = 5.0f;
    private float rotSpeed = 10.0f;
    private float jumpForce = 10.0f;     // ���� ��
    private float maxFuel = 10;
    private float maxWeight = 100.0f;  // ������

    private void Awake()
    {
        cam = Camera.main;
    }

    void Start () {
		anim = gameObject.GetComponentInChildren<Animator>();
        rb = gameObject.GetComponentInParent<Rigidbody>();
        fuel = maxFuel;
        particle = gameObject.GetComponentInChildren<ParticleSystem>();
    }

    void Update ()
    {
        MoveCharacter();
        if (hp > 0 && hp < 100) 
        {
            hp += 1 * Time.deltaTime;
        }

        if (hp < 0) 
        {
            hp = 0;
        }
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

    public float GetFuel()
    {
        return fuel;
    }

    public float GetMaxFuel()
    {
        return maxFuel;
    }

    public int GetWeight()
    {
        return weight;
    }

    public float GetMaxWeight()
    {
        return maxWeight;
    }

    public float GetHP()
    {
        return hp;
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
}
