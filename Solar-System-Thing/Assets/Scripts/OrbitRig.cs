using UnityEngine;

[ExecuteAlways] // lets you see the initial placement in edit mode
public class OrbitRig3D : MonoBehaviour 
{
    [Header("references")]
    [SerializeField] private Transform body; // the orbiting object

    [Header("orbit shape (slightly elliptical)")]
    [SerializeField] private float semiMajorAxis = 25f; // ellipse radius along local x (a)
    [SerializeField] private float semiMinorAxis = 23f; // ellipse radius along local z (b)

    [Header("orbit motion")]
    [SerializeField] private float orbitDegreesPerSecond = 12f; // angular speed around the ellipse
    [SerializeField] private float startPhaseDegrees = 0f; // starting angle on the ellipse

    [Header("orbit plane start (this sets the starting 3d direction)")]
    [SerializeField] private Vector3 initialPlaneEuler = Vector3.zero; // initial orbit plane rotation in degrees (x,y,z)
    [SerializeField] private float startPrecessionAngleA = 0f;
    [SerializeField] private float startPrecessionAngleB = 0f; 

    [Header("3d veer (orbit plane slowly rotates)")]
    [SerializeField] private Vector3 precessionAxisA = Vector3.up; // first axis that the orbit plane rotates around  // plane drift
    [SerializeField] private float precessionDegPerSecA = 0.2f; // speed of plane rotation around axis A   // plane drift
    [SerializeField] private Vector3 precessionAxisB = Vector3.right; // second axis for additional drift  // plane drift
    [SerializeField] private float precessionDegPerSecB = 0.08f; // speed of plane rotation around axis B  // plane drift

    [Header("spin (self rotation)")] 
    [SerializeField] private float spinDegreesPerSecond = 30f;
    [SerializeField] private Vector3 spinAxis = Vector3.up; // local axis for body spin

    [Header("editor behavior")] 
    [SerializeField] private bool previewInEditMode = true; // updates placement in edit mode without animating 

    private float orbitAngleDeg; // accumulated orbit angle theta(t) in degrees  // state
    private float precessionAngleDegA; // accumulated plane rotation angle around axis A  // state
    private float precessionAngleDegB; // accumulated plane rotation angle around axis B  // state

    void OnEnable()
    {
        ResetState(); // init
        ApplyPose(); // place body immediately
    }

    void OnValidate() // runs in edit mode whenever serialized fields change, scripts recompile, or component is added/loaded
    {
        if (!previewInEditMode) return;
        if (Application.isPlaying) return; // prevents the editor from fighting Update() logic
        ResetState(); // re-initialize internal runtime variables from inspector values so the scene preview matches the configuration; prevents stale angle accumulators from previous edits
        ApplyPose(); // immediately recompute and apply the object's orbit position to visually see the effect of inspector edits in the scene view
    }

    void Update()
    {
        if (body == null) return;

        if (!Application.isPlaying) // guard clause for edit mode
        {
            if (previewInEditMode) ApplyPose(); // if preview is enabled, we still snap the body into the correct pose so the scene stays consistent with inspector values 
            return; // stops here so we do not advance orbit angles using deltaTime in edit mode 
        }

        float dt = Time.deltaTime;

        orbitAngleDeg += orbitDegreesPerSecond * dt; // Euler integration for angular velocity: angle(t) = angle(t-1) + speed * dt
        precessionAngleDegA += precessionDegPerSecA * dt; // same integration idea, but for orbit-plane rotation around axis A; this is the slow veer that makes paths wander in 3D over time
        precessionAngleDegB += precessionDegPerSecB * dt; // using two distinct axes/speeds gives richer 3D coverage (avoids everything staying in one band)

        ApplyPose(); // after updating the angles, we compute the new position from the orbit math and assign it

        float spinStep = spinDegreesPerSecond * dt; // computes this frame's local spin amount in degrees
        body.Rotate(spinAxis, spinStep, Space.Self); // rotates the body around its own local axis; Space.Self uses the body's local coordinate system (so spin axis follows the body if it tilts)
    }

    private void ResetState() // helper method: re-syncs internal accumulators and validates inspector data; called from OnEnable/OnValidate so edits don't leave the object in a weird state
    {
        semiMajorAxis = Mathf.Max(0.001f, semiMajorAxis); // clamp: prevents a=0 which would collapse ellipse into a line/point; 0.001f avoids divide-by-zero-ish degenerate geometry and NaNs
        semiMinorAxis = Mathf.Max(0.001f, semiMinorAxis); // same clamp for b axis; keeps trig results meaningful and ensures the orbit remains drawable

        orbitAngleDeg = startPhaseDegrees; // sets the orbit's starting parameter angle (theta) to the inspector-defined phase; this controls where on the ellipse the body begins
        precessionAngleDegA = startPrecessionAngleA; // sets initial plane rotation around axis A; lets each planet start with a different 3D lane even before time advances
        precessionAngleDegB = startPrecessionAngleB; // sets initial plane rotation around axis B; combined with A, this provides a broad range of starting orientations
    }

    private void ApplyPose() // core geometry function: (1) compute point on ellipse in a base plane, (2) build a 3D rotation representing orbit-plane orientation, (3) rotate point into 3D and apply it
    {
        Vector3 axisA = precessionAxisA.sqrMagnitude > 0.0001f ? precessionAxisA.normalized : Vector3.up; // choose a safe rotation axis: if (0,0,0), normalized would be invalid; sqrMagnitude avoids expensive sqrt
        Vector3 axisB = precessionAxisB.sqrMagnitude > 0.0001f ? precessionAxisB.normalized : Vector3.right; // same safety for axis B; fallback axes provide deterministic behavior rather than silently breaking

        float theta = orbitAngleDeg * Mathf.Deg2Rad; // trig functions take radians, not degrees

        float x0 = Mathf.Cos(theta) * semiMajorAxis; // parametric ellipse equation: x = a cos(theta); semiMajorAxis is a; cos controls left/right along ellipse
        float z0 = Mathf.Sin(theta) * semiMinorAxis; // parametric ellipse equation: z = b sin(theta); semiMinorAxis is b; sin controls forward/back along ellipse
        Vector3 inPlane = new Vector3(x0, 0f, z0); // base orbit is defined in the local XZ plane (y=0); we intentionally start flat so we can later rotate the plane into any 3D orientation

        Quaternion qStart = Quaternion.Euler(initialPlaneEuler); // quaternion from euler: defines initial plane orientation; euler is convenient for authors, quaternion is stable for composition and avoids gimbal issues during accumulation
        Quaternion qA = Quaternion.AngleAxis(precessionAngleDegA, axisA); // build a rotation of angle A about axisA; AngleAxis is mathematically "rotate around arbitrary axis" and is ideal for precession-like motion
        Quaternion qB = Quaternion.AngleAxis(precessionAngleDegB, axisB); // second axis rotation; by using two independent rotations, the plane's normal vector can explore more directions over time

        Quaternion planeRot = qStart * qA * qB; // quaternion composition: multiplication applies rotations in sequence (rightmost happens first in effect); order matters because 3D rotations do not commute

        Vector3 tilted = planeRot * inPlane; // apply rotation to the position vector: quaternion-vector multiplication rotates the vector; this turns a flat ellipse point into a 3D orbit point (y becomes nonzero)
        body.localPosition = tilted; // assign in local space of the orbit pivot object; localPosition means the pivot stays at earth center and we move only the body relative to that center (clean hierarchy design)
    }

}
