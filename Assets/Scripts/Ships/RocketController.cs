using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RocketController : MonoBehaviour
{
    public float thrustPower = 1f;       // ���� ����
    public float rotationSpeed = 0.1f;    // �������� ���������
    private float currentThrust = 0f;     // ������� ����

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // ������� ������� ����
        if (Input.GetKey(KeyCode.LeftShift))
            currentThrust = thrustPower;
        else if (Input.GetKey(KeyCode.LeftControl))
            currentThrust = -thrustPower;
        else
            currentThrust = 0f;
    }

    void FixedUpdate()
    {
        // ������ ���� ������ �������� �� Y (�����)
        rb.AddForce(transform.up * currentThrust);

        float rotation = 0f;

        if (Input.GetKey(KeyCode.A))
            rotation = rotationSpeed;
        else if (Input.GetKey(KeyCode.D))
            rotation = -rotationSpeed;

        rb.AddTorque(Vector3.forward * rotation);
    }
}
