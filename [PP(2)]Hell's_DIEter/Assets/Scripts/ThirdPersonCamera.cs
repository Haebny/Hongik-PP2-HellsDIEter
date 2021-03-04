using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    private GameObject player;

    private float rotSpeed = 2.0f;

    private float camDistance = 0;      // ���׺��� ī�޶������ �Ÿ�
    private float camWidth = -10.0f;    // ���� �Ÿ�
    private float camHeight = 4.0f;     // ���� �Ÿ�
    private float camFix = 3.0f;        // ����ĳ��Ʈ �� ���׸� ���� �� ���� ��(�Ÿ�)

    Vector3 direction;
    Vector3 mouseMove;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        // ī�޶� ���׺��� ī�޶������ �Ÿ�
        camDistance = Mathf.Sqrt(camWidth * camWidth + camHeight * camHeight);

        // ī�޶� ���׺��� ī�޶� ��ġ������ ���⺤��
        direction = new Vector3(0, camHeight, camWidth).normalized;
    }

    private void Update()
    {
        LookAround();
    }

    private void LookAround()
    {
        // ī�޶� ȸ�� ����
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * Time.deltaTime * rotSpeed, Space.World);
        transform.Rotate(Vector3.left * Input.GetAxis("Mouse Y") * Time.deltaTime * rotSpeed, Space.Self);
        transform.position = player.transform.position;

        transform.Translate(player.transform.position);
        Debug.DrawRay(transform.position, new Vector3(transform.forward.x, 0f, transform.forward.z).normalized, Color.red);
    }
}
