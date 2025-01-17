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

    private bool isJetpackOn = false;       // 제트팩 사용 확인
    public bool IsJetpackOn
    {
        get { return isJetpackOn; }
    }

    private bool isImmortal = false;       // 제트팩 사용 확인
    public bool IsImmortal
    {
        get { return isImmortal; }
    }

    private float fuel = 1.0f;    // 비행 연료
    public float Fuel
    {
        get { return fuel; }
    }

    private float maxFuel = 1.0f;   // 연료량
    public float MaxFuel
    {
        get { return (int)maxFuel; }
        set { maxFuel = value; }
    }

    private float weight = 100;  // 몸무게
    public float Weight
    {
        get { return weight; }
        set { weight = value; }
    }

    private int maxWeight = 100;  // 증량 가능 몸무게
    public int MaxWeight
    {
        get { return maxWeight; }
    }

    private int minWeight = 100;  // 감량 가능 몸무게
    public int MinWeight
    {
        get { return minWeight; }
        set { minWeight = value; }
    }

    private float hp = 100.0f;                // 체력
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

    private int dumCounts = 0;              // 덤벨 개수
    public int DumCounts
    {
        get { return dumCounts; }
        set { dumCounts = value; }
    }

    private int coinCounts = 0;              // 동전 개수
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

    private bool isGrabbing;        // 몬스터를 잡고있는지
    public bool IsGrabbing
    {
        get { return isGrabbing; }
    }

    private bool resized;

    [SerializeField] private bool isGrounded;                 // 지면에 있는지
    private float moveSpeed;     // 캐릭터 이동 속도
    private float rotSpeed;      // 캐릭터 회전 속도
    private float jumpForce;    // 점프 힘

    private bool isWalking;
    private bool isWind;
    private AudioSource audio;
    public AudioClip[] AudioClips;
    #endregion

    private void Awake()
    {
        Application.targetFrameRate = 60;

        if (SceneManager.GetActiveScene().buildIndex == 5)
            Cursor.visible = false;
        else
            Cursor.visible = true;
    }

    void Start ()
    {
        if (PlayerPrefs.HasKey("Min") == false)
            PlayerPrefs.SetInt("Min", minWeight);

        // Initialize variables
        cam = Camera.main;
        anim = gameObject.GetComponentInChildren<Animator>();
        anim.SetInteger("WalkParam", 0);
        rb = gameObject.GetComponentInParent<Rigidbody>();
        fuel = maxFuel;
        particle = gameObject.GetComponentInChildren<ParticleSystem>();
        particle.Stop();
        audio = GetComponent<AudioSource>();

        // Initialize player variables
        moveSpeed = 5.0f;     // 캐릭터 이동 속도
        rotSpeed = 10.0f;      // 캐릭터 회전 속도
        jumpForce = 10.0f;    // 점프 힘

        // Initialize player states
        resized = false;
        isGrabbing = false;
        isWalking = false;
        isWind = false;

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

        // 체중 감량과 애니메이션
        if (Input.GetMouseButtonDown(1))
        {
            LoosingWeight();
        }
        else if (Input.GetMouseButtonUp(1))
        {
            anim.SetBool("DoingExercise", false);
        }

        if (Cursor.visible == true)
        {
            anim.SetInteger("WalkParam", 0);
            isWalking = false;
            return;
        }

        MoveCharacter();    // 플레이어 이동

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

        // 잡기
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

    // 체중 감소메소드
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

            // 체중에 따라 플레이어 사이즈와 능력치 변경
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
        // 플레이어 체력 확인 및 회복
        if (hp > 0 && hp < 100)
        {
            hp += 1 * Time.fixedDeltaTime;
        }

        if(fuel > 0.5f && isJetpackOn)
            rb.AddForce(Vector3.up * jumpForce * 1.5f, ForceMode.Force);   // 제트팩 작동(상승) 부분

        if(isWind)
            rb.AddForce(Vector3.up * jumpForce * 3.0f, ForceMode.Force);
    }

    #region Methods
    


    private void MoveCharacter()
    {
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        bool isMove = moveInput.magnitude != 0; // 이동 입력 판별(True면 이동, False면 정지)

        // 점프 판별
        if (isGrounded)  // 점프중이 아님
        {
            // 에니매이션 처리
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))          // 앞으로, 왼쪽, 오른쪽으로 걷기
            {
                anim.SetInteger("WalkParam", 1);
                isWalking = true;
            }

            else if (Input.GetKey(KeyCode.S))           // 뒤로가기
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

            if (Input.GetKeyDown(KeyCode.Space)) // 점프
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
        else // 점프 또는 낙하 중
        {
            isWalking = false;
            anim.SetInteger("WalkParam", 0);
            // 점프 애니메이션 후 연료가 남아있으면 제트팩 사용
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

        if (isMove)         // 이동 처리
        {
            Vector3 lookForward = new Vector3(cam.transform.forward.x, 0f, cam.transform.forward.z).normalized; // 전방향 확인
            Vector3 lookRight = new Vector3(cam.transform.right.x, 0f, cam.transform.right.z).normalized;       // 우방향 확인
            Vector3 moveDir = lookForward * moveInput.y + lookRight * moveInput.x;                              // 캐릭터가 움직일 방향 계산

            Quaternion rotateFoward = Quaternion.LookRotation(lookForward);     // 플레이어가 앞을 바라보고
            rb.rotation = Quaternion.Slerp(rb.rotation, rotateFoward, rotSpeed * Time.deltaTime);    // 부드럽게 회전
            this.transform.position += moveDir * Time.deltaTime * moveSpeed;    // 움직일 방향을 향해 나아감
        }
    }

    // 제트팩 연산 및 애니메이션 처리
    private void UsingJetpack()
    {
        switch (isJetpackOn)
        {
            case true:
                anim.SetBool("UsingJetpack", true);                         // 제트팩 사용 애니메이션 실행
                particle.Play();                                            // 불꽃 파티클 실행
                fuel -= Time.deltaTime * 1.5f;                                     // 연료 사용
                if (fuel < 0f)
                {
                    fuel = 0;
                    isJetpackOn = false;
                    break;
                }
                
                break;
                
            case false:
                anim.SetBool("UsingJetpack", false);                        // 제트팩 사용 애니메이션 중단
                particle.Stop();                                            // 불꽃 파티클 중단
                if (fuel < maxFuel)                                     // 연료 회복
                {
                    fuel += Time.deltaTime * 0.5f;
                    if (fuel > maxFuel)                                 // 연료가 최대량보다 크면 최대량에 맞게 조정
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
            anim.SetInteger("JumpParam", 0);    // 점프 에니메이션 OFF
            anim.SetBool("UsingJetpack", false);    // 제트팩 에니메이션 OFF
            isJetpackOn = false;                // 제트팩 사용 중 아님
        }
        // B3 - 2 입장
        if (other.CompareTag("PanelRoom") && PanelPuzzleController.level == 1)
        {
            PlayerPrefs.SetFloat("PosX", transform.position.x);
            PlayerPrefs.SetFloat("PosY", transform.position.y);
            PlayerPrefs.SetFloat("PosZ", transform.position.z);
            PlayerPrefs.SetInt("Recent", 1);

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

            SceneManager.LoadSceneAsync("3.PanelPuzzle");
        }
        // B2 - 4 입장
        else if (other.CompareTag("ScaleRoom") && WeightPuzzleController.wLevel == 1)
        {
            PlayerPrefs.SetFloat("PosX", transform.position.x);
            PlayerPrefs.SetFloat("PosY", transform.position.y);
            PlayerPrefs.SetFloat("PosZ", transform.position.z);
            PlayerPrefs.SetInt("Recent", 2);

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

            SceneManager.LoadSceneAsync("4.WeightScale");

        }
        // B2 - 7 입장
        else if (other.CompareTag("PanelRoom") && PanelPuzzleController.level == 2)
        {
            PlayerPrefs.SetFloat("PosX", transform.position.x);
            PlayerPrefs.SetFloat("PosY", transform.position.y);
            PlayerPrefs.SetFloat("PosZ", transform.position.z);
            PlayerPrefs.SetInt("Recent", 3);

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

            SceneManager.LoadSceneAsync("3.PanelPuzzle");
        }
        // B3 - 8 입장
        else if (other.CompareTag("ScaleRoom") && WeightPuzzleController.wLevel == 2)
        {
            PlayerPrefs.SetFloat("PosX", transform.position.x);
            PlayerPrefs.SetFloat("PosY", transform.position.y);
            PlayerPrefs.SetFloat("PosZ", transform.position.z);
            PlayerPrefs.SetInt("Recent", 4);

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

            SceneManager.LoadSceneAsync("4.WeightScale");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // 바람을 타고 올라감(무거우면....)
        if (other.CompareTag("Wind"))
        {
            isGrounded = false;
            isWind = true;
        }
        // 음식의 방에 있으면 살이 찜
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

        // 덤벨 획득 시 감량 하한 체중 재설정
        if(collision.gameObject.CompareTag("Dumbbell"))
        {
            audio.clip = AudioClips[3];
            audio.volume = 1.0f;
            audio.Play();
            dumCounts += 1; // 획득한 덤벨 개수 추가
            minWeight -= 10 * dumCounts;  // 체중 하한 재설정
            PlayerPrefs.SetInt("Dumb", dumCounts);
            Destroy(collision.gameObject);
        }
        // 연료 획득 시 연료량 상한 재설정
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
        // 음식 섭취 시 체중 증가
        else if (collision.gameObject.CompareTag("Food"))
        {
            audio.clip = AudioClips[3];
            audio.volume = 1.0f;
            audio.Play();
            weight = maxWeight;
            this.gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            Destroy(collision.gameObject);
        }
        // 슬라임 접촉 시 데미지
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

    // 무적 타이머 작동 코루틴 
    IEnumerator SetImmortalTimer()
    {
        if(!isImmortal)
            yield return null;

        float timer = 0;
        isImmortal = true;

        while (timer<3)
        {
            float flick = Mathf.Abs(Mathf.Sin(Time.time * 100));    // 색 점멸, 0과 1을 반복적으로 반환
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