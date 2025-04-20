using System.Collections.Generic;
using UnityEngine;

public class FloatingOrigin : MonoBehaviour
{
    [Tooltip("ֳ��, ������� ��� ���������� ��� � �������� ������ ��� Main Camera")]
    public Transform target;

    [Tooltip("����, ��� ����� ���������� ���� ����")]
    public float threshold = 50000f;

    private List<Rigidbody> shiftableRigidbodies = new List<Rigidbody>();

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("FloatingOrigin: Target �� �����������.");
            return;
        }

        // ����������� ���������� ������ ��� Rigidbody, ��� ���������
        Rigidbody[] allBodies = FindObjectsOfType<Rigidbody>();
        foreach (Rigidbody rb in allBodies)
        {
            if (rb.transform != target)
                shiftableRigidbodies.Add(rb);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // ���� target ������� ������ �� (0, 0, 0), ������� ���
        if (target.position.magnitude > threshold)
        {
            Vector3 offset = target.position;

            // ��������� �� ��'����
            foreach (var rb in shiftableRigidbodies)
            {
                if (rb == null) continue;

                bool wasKinematic = rb.isKinematic;
                rb.isKinematic = true; // ��� �� ��������� � ������� �� ��� �����
                rb.position -= offset;
                rb.isKinematic = wasKinematic;
            }

            // ��������� WorldRoot, ���� �� ������� �� target
            if (transform != target)
                transform.position -= offset;

            var bodies = FindObjectsOfType<OrbitRenderer>();
            foreach (var body in bodies)
            {
                body.DrawOrbit();
            }

            Debug.Log($"FloatingOrigin: ���� ���� �� {offset}");
        }
    }
}
