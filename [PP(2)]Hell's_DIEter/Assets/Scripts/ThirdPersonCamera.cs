using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    private GameObject player;

    private float rotSpeed = 5.0f;

    private float camDistance = 0;      // ���׺��� ī�޶������ �Ÿ�
    private float camWidth = -20.0f;    // ���� �Ÿ�
    private float camHeight = 5.0f;     // ���� �Ÿ�
    private float camFix = 3.0f;        // ����ĳ��Ʈ �� ���׸� ���� �� ���� ��(�Ÿ�)

    Vector3 direction;
    Vector3 mouseMove;
    Vector3 stopMouse;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        // ī�޶� ���׺��� ī�޶������ �Ÿ�
        camDistance = Mathf.Sqrt(camWidth * camWidth + camHeight * camHeight);

        // ī�޶� ���׺��� ī�޶� ��ġ������ ���⺤��
        direction = new Vector3(0, camHeight, camWidth).normalized;

        // ���콺 ���� ��ġ ����
        stopMouse = Input.mousePosition;
    }

    private void Update()
    {
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
    }

    private void LookAround()
    {
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
    }
}
