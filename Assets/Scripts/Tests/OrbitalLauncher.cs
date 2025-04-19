using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class OrbitalLauncher : MonoBehaviour
{
    public Transform planet; // ҳ��, ������� ����� �����
    public float altitudeAboveSurface = 2f; // ������ ��� ��������� �������

    private Rigidbody rb;
    private Rigidbody planetrb;
    private GravityConfig config;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        planetrb = planet.GetComponent<Rigidbody>();

        config = GravityConfig.Instance ?? Resources.Load<GravityConfig>("Scripts/Configs/GravityConfig");

        if (config == null)
            Debug.LogError("GravityConfig is missing.");

        // ���������� ������ �� ������ ������� �� ��'����
        float planetRadius = planet.localScale.x / 2f; // ���� ������� � localScale
        float orbitalRadius = planetRadius + altitudeAboveSurface;

        // ����������� ��'��� �� ��������� �������
        Vector3 upDirection = (transform.position - planet.position).normalized;
        transform.position = planet.position + upDirection * orbitalRadius;

        // ���������� ��������� ��������
        float orbitalVelocity = Mathf.Sqrt(config.gravitationalConstant * planetrb.mass / orbitalRadius);

        // ������� ������ ������������� (������������� �� ���� ������)
        Vector3 orbitalDirection = Vector3.Cross(upDirection, Vector3.forward).normalized;
        if (orbitalDirection == Vector3.zero) // �����������, ���� forward �������������� � up
        {
            orbitalDirection = Vector3.Cross(upDirection, Vector3.right).normalized;
        }

        // ������������ ��������� ��������
        rb.velocity = orbitalDirection * orbitalVelocity;

        Debug.Log($"������ �� �����! ��������� ��������: {orbitalVelocity}, ������: {orbitalDirection}");
    }
}
