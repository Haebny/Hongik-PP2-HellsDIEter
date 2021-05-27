using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour {

    #region Variables
    private Animator anim;
    private Camera cam;
    private Rigidbody rb;
    private ParticleSystem particle;
    private Color originColor;
    public SkinnedMeshRenderer body;
    private GameObject grab;

    public static Vector3 loadPos;

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

    private float fuel = 1.0f;    // ���� ����
    public float Fuel
    {
        get { return fuel; }
    }

    private float maxFuel = 1.0f;   // ���ᷮ
    public float MaxFuel
    {
        get { return (int)maxFuel; }
        set { maxFuel = value; }
    }

    private float weight = 100;  // ������
    public float Weight
    {
        get { return weight; }
        set { weight = value; }
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
        set { minWeight = value; }
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

    private bool usable;                          // ���콺�� �����Ӱ� ��� ��������
    public bool Usable
    {
        get { return usable; }
        set { usable = value; }
    }

    private int dumCounts = 0;              // ���� ����
    public int DumCounts
    {
        get { return dumCounts; }
        set { dumCounts = value; }
    }

    private int coinCounts = 0;              // ���� ����
    public int CoinCounts
    {
        get { return coinCounts; }
        set { coinCounts = value; }
    }

    private bool hasKey = false;
    public bool HasKey
    {
        get { return hasKey; }
        set { hasKey = value; }
    }
    private bool hasMap;
    public bool HasMap
    {
        get { return hasMap; }
        set { hasMap = value; }
    }

    private bool isGrabbing = false;        // ���͸� ����ִ���
    public bool IsGrabbing
    {
        get { return isGrabbing; }
    }

    private bool resized = false;

    [SerializeField]private bool isGrounded;                 // ���鿡 �ִ���
    private float moveSpeed = 5.0f;     // ĳ���� �̵� �ӵ�
    private float rotSpeed = 10.0f;      // ĳ���� ȸ�� �ӵ�
    private float jumpForce = 10.0f;    // ���� ��

    private bool isWalking;
    private bool isWind = false;
    private AudioSource audio;
    public AudioClip[] AudioClips;
    #endregion

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    void Start ()
    {
        if (PlayerPrefs.HasKey("Min") == false)
            PlayerPrefs.SetInt("Min", minWeight);

        cam = Camera.main;
        anim = gameObject.GetComponentInChildren<Animator>();
        rb = gameObject.GetComponentInParent<Rigidbody>();
        fuel = maxFuel;
        particle = gameObject.GetComponentInChildren<ParticleSystem>();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        usable = true;
        isWalking = false;
        audio = GetComponent<AudioSource>();

        originColor = body.materials[1].color;
    }

    void Update ()
    {
        if (resized == false)
        {
            ResizingWeight(weight);
            UIManger ui = GameObject.Find("UI").GetComponent<UIManger>();
            ui.ShowMapIcon(this.hasMap);
            resized = true;
        }

        // ���콺 On/Off
        if(Input.GetKeyDown(KeyCode.LeftControl) && usable)
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
            LoosingWeight();
        }
        else if (Input.GetMouseButtonUp(1))
        {
            anim.SetBool("DoingExercise", false);
        }

        MoveCharacter();    // �÷��̾� �̵�

        if (isWalking && isGrounded)
        {
            audio.clip = AudioClips[0];
            audio.volume = 0.5f;
            if (audio.isPlaying == false)
            {
                audio.Play();
            }
        }
        else
        {
            audio.Stop();
        }

        // ���
        if (Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {

                RaycastHit hit;
                Debug.DrawRay(transform.position, transform.forward * 10.0f, Color.blue, 0.3f);
                if (Physics.Raycast(transform.position, transform.forward, out hit, 10.0f)
                    && hit.transform.CompareTag("Slime"))
                {
                    isGrabbing = !isGrabbing;

                    if (isGrabbing)
                    {
                        grab = hit.transform.gameObject;
                        if (grab.transform.Find("Body").CompareTag("Slime"))
                            grab.GetComponent<Slime>().Estate = Slime.STATE.CATCHED;

                        grab.transform.SetParent(this.gameObject.transform);
                    }
                }
                else if (Physics.Raycast(transform.position, transform.forward, out hit, 10.0f)
                    && hit.transform.CompareTag("Scale")
                    && grab != null)
                {
                    hit.transform.GetComponent<Scale>().CheckMonster();
                    isGrabbing = false;
                    grab = null;
                }
            }
        }

        if (grab != null)
        {
            GrabObject();
        }
    }

    // ü�� ���Ҹ޼ҵ�
    private void LoosingWeight()
    {
        if (dumCounts != 0)
        {
            audio.clip = AudioClips[2];
            audio.volume = 1.0f;
            audio.Play();
            weight += -dumCounts;
            anim.SetBool("DoingExercise", true);
            if (weight < minWeight)
                weight = minWeight;

            // ü�߿� ���� �÷��̾� ������� �ɷ�ġ ����
            ResizingWeight(weight);
        }
    }

    private void ResizingWeight(float weight)
    {
        switch (weight)
        {
            case 0:
                this.gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                moveSpeed = 11.0f;
                jumpForce = 17.5f;
                break;
            case 40:
                this.gameObject.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                moveSpeed = 11.0f;
                jumpForce = 16.5f;
                break;
            case 70:
                this.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
                moveSpeed = 9.5f;
                jumpForce = 15.0f;
                break;
            case 90:
                this.gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                moveSpeed = 8.5f;
                jumpForce = 12.5f;
                break;
            case 100:
                this.gameObject.transform.localScale = new Vector3(2f, 2f, 2f);
                moveSpeed = 7.5f;
                jumpForce = 10.0f;
                break;
        }
    }

    private void FixedUpdate()
    {
        // �÷��̾� ü�� Ȯ�� �� ȸ��
        if (hp > 0 && hp < 100)
        {
            hp += 1 * Time.fixedDeltaTime;
        }

        if(fuel > 0.5f && isJetpackOn)
            rb.AddForce(Vector3.up * jumpForce * 1.5f, ForceMode.Force);   // ��Ʈ�� �۵�(���) �κ�

        if(isWind)
            rb.AddForce(Vector3.up * jumpForce * 3.0f, ForceMode.Force);
    }

    #region Methods
    


    private void MoveCharacter()
    {
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        bool isMove = moveInput.magnitude != 0; // �̵� �Է� �Ǻ�(True�� �̵�, False�� ����)

        // ���� �Ǻ�
        if (isGrounded)  // �������� �ƴ�
        {
            // ���ϸ��̼� ó��
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))          // ������, ����, ���������� �ȱ�
            {
                anim.SetInteger("WalkParam", 1);
                isWalking = true;
            }

            else if (Input.GetKey(KeyCode.S))           // �ڷΰ���
            {
                anim.SetInteger("WalkParam", 2);
                isWalking = true;
            }

            else // Idle
            { 
                anim.SetInteger("WalkParam", 0);
                isWalking = false;
            }

            anim.SetInteger("JumpParam", 0);
            anim.SetBool("UsingJetpack", false);

            if (Input.GetKeyDown(KeyCode.Space)) // ����
            {
                if (isGrounded)
                {
                    audio.clip = AudioClips[1];
                    audio.volume = 0.7f;
                    audio.Play();
                }
                isWalking = false;
                isGrounded = false;
                anim.SetInteger("JumpParam", 1);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
        else // ���� �Ǵ� ���� ��
        {
            isWalking = false;
            anim.SetInteger("WalkParam", 0);
            // ���� �ִϸ��̼� �� ���ᰡ ���������� ��Ʈ�� ���
            if (Input.GetKey(KeyCode.Space) && fuel > 0.5f &&
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
                if (fuel < 0f)
                {
                    fuel = 0;
                    isJetpackOn = false;
                    break;
                }
                
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
        }
        // B3 - 2 ����
        if (other.CompareTag("PanelRoom") && PanelPuzzleController.level == 1)
        {
            SceneManager.LoadSceneAsync("3.PanelPuzzle");
            PlayerPrefs.SetInt("Dumb", dumCounts);
            PlayerPrefs.SetInt("Fuel", (int)maxFuel);
            PlayerPrefs.SetInt("Coin", coinCounts);
            PlayerPrefs.SetInt("Weight", (int)weight);
            PlayerPrefs.SetInt("Min", minWeight);
            if (HasMap == true)
                PlayerPrefs.SetInt("Map", 1);
            else
                PlayerPrefs.SetInt("Map", 0);
            if (HasKey == true)
                PlayerPrefs.SetInt("Key", 1);
            else
                PlayerPrefs.SetInt("Key", 0);
        }
        // B2 - 4 ����
        else if (other.CompareTag("ScaleRoom") && WeightPuzzleController.wLevel == 1)
        { 
            SceneManager.LoadSceneAsync("4.WeightScale");
            PlayerPrefs.SetInt("Dumb", dumCounts);
            PlayerPrefs.SetInt("Fuel", (int)maxFuel);
            PlayerPrefs.SetInt("Coin", (int)coinCounts);
            PlayerPrefs.SetInt("Weight", (int)weight);
            PlayerPrefs.SetInt("Min", minWeight);
            if(HasMap == true)
                PlayerPrefs.SetInt("Map", 1);
            else
                PlayerPrefs.SetInt("Map", 0);
            if (HasKey == true)
                PlayerPrefs.SetInt("Key", 1);
            else
                PlayerPrefs.SetInt("Key", 0);

        }
        // B2 - 7 ����
        else if (other.CompareTag("PanelRoom") && PanelPuzzleController.level == 2)
        {
            SceneManager.LoadSceneAsync("3.PanelPuzzle");
            PlayerPrefs.SetInt("Dumb", dumCounts);
            PlayerPrefs.SetInt("Fuel", (int)maxFuel);
            PlayerPrefs.SetInt("Coin", (int)coinCounts);
            PlayerPrefs.SetInt("Weight", (int)weight);
            PlayerPrefs.SetInt("Min", minWeight);
            if (HasMap == true)
                PlayerPrefs.SetInt("Map", 1);
            else
                PlayerPrefs.SetInt("Map", 0);
            if (HasKey == true)
                PlayerPrefs.SetInt("Key", 1);
            else
                PlayerPrefs.SetInt("Key", 0);
        }
        // B3 - 8 ����
        else if (other.CompareTag("ScaleRoom") && WeightPuzzleController.wLevel == 2)
        {
            SceneManager.LoadSceneAsync("4.WeightScale");
            PlayerPrefs.SetInt("Dumb", dumCounts);
            PlayerPrefs.SetInt("Fuel", (int)maxFuel);
            PlayerPrefs.SetInt("Coin", (int)coinCounts);
            PlayerPrefs.SetInt("Weight", (int)weight);
            PlayerPrefs.SetInt("Min", minWeight);
            if (HasMap == true)
                PlayerPrefs.SetInt("Map", 1);
            else
                PlayerPrefs.SetInt("Map", 0);
            if (HasKey == true)
                PlayerPrefs.SetInt("Key", 1);
            else
                PlayerPrefs.SetInt("Key", 0);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // �ٶ��� Ÿ�� �ö�(���ſ��....)
        if (other.CompareTag("Wind"))
        {
            isGrounded = false;
            isWind = true;
        }
        // ������ �濡 ������ ���� ��
        else if (other.CompareTag("FoodRoom"))
        {
            weight += Time.deltaTime * 1;
            if (weight > maxWeight)
                weight = maxWeight;

            ResizingWeight(weight);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Wind"))
        {
            isWind = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }

        // ���� ȹ�� �� ���� ���� ü�� �缳��
        if(collision.gameObject.CompareTag("Dumbbell"))
        {
            audio.clip = AudioClips[3];
            audio.volume = 1.0f;
            audio.Play();
            dumCounts += 1; // ȹ���� ���� ���� �߰�
            minWeight -= 10 * dumCounts;  // ü�� ���� �缳��
            PlayerPrefs.SetInt("Dumb", dumCounts);
            Destroy(collision.gameObject);
        }
        // ���� ȹ�� �� ���ᷮ ���� �缳��
        else if (collision.gameObject.CompareTag("Fuel"))
        {
            audio.clip = AudioClips[3];
            audio.volume = 1.0f;
            audio.Play();
            Destroy(collision.gameObject);
            maxFuel += 5.0f;
            if(maxFuel > 11)
            {
                maxFuel = 11;
            }
            PlayerPrefs.SetInt("Fuel", (int)maxFuel);
        }
        // ���� ���� �� ü�� ����
        else if (collision.gameObject.CompareTag("Food"))
        {
            audio.clip = AudioClips[3];
            audio.volume = 1.0f;
            audio.Play();
            weight = maxWeight;
            this.gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            Destroy(collision.gameObject);
        }
        // ������ ���� �� ������
        else if (collision.gameObject.CompareTag("Slime"))
        {
            hp -= 10;
            StartCoroutine("SetImmortalTimer");
        }
        else if(collision.gameObject.CompareTag("Key") && this.hasKey == false)
        {
            audio.clip = AudioClips[3];
            audio.volume = 1.0f;
            audio.Play();
            this.hasKey = true;
            Destroy(collision.gameObject);
            UIManger ui = GameObject.Find("UI").GetComponent<UIManger>();
            ui.ShowKeyIcon(this.hasKey);
            PlayerPrefs.SetInt("Key", 1);
        }
        else if (collision.gameObject.CompareTag("Map") && this.hasMap==false)
        {
            audio.clip = AudioClips[3];
            audio.volume = 1.0f;
            audio.Play();
            this.hasMap = true;
            Destroy(collision.gameObject);
            UIManger ui = GameObject.Find("UI").GetComponent<UIManger>();
            ui.ShowMapIcon(this.hasMap);
            PlayerPrefs.SetInt("Map", 1);
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

    private void GrabObject()
    {
        if (isGrabbing)
        {
            Vector3 pos = transform.position + transform.forward * 5.0f + transform.up * 5.0f;
            grab.transform.position = pos;
        }
    }
}
#endregion