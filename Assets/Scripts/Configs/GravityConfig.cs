using UnityEngine;

/// <summary>
/// �������� ��������� ���������, �� ����� ��������� � ���� �������.
/// ������������� ���� ��� �������� ������������ � Unity.
/// 
/// ==== �������� ������� ====
/// ============================
/// </summary>
[CreateAssetMenu(fileName = "GravityConfig", menuName = "Configs/GravityConfig")]
public class GravityConfig : ScriptableObject
{
    [Header("Գ���� ���������")]
    public float gravitationalConstant = 66.588f;

    public static GravityConfig Instance;

    private void OnEnable()
    {
        Instance = this;
    }
}
