using UnityEngine; // unity engine core types

[ExecuteAlways] // lets you see the initial placement in edit mode
public class OrbitRig3D : MonoBehaviour // attach to the orbit pivot object (e.g., Moon_Orbit)
{
    [Header("references")] // inspector grouping label
    [SerializeField] private Transform body; // the orbiting object (moon/planet), child of this pivot  // reference

    [Header("orbit shape (slightly elliptical)")] // inspector grouping label
    [SerializeField] private float semiMajorAxis = 25f; // ellipse radius along local x (a)  // shape
    [SerializeField] private float semiMinorAxis = 23f; // ellipse radius along local z (b)  // shape

    [Header("orbit motion")] // inspector grouping label
    [SerializeField] private float orbitDegreesPerSecond = 12f; // angular speed around the ellipse (deg/sec)  // speed
    [SerializeField] private float startPhaseDegrees = 0f; // starting angle on the ellipse (deg)  // initial position

    [Header("orbit plane start (this sets the starting 3d direction)")] // inspector grouping label
    [SerializeField] private Vector3 initialPlaneEuler = Vector3.zero; // initial orbit plane rotation in degrees (x,y,z)  // start direction
    [SerializeField] private float startPrecessionAngleA = 0f; // starting angle for precession A in degrees  // start direction
    [SerializeField] private float startPrecessionAngleB = 0f; // starting angle for precession B in degrees  // start direction

    [Header("3d veer (orbit plane slowly rotates)")] // inspector grouping label
    [SerializeField] private Vector3 precessionAxisA = Vector3.up; // first axis that the orbit plane rotates around  // plane drift
    [SerializeField] private float precessionDegPerSecA = 0.2f; // speed of plane rotation around axis A (deg/sec)  // plane drift
    [SerializeField] private Vector3 precessionAxisB = Vector3.right; // second axis for additional drift (helps "draw a sphere")  // plane drift
    [SerializeField] private float precessionDegPerSecB = 0.08f; // speed of plane rotation around axis B (deg/sec)  // plane drift

    [Header("spin (self rotation)")] // inspector grouping label
    [SerializeField] private float spinDegreesPerSecond = 30f; // how fast the body spins around itself (deg/sec)  // spin
    [SerializeField] private Vector3 spinAxis = Vector3.up; // local axis for body spin  // spin

    [Header("editor behavior")] // inspector grouping label
    [SerializeField] private bool previewInEditMode = true; // updates placement in edit mode without animating  // editor

    private float orbitAngleDeg; // accumulated orbit angle theta(t) in degrees  // state
    private float precessionAngleDegA; // accumulated plane rotation angle around axis A  // state
    private float precessionAngleDegB; // accumulated plane rotation angle around axis B  // state

    void OnEnable() // called when enabled (also in edit mode due to executealways)  // unity lifecycle
    {
        ResetState(); // initialize state from inspector values  // init
        ApplyPose(); // place body immediately so you see it right away  // init
    }

    void OnValidate() // called when inspector values change in edit mode  // unity lifecycle
    {
        if (!previewInEditMode) return; // user opted out of edit-time preview updates  // guard
        if (Application.isPlaying) return; // avoid fighting runtime animation while playing  // guard
        ResetState(); // keep preview consistent with inspector values  // init
        ApplyPose(); // refresh placement in the scene view  // preview
    }

    void Update() // called every frame  // unity lifecycle
    {
        if (body == null) return; // no body assigned, nothing to move/spin  // safety

        if (!Application.isPlaying) // if not playing, don't animate  // guard
        {
            if (previewInEditMode) ApplyPose(); // keep static preview correct in edit mode  // preview
            return; // exit before doing time-based motion  // guard
        }

        float dt = Time.deltaTime; // seconds since last frame  // timing

        orbitAngleDeg += orbitDegreesPerSecond * dt; // advance orbit angle smoothly forward  // orbit
        precessionAngleDegA += precessionDegPerSecA * dt; // advance plane drift A smoothly forward  // plane drift
        precessionAngleDegB += precessionDegPerSecB * dt; // advance plane drift B smoothly forward  // plane drift

        ApplyPose(); // compute and apply new 3d position  // placement

        float spinStep = spinDegreesPerSecond * dt; // degrees to spin this frame  // timing
        body.Rotate(spinAxis, spinStep, Space.Self); // spin the body around its own axis  // spin
    }

    private void ResetState() // initializes runtime accumulators from inspector values  // helper
    {
        semiMajorAxis = Mathf.Max(0.001f, semiMajorAxis); // prevent degenerate ellipse  // validation
        semiMinorAxis = Mathf.Max(0.001f, semiMinorAxis); // prevent degenerate ellipse  // validation
        orbitAngleDeg = startPhaseDegrees; // start orbit at chosen phase  // init
        precessionAngleDegA = startPrecessionAngleA; // apply user-chosen starting plane rotation component A  // init
        precessionAngleDegB = startPrecessionAngleB; // apply user-chosen starting plane rotation component B  // init
    }

    private void ApplyPose() // computes position on an ellipse, then rotates that ellipse plane in 3d  // helper
    {
        Vector3 axisA = precessionAxisA.sqrMagnitude > 0.0001f ? precessionAxisA.normalized : Vector3.up; // safe axis A  // validation
        Vector3 axisB = precessionAxisB.sqrMagnitude > 0.0001f ? precessionAxisB.normalized : Vector3.right; // safe axis B  // validation

        float theta = orbitAngleDeg * Mathf.Deg2Rad; // orbit parameter angle in radians  // trig

        float x0 = Mathf.Cos(theta) * semiMajorAxis; // ellipse x coordinate  // shape
        float z0 = Mathf.Sin(theta) * semiMinorAxis; // ellipse z coordinate  // shape
        Vector3 inPlane = new Vector3(x0, 0f, z0); // point on ellipse in a flat reference plane  // point

        Quaternion qStart = Quaternion.Euler(initialPlaneEuler); // sets the starting orbit plane orientation  // start direction
        Quaternion qA = Quaternion.AngleAxis(precessionAngleDegA, axisA); // rotate around axis A  // plane rotation
        Quaternion qB = Quaternion.AngleAxis(precessionAngleDegB, axisB); // rotate around axis B  // plane rotation
        Quaternion planeRot = qStart * qA * qB; // combine rotations into final orbit plane rotation  // plane rotation

        Vector3 tilted = planeRot * inPlane; // rotate the orbit plane in 3d so y becomes nonzero  // 3d orbit
        body.localPosition = tilted; // assign local position relative to the orbit pivot  // placement
    }
}
