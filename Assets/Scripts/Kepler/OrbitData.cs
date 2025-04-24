using System;
using UnityEngine;

[Serializable]
public class OrbitData
{
    public double MG;
    public double SemiMinorAxis;
    public double SemiMajorAxis;
    public double FocalParameter;
    public double Eccentricity;
    public double Period;
    public double TrueAnomaly;
    public double MeanAnomaly;
    public double EccentricAnomaly;
    public double MeanMotion;
    public Vector3d Periapsis;
    public double PeriapsisDistance;
    public Vector3d Apoapsis;
    public double ApoapsisDistance;
    public Vector3d CenterPoint;
    public double OrbitCompressionRatio;
    public Vector3d OrbitNormal;
    public Vector3d SemiMinorAxisBasis;
    public Vector3d SemiMajorAxisBasis;

    public Vector3d positionRelativeToAttractor;
    public Vector3d velocityRelativeToAttractor;

    public double AttractorMass;
    public double AttractorDistance;

    public static readonly Vector3d EclipticRight = new Vector3d(1, 0, 0);
    public static readonly Vector3d EclipticUp = new Vector3d(0, 1, 0);
    public static readonly Vector3d EclipticNormal = new Vector3d(0, 0, 1);

    #region Initialization

    /// <summary>
    /// ����������� ������
    /// </summary>
    public OrbitData() { }

    /// <summary>
    /// ����������� �� ��������� �����: position, velocity, attractorMass, gConst
    /// </summary>
    public OrbitData(Vector3d position, Vector3d velocity, double attractorMass, double gConst)
    {
        // �������� ������� �����
        this.positionRelativeToAttractor = position;
        this.velocityRelativeToAttractor = velocity;
        // ? = G * M
        this.MG = attractorMass * gConst;
        // ���������� ��� ����� ����
        CalculateOrbitStateFromOrbitalVectors();
    }

    /// <summary>
    /// ����������� �� ����� ��������� ���������� ��������:
    /// ��������������� e, ������ ���� a, �������� �����볿 M?, ������ i?,
    /// ��������� ���������� ??, ������� ���������� ����� ??, ���� ��������� �� G.
    /// </summary>
    public OrbitData(double eccentricity,
                     double semiMajorAxis,
                     double meanAnomalyDeg,
                     double inclinationDeg,
                     double argOfPerifocusDeg,
                     double ascendingNodeDeg,
                     double attractorMass,
                     double gConst)
    {
        // �������� ����� �����/��������
        this.Eccentricity = eccentricity;
        this.SemiMajorAxis = semiMajorAxis;

        // ���������� ���� ���� ��� �������� (e<1) ��� ���������� (e>1) �������
        if (eccentricity < 1.0)
            this.SemiMinorAxis = SemiMajorAxis * Math.Sqrt(1 - eccentricity * eccentricity);
        else if (eccentricity > 1.0)
            this.SemiMinorAxis = SemiMajorAxis * Math.Sqrt(eccentricity * eccentricity - 1);
        else
            this.SemiMinorAxis = 0;  // ����������� �������

        // ����������� ������� ��������� ������� �� ����� ������� ��������
        var normal = EclipticNormal.normalized;
        var ascendingNode = EclipticRight.normalized;

        // ��������� ���� �� �������� [-180�,180�]
        ascendingNodeDeg %= 360;
        if (ascendingNodeDeg > 180) ascendingNodeDeg -= 360;
        inclinationDeg %= 360;
        if (inclinationDeg > 180) inclinationDeg -= 360;
        argOfPerifocusDeg %= 360;
        if (argOfPerifocusDeg > 180) argOfPerifocusDeg -= 360;

        // ���������� ������ ����� ������� ������ �� ��� ?
        ascendingNode = Vector3d.RotateVectorByAngle(
                           ascendingNode,
                           ascendingNodeDeg * Vector3d.Deg2Rad,
                           normal
                       ).normalized;

        // ��� �������� ������� ������� ������ ��������� ������� �� ��� i
        normal = Vector3d.RotateVectorByAngle(
                     normal,
                     inclinationDeg * Vector3d.Deg2Rad,
                     ascendingNode
                 ).normalized;

        // ������ ���������: ��������� �� ������� ��, ���� �������� �� ?
        Periapsis = Vector3d.RotateVectorByAngle(
                        ascendingNode,
                        argOfPerifocusDeg * Vector3d.Deg2Rad,
                        normal
                    ).normalized;

        // ������ ������ �� ���� ������ � ����������� �������
        this.SemiMajorAxisBasis = Periapsis;
        this.SemiMinorAxisBasis = Vector3d.Cross(Periapsis, normal).normalized;

        // ������������ ����� � �������� �����볿 � ������
        this.MeanAnomaly = meanAnomalyDeg * Vector3d.Deg2Rad;
        this.EccentricAnomaly = Utils.ConvertMeanToEccentricAnomaly(
                                    this.MeanAnomaly,
                                    this.Eccentricity
                                );
        this.TrueAnomaly = Utils.ConvertEccentricToTrueAnomaly(
                                    this.EccentricAnomaly,
                                    this.Eccentricity
                                );

        // ��������� ���������
        this.AttractorMass = attractorMass;
        this.MG = gConst;

        // ������ ���������� ��� ���� �� ����� ��� ��������
        CalculateOrbitStateFromOrbitalVectors();
    }

    /// <summary>
    /// �������� �� ���� OrbitData �� ����� 
    /// positionRelativeToAttractor, velocityRelativeToAttractor � MG.
    /// </summary>
    public void CalculateOrbitStateFromOrbitalVectors()
    {
        // ����� �����
        SemiMajorAxis = ComputeSemiMajorAxis();
        Eccentricity = ComputeEccentricity();
        SemiMinorAxis = ComputeSemiMinorAxis();
        FocalParameter = ComputeFocalParameter();
        OrbitCompressionRatio = SemiMinorAxis / SemiMajorAxis;

        // ����� ��������������
        Period = ComputePeriod();
        MeanMotion = ComputeMeanMotion();

        // �����볿
        TrueAnomaly = ComputeTrueAnomaly();
        EccentricAnomaly = ComputeEccentricAnomaly();
        MeanAnomaly = ComputeMeanAnomaly();

        // �������� � �������
        OrbitNormal = GetOrbitalPlaneNormal();
        SemiMajorAxisBasis = GetSemiMajorBasis();
        SemiMinorAxisBasis = GetSemiMinorBasis();

        // ����� �� ����
        PeriapsisDistance = ComputePeriapsisDistance();
        ApoapsisDistance = ComputeApoapsisDistance();
        CenterPoint = ComputeEllipseCenter();
        Periapsis = GetPeriapsisPoint();
        Apoapsis = GetApoapsisPoint();

        // ���-�����
        // MG ��� ������
    }

    #endregion

    #region VectorCalculations

    /// <summary>
    /// ���������� ������ ��������: h = r ? v
    /// </summary>
    public Vector3d ComputeSpecificAngularMomentum()
    {
        return Vector3d.Cross(positionRelativeToAttractor, velocityRelativeToAttractor);
    }

    /// <summary>
    /// ������ �� �����: n = k ? h (k = (0,0,1))
    /// </summary>
    public Vector3d ComputeNodeVector()
    {
        Vector3d h = ComputeSpecificAngularMomentum();
        return Vector3d.Cross(new Vector3d(0, 0, 1), h);
    }

    /// <summary>
    /// ������ ���������������: e = (v ? h)/? - r?
    /// </summary>
    public Vector3d ComputeEccentricityVector()
    {
        Vector3d h = ComputeSpecificAngularMomentum();
        Vector3d term = Vector3d.Cross(velocityRelativeToAttractor, h) / MG;
        Vector3d rNorm = positionRelativeToAttractor.normalized;
        return term - rNorm;
    }

    /// <summary>
    /// ���������� �������� ������: ? = v?/2 ? ?/r
    /// </summary>
    public double ComputeSpecificEnergy()
    {
        double v2 = velocityRelativeToAttractor.sqrMagnitude;
        double r = positionRelativeToAttractor.magnitude;
        return v2 / 2.0 - MG / r;
    }

    /// <summary>
    /// ������ ������ �����: n = (r ? v).normalized
    /// </summary>
    public Vector3d ComputeOrbitNormal()
    {
        return Vector3d.Cross(positionRelativeToAttractor, velocityRelativeToAttractor).normalized;
    }

    #endregion

    #region OrbitalGeometry

    /// <summary>
    /// ���������� ������ ����: a = -? / (2?)
    /// </summary>
    public double ComputeSemiMajorAxis()
    {
        double energy = ComputeSpecificEnergy();
        return -MG / (2 * energy);
    }

    /// <summary>
    /// ���������� ���������������: e = |e?|
    /// </summary>
    public double ComputeEccentricity()
    {
        return ComputeEccentricityVector().magnitude;
    }

    /// <summary>
    /// ���������� ���� ����: b = a * sqrt(1 - e^2)
    /// </summary>
    public double ComputeSemiMinorAxis()
    {
        double a = ComputeSemiMajorAxis();
        double e = ComputeEccentricity();
        return a * Math.Sqrt(1 - e * e);
    }

    /// <summary>
    /// �������� ������ (����������� ��������): p = a * (1 - e^2)
    /// </summary>
    public double ComputeFocalParameter()
    {
        double a = ComputeSemiMajorAxis();
        double e = ComputeEccentricity();
        return a * (1 - e * e);
    }

    /// <summary>
    /// ���������� �����: T = 2? * sqrt(a^3 / ?)
    /// </summary>
    public double ComputePeriod()
    {
        double a = ComputeSemiMajorAxis();
        return 2 * Math.PI * Math.Sqrt(a * a * a / MG);
    }

    /// <summary>
    /// ������� ���: n = sqrt(? / a^3)
    /// </summary>
    public double ComputeMeanMotion()
    {
        double a = ComputeSemiMajorAxis();
        return Math.Sqrt(MG / (a * a * a));
    }

    #endregion

    #region OrbitalAnomalies

    /// <summary>
    /// ���������� ������� �����볿 ? �� ���������� �� �������� ���������������
    /// </summary>
    public double ComputeTrueAnomaly()
    {
        Vector3d e = ComputeEccentricityVector();
        Vector3d r = positionRelativeToAttractor;

        double cosNu = Vector3d.Dot(e, r.normalized) / e.magnitude;
        double nu = Math.Acos(Math.Clamp(cosNu, -1.0, 1.0));

        // �������� ������� �������� ������� ����
        if (Vector3d.Dot(r, velocityRelativeToAttractor) < 0)
            nu = 2 * Math.PI - nu;

        return nu;
    }

    /// <summary>
    /// ���������� ������������ �����볿 E �� �������� �����볺� ?
    /// </summary>
    public double ComputeEccentricAnomaly()
    {
        double e = ComputeEccentricity();
        double nu = ComputeTrueAnomaly();

        double cosE = (e + Math.Cos(nu)) / (1 + e * Math.Cos(nu));
        double sinE = Math.Sqrt(1 - e * e) * Math.Sin(nu) / (1 + e * Math.Cos(nu));
        return Math.Atan2(sinE, cosE);
    }

    /// <summary>
    /// ���������� �������� �����볿 M �� ������������� �����볺�
    /// M = E - e * sin(E)
    /// </summary>
    public double ComputeMeanAnomaly()
    {
        double e = ComputeEccentricity();
        double E = ComputeEccentricAnomaly();
        return E - e * Math.Sin(E);
    }

    #endregion

    #region OrbitalOrientation

    /// <summary>
    /// ������� �� ������� ����� (��������� ������)
    /// </summary>
    public Vector3d GetOrbitalPlaneNormal()
    {
        return ComputeOrbitNormal();
    }

    /// <summary>
    /// �������� �� �������� (��������� ������)
    /// </summary>
    public Vector3d GetPeriapsisDirection()
    {
        return ComputeEccentricityVector().normalized;
    }

    /// <summary>
    /// �������� �� �������� (����������� �� ���������)
    /// </summary>
    public Vector3d GetApoapsisDirection()
    {
        return -GetPeriapsisDirection();
    }

    /// <summary>
    /// ����� ������ ���� (�� �������� ���������)
    /// </summary>
    public Vector3d GetSemiMajorBasis()
    {
        return GetPeriapsisDirection();
    }

    /// <summary>
    /// ����� ���� ���� (� ������ �����, ������������� �� ������)
    /// </summary>
    public Vector3d GetSemiMinorBasis()
    {
        return Vector3d.Cross(GetOrbitalPlaneNormal(), GetSemiMajorBasis()).normalized;
    }

    /// <summary>
    /// ��� ������ ����� (i): �� ���������� �������� �� ���� Z
    /// </summary>
    public double GetInclination()
    {
        Vector3d h = GetOrbitalPlaneNormal();
        return Math.Acos(Math.Clamp(h.z, -1.0, 1.0));
    }

    /// <summary>
    /// ������� ���������� ����� (?): �� ���� X � �������� �����
    /// </summary>
    public double GetLongitudeOfAscendingNode()
    {
        Vector3d n = ComputeNodeVector().normalized;
        double angle = Math.Acos(Math.Clamp(n.x, -1.0, 1.0));
        if (n.y < 0) angle = 2 * Math.PI - angle;
        return angle;
    }

    /// <summary>
    /// �������� ���������� (?): �� �������� ����� � ��������� �� ��������
    /// </summary>
    public double GetArgumentOfPeriapsis()
    {
        Vector3d n = ComputeNodeVector().normalized;
        Vector3d e = GetPeriapsisDirection();

        double angle = Math.Acos(Math.Clamp(Vector3d.Dot(n, e), -1.0, 1.0));
        if (e.z < 0) angle = 2 * Math.PI - angle;
        return angle;
    }

    #endregion

    #region OrbitalPoints

    /// <summary>
    /// ������� ������� ��'���� �� ���� (��� ������)
    /// </summary>
    public Vector3d GetCurrentPosition()
    {
        return positionRelativeToAttractor;
    }

    /// <summary>
    /// ������� �������� ��'���� �� ���� (��� ������)
    /// </summary>
    public Vector3d GetCurrentVelocity()
    {
        return velocityRelativeToAttractor;
    }

    /// <summary>
    /// ������� ��������� � ������� �����������
    /// </summary>
    public Vector3d GetPeriapsisPoint()
    {
        return CenterPoint + GetPeriapsisDirection() * ComputePeriapsisDistance();
    }

    /// <summary>
    /// ������� ��������� � ������� �����������
    /// </summary>
    public Vector3d GetApoapsisPoint()
    {
        return CenterPoint + GetApoapsisDirection() * ComputeApoapsisDistance();
    }

    /// <summary>
    /// ³������ �� ���������: r_p = a * (1 - e)
    /// </summary>
    public double ComputePeriapsisDistance()
    {
        double a = ComputeSemiMajorAxis();
        double e = ComputeEccentricity();
        return a * (1 - e);
    }

    /// <summary>
    /// ³������ �� ���������: r_a = a * (1 + e)
    /// </summary>
    public double ComputeApoapsisDistance()
    {
        double a = ComputeSemiMajorAxis();
        double e = ComputeEccentricity();
        return a * (1 + e);
    }

    /// <summary>
    /// ����� ����� (�������� ������ ��, �� ���- � ����������)
    /// </summary>
    public Vector3d ComputeEllipseCenter()
    {
        return (GetPeriapsisPoint() + GetApoapsisPoint()) * 0.5;
    }

    #endregion

    #region TemporalCalculations

    /// <summary>
    /// ���������� �������� �����볿 � ������� ���
    /// M(t) = M0 + n * (t - t0)
    /// </summary>
    public double ComputeMeanAnomalyAtTime(double meanAnomalyAtEpoch, double meanMotion, double timeSinceEpoch)
    {
        double M = meanAnomalyAtEpoch + meanMotion * timeSinceEpoch;
        return NormalizeAngle(M);
    }

    /// <summary>
    /// ���������� ������� �����볿 � ������������
    /// </summary>
    public double ComputeTrueAnomalyFromEccentric(double E, double e)
    {
        double cosNu = (Math.Cos(E) - e) / (1 - e * Math.Cos(E));
        double sinNu = (Math.Sqrt(1 - e * e) * Math.Sin(E)) / (1 - e * Math.Cos(E));
        return Math.Atan2(sinNu, cosNu);
    }

    /// <summary>
    /// ������� �� ���� � ������ ���� t
    /// </summary>
    public Vector3d ComputePositionAtTime(double timeSinceEpoch)
    {
        double a = ComputeSemiMajorAxis();
        double e = ComputeEccentricity();
        double M = ComputeMeanAnomalyAtTime(MeanAnomaly, ComputeMeanMotion(), timeSinceEpoch);
        double E = Utils.SolveKeplersEquation(M, e);
        double nu = ComputeTrueAnomalyFromEccentric(E, e);

        double r = a * (1 - e * e) / (1 + e * Math.Cos(nu));

        Vector3d direction = GetPeriapsisDirection().RotateAround(OrbitNormal, nu);
        return CenterPoint + direction * r;
    }

    /// <summary>
    /// ����������� ���� � ����� [0, 2?]
    /// </summary>
    private double NormalizeAngle(double angle)
    {
        double twoPi = 2 * Math.PI;
        angle = angle % twoPi;
        if (angle < 0) angle += twoPi;
        return angle;
    }

    #endregion
}
