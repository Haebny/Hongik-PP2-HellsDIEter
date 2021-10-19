using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    private GameObject player;

<<<<<<< Updated upstream
    private float rotSpeed = 5.0f;

    private float camDistance = 0;      // ���׺��� ī�޶������ �Ÿ�
    private float camWidth = -20.0f;    // ���� �Ÿ�
    private float camHeight = 5.0f;     // ���� �Ÿ�
    private float camFix = 3.0f;        // ����ĳ��Ʈ �� ���׸� ���� �� ���� ��(�Ÿ�)

    Vector3 direction;
    Vector3 mouseMove;
    Vector3 stopMouse;
=======
    float camDistance = 0;      // ���׺��� ī�޶������ �Ÿ�
    float camWidth = -20.0f;    // ���� �Ÿ�
    float camHeight = 5.0f;     // ���� �Ÿ�
    float camFix = 3.0f;        // ����ĳ��Ʈ �� ���׸� ���� �� ���� ��(�Ÿ�)
    float distance = 20f;
    float minZoom = 10.0f;       //�� ������ �� �ּ� �Ÿ�
    float maxZoom = 30.0f; // �ܾƿ����� �� �ִ� �Ÿ�
    float sensitivity = 100f; // ���콺 ����
    float zoomSpeed = 7f; // ���콺 ����
    Vector3 direction;

    // 2021 �ϰ� ����Ƽ ���� �м�ȸ���� �����ߴ� TPS Camera�� Update
    float x;
    float y;

    GameObject transparentObj;  // ������ȭ �� ������Ʈ
    Renderer ObstacleRenderer;  // ������Ʈ�� �������ϰ� ������ִ� ������
    List<GameObject> Obstacles; // ������ȭ �� ��ֹ� ����Ʈ
>>>>>>> Stashed changes

    private void Start()
    {
        // ���콺 ���� ����
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // �÷��̾� �±׸� ���� ���ӿ�����Ʈ(=�÷��̾�)�� ã�Ƽ� �ֱ�
        player = GameObject.FindObjectOfType<Player>().gameObject;
        Obstacles = new List<GameObject>(); // �� ����Ʈ ����

        // ���������κ��� ī�޶������ ����
        camDistance = Mathf.Sqrt(camWidth * camWidth + camHeight * camHeight);

        // ���������κ��� ī�޶� ��ġ������ ����
        direction = new Vector3(0, camHeight, camWidth).normalized;

        // ���콺 ���� ��ġ ����
        stopMouse = Input.mousePosition;
    }

    private void Update()
    {
<<<<<<< Updated upstream
        ////y�� ���� ȸ��
        //transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * Time.deltaTime * rotSpeed, Space.World);
        ////x�� ���� ȸ��
        //transform.Rotate(Vector3.left * Input.GetAxis("Mouse Y") * Time.deltaTime * rotSpeed, Space.Self);

        //transform.position = player.transform.position;
        ////����ĳ��Ʈ�� ���Ͱ�
        //Vector3 ray_target = transform.up * camHeight + transform.forward * camWidth;

        //RaycastHit hitinfo;
        //Physics.Raycast(transform.position, ray_target, out hitinfo, camDistance);

        //if (hitinfo.point != Vector3.zero)//�����ɽ�Ʈ ������
        //{
        //    //point�� �ű��.
        //    this.transform.position = hitinfo.point;
        //    //ī�޶� ����
        //    this.transform.Translate(direction * -1 * camFix);
        //}
        //else
        //{
        //    //������ǥ�� 0���� 
        //    this.transform.localPosition = Vector3.zero;
        //    //ī�޶���ġ������ ���⺤�� * ī�޶� �ִ�Ÿ� �� �ű��.
        //    this.transform.Translate(direction * camDistance);

        //    //ī�޶� ����
        //    this.transform.Translate(direction * -1 * camFix);
        //}

        LookAround();
=======
        // ���콺 On/Off
        if (Input.GetKeyDown(KeyCode.LeftControl))
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

        if (Cursor.visible == true)
            return;

        RotateAround();
        CalculateZoom();
        FadeOut();
    }

    private void RotateAround()
    {
        // ���콺�� ��ġ�� �޾ƿ���
        x += Input.GetAxis("Mouse X") * sensitivity * 0.01f; // ���콺 �¿� ������ ����
        y -= Input.GetAxis("Mouse Y") * sensitivity * 0.01f; // ���콺 ���� ������ ����

        // ī�޶� ���̰�(������������) ����
        if (y < 0)  // �ٴ��� ���� �ʰ�
            y = 0;
        if (y > 50) // Top View(�������� ��������)�� �ϰ� �ʹٸ� 90���� �ٲٱ�
            y = 50;

        // player.transform�� ���� ����Ұǵ� �ʹ� �� ġȯ => target
        Transform target = player.transform;

        // ī�޶� ȸ���� ������ �̵��� ��ġ ���
        Quaternion angle = Quaternion.Euler(y, x, 0);
        Vector3 destination = angle * (Vector3.back * distance) + target.position + Vector3.zero;

        transform.rotation = angle;             // ī�޶� ���� ����
        transform.position = destination;   // ī�޶� ��ġ ����

        //����ĳ��Ʈ�� ���Ͱ�
        Vector3 ray_target = transform.up * camHeight + transform.forward * camWidth;

        RaycastHit hitinfo;
        Physics.Raycast(transform.position, ray_target, out hitinfo, camDistance);

        if (hitinfo.point != Vector3.zero)//�����ɽ�Ʈ ������
        {
            //point�� �ű��.
            transform.position = hitinfo.point;
            //ī�޶� ����
            transform.Translate(direction * -1 * camFix);
        }
        else
        {
            //������ǥ�� 0���� �����. (ī�޶󸮱׷� �ű��.)
            transform.localPosition = Vector3.zero;
            //ī�޶���ġ������ ���⺤�� * ī�޶� �ִ�Ÿ� �� �ű��.
            transform.Translate(direction * camDistance);
            //ī�޶� ����
            transform.Translate(direction * -1 * camFix);
        }
    }

    // ī�޶� Ȯ���� ���
    void CalculateZoom()
    {
        // ���콺 �� ��/�ƿ�
        distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;

        // �� �ּ�/�ִ� ����
        // Clamp�Լ� : �ִ�/�ּҰ��� �������ְ� ����
        distance = Mathf.Clamp(distance, minZoom, maxZoom);
    }

    // ���ϴ� �÷ξ �ƴ� ���ӿ�����Ʈ�� �÷��̾ ������ ���ϵ��� ������ȭ �ϴ� �޼ҵ�
    private void FadeOut()
    {
        // Raycast�� �̿��Ͽ� �÷��̾�� ī�޶� ���̿� �ִ� ������Ʈ ����
        // ������Ʈ�� �������� �������� Layer�� Ignor Raycast�� �ٲ���ƾ� ��
        // Ignore Raycast: Player, Ground, Particles
        float distance = Vector3.Distance(transform.position, player.transform.position) - 1;
        Vector3 direction = (player.transform.position - transform.position).normalized;
        RaycastHit[] hits;

        // ī�޶󿡼� �÷��̾ ���� �������� ����� �� ���� ������Ʈ�� �ִٸ�
        hits = Physics.RaycastAll(transform.position, direction, distance);

        bool remove = true;
        if (Obstacles.Count != 0 && hits != null)
        {
            for (int i = 0; i < Obstacles.Count; i++)
            {
                foreach (var hit in hits)
                {
                    // hit�� ������Ʈ�� ����Ʈ�� ������� �ʾ��� ���̸� ��� Ž��
                    if (Obstacles[i] != hit.collider.gameObject)
                        continue;
                    // ����� ������Ʈ�� ����
                    else
                    {
                        remove = false;
                        break;
                    }
                }

                // ���� ����̸�
                if (remove == true)
                {
                    ObstacleRenderer = Obstacles[i].GetComponent<MeshRenderer>();
                    RestoreMaterial();

                    Obstacles.Remove(Obstacles[i]);
                }
            }
        }

        if (hits.Length > 0)
        {
            // �̹� ����� ������Ʈ���� Ȯ��
            for (int i = 0; i < hits.Length; i++)
            {
                Debug.DrawRay(transform.position, direction * distance, Color.red);

                transparentObj = hits[i].collider.gameObject;

                // ��ֹ� ���̾ ���ٸ� ���� ������Ʈ �˻�
                if (transparentObj.layer != 13)
                    continue;

                // �̹� ����� ������Ʈ�̸� ���� ������Ʈ �˻�
                if (Obstacles != null && Obstacles.Contains(transparentObj))
                    continue;

                // ������� ���� ������Ʈ�� ����ȭ �� ����Ʈ�� �߰�
                if (transparentObj.layer == 9)
                    ObstacleRenderer = transparentObj.GetComponent<Renderer>();
                if (ObstacleRenderer != null && transparentObj != null)
                {
                    // ������Ʈ�� �������ϰ� �������Ѵ�
                    Material material = ObstacleRenderer.material;
                    Color matColor = material.color;
                    matColor.a = 0.5f;
                    material.color = matColor;

                    // ����Ʈ�� �߰�
                    Obstacles.Add(transparentObj);
                    ObstacleRenderer = null;
                    transparentObj = null;
                }
            }
        }
>>>>>>> Stashed changes
    }

    // ���� ����ȭ�� ������Ʈ�� ���󺹱� �ϴ� �޼ҵ�
    void RestoreMaterial()
    {
<<<<<<< Updated upstream
        // ī�޶� ȸ�� ����
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * Time.fixedDeltaTime * rotSpeed, Space.World);
        transform.Rotate(Vector3.left * Input.GetAxis("Mouse Y") * Time.fixedDeltaTime * rotSpeed, Space.Self);
        transform.position = player.transform.position;

        transform.Translate(player.transform.position);
    }

    public void SetDistance(int weight)
    {
        switch (weight)
        {
            case 0:
                break;
            case 40:
            case 70:
                camWidth = -10.0f;    // ���� �Ÿ�
                camHeight = 4.0f;     // ���� �Ÿ�
                break;
            case 90:
                camWidth = -20.0f;    // ���� �Ÿ�
                camHeight = 5.0f;     // ���� �Ÿ�
                break;
        }
=======
        Material material = ObstacleRenderer.material;
        Color matColor = material.color;
        matColor.a = 1f;    // ���İ� 1:������(���󺹱�)
        material.color = matColor;

        ObstacleRenderer = null;
>>>>>>> Stashed changes
    }
}
