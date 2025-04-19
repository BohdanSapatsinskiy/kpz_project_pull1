using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(Rigidbody))]
public class TrajectoryPredictor : MonoBehaviour
{
    [Header("������������ ����������")]
    public int steps = 300; // ʳ������ �����
    public float timeStep = 0.1f; // ��� �� ������� (� ��������)
    public float lineWidth = 0.1f;
    public Color lineColor = Color.green;
    public GravityConfig config;

    [Header("����������� �������")]
    public Transform gravitySource;

    private LineRenderer lineRenderer;
    private Rigidbody rb;
    private Rigidbody planetrb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = steps;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;

        planetrb = gravitySource.GetComponent<Rigidbody>();

        config = GravityConfig.Instance ?? Resources.Load<GravityConfig>("Scripts/Configs/GravityConfig");
        if (config == null)
            Debug.LogError("GravityConfig is missing.");
    }

    private void OnTriggerEnter(Collider other)
    {
        planetrb = other.GetComponent<Rigidbody>();
        if (planetrb != null) 
        {
            gravitySource = other.transform;
        }
    }

    void LateUpdate()
    {
        SimulateTrajectory();
    }

    void SimulateTrajectory()
    {
        Vector3[] positions = new Vector3[steps];
        Vector3 position = transform.position;
        Vector3 velocity = rb.velocity;

        for (int i = 0; i < steps; i++)
        {
            // �������� �������
            positions[i] = position;

            // ���������� ���� ���������
            Vector3 dir = (gravitySource.position - position).normalized;
            float distance = Vector3.Distance(position, gravitySource.position);
            float forceMag = config.gravitationalConstant * planetrb.mass / (distance * distance);
            Vector3 gravity = dir * forceMag;

            // ���������� (����������)
            velocity += gravity * timeStep;
            position += velocity * timeStep;
        }

        lineRenderer.positionCount = steps;
        lineRenderer.SetPositions(positions);
    }
}
