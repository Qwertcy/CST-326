using UnityEngine; // unity core types like transform, vector3, quaternion, time
using UnityEngine.InputSystem; // new input system access (keyboard.current, key states)

public class OrbitRigTour : MonoBehaviour // attach to the orbit pivot (camera rig parent)
{
    [Header("references")]
    [SerializeField] private Transform cameraBody; // assign main camera transform (child or referenced)  // camera ref
    [SerializeField] private Transform lookTarget; // assign earth (or center object)  // look target

    [Header("orbit shape")]
    [SerializeField] private float semiMajorAxis = 120f; // ellipse radius along local x  // orbit size
    [SerializeField] private float semiMinorAxis = 100f; // ellipse radius along local z  // orbit size
    [SerializeField] private Vector3 initialPlaneEuler = new Vector3(90f, 0f, 0f); // rotates orbit plane vertical  // orbit plane

    [Header("orbit motion")]
    [SerializeField] private float orbitDegreesPerSecond = 6f; // how fast we travel along the ellipse  // orbit speed
    [SerializeField] private float precessionDegPerSecA = 0.03f; // slow drift component a  // veer
    [SerializeField] private float precessionDegPerSecB = 0.015f; // slow drift component b  // veer

    [Header("look behavior")]
    [SerializeField] private Vector3 lookOffset = new Vector3(0f, 3f, 0f); // aim slightly above earth center  // framing
    [SerializeField] private bool lockRoll = true; // keeps horizon stable by removing roll  // comfort

    [Header("overhead lock (space)")]
    [SerializeField] private float overheadHeight = 35f; // how high above target we go in lock mode  // lock pos
    [SerializeField] private float overheadDistance = 12f; // small forward offset so target isn't perfectly centered  // lock pos
    [SerializeField] private float lockLerpSpeed = 4f; // how fast we blend into/out of lock mode  // smoothing

    private float orbitAngleDeg; // current orbit parameter angle in degrees  // state
    private float precessionA; // accumulated precession angle a in degrees  // state
    private float precessionB; // accumulated precession angle b in degrees  // state

    private bool overheadLocked; // whether space lock is currently active  // state
    private Vector3 lastOrbitCamWorldPos; // last computed orbit camera world position (for smooth return)  // state

    void Awake() // runs once when object becomes active  // lifecycle
    {
        if (cameraBody == null || lookTarget == null) return; // we need both references to work  // safety

        transform.localRotation = Quaternion.Euler(initialPlaneEuler); // set initial orbit plane orientation  // setup
        orbitAngleDeg = Random.Range(0f, 360f); // random starting angle so things aren't aligned  // setup

        lastOrbitCamWorldPos = cameraBody.position; // initialize return position safely  // setup
    }

    void Update() // runs every frame  // lifecycle
    {
        handleInputNewSystem(); // poll keyboard using new input system  // input

        if (!overheadLocked) // only advance the orbit when we're not locked overhead  // mode
        {
            updateOrbitState(); // advance angles forward in time  // orbit
            lastOrbitCamWorldPos = computeOrbitCameraWorldPos(); // compute where orbit wants camera to be  // orbit
        }

        updateCameraPose(); // apply either orbit-follow or overhead lock pose  // pose
    }

    void handleInputNewSystem() // reads space key using the new input system  // input helper
    {
        if (Keyboard.current == null) return; // can be null on some platforms or before input initializes  // safety

        if (Keyboard.current.spaceKey.wasPressedThisFrame) // space press event (edge)  // input
        {
            overheadLocked = true; // enter overhead mode  // state
        }

        if (Keyboard.current.spaceKey.wasReleasedThisFrame) // space release event (edge)  // input
        {
            overheadLocked = false; // exit overhead mode and return to orbit  // state
        }
    }

    void updateOrbitState() // advances orbit and precession angles smoothly  // orbit helper
    {
        float dt = Time.deltaTime; // elapsed seconds since last frame  // timing

        orbitAngleDeg += orbitDegreesPerSecond * dt; // move forward along the orbit each frame  // orbit
        precessionA += precessionDegPerSecA * dt; // slowly drift orbit plane component a  // veer
        precessionB += precessionDegPerSecB * dt; // slowly drift orbit plane component b  // veer
    }

    Vector3 computeOrbitCameraWorldPos() // computes the orbit camera position in world space  // orbit helper
    {
        float theta = orbitAngleDeg * Mathf.Deg2Rad; // convert degrees to radians for sin/cos  // trig

        float x = Mathf.Cos(theta) * semiMajorAxis; // ellipse x coordinate  // shape
        float z = Mathf.Sin(theta) * semiMinorAxis; // ellipse z coordinate  // shape
        Vector3 inPlane = new Vector3(x, 0f, z); // point on ellipse in the xz plane  // base plane

        Quaternion precessionRot = Quaternion.Euler(precessionA, precessionB, 0f); // simple drifting rotation  // drift
        Vector3 driftedOffset = precessionRot * inPlane; // rotate ellipse offset into 3d  // 3d offset

        return lookTarget.position + driftedOffset; // orbit around target's world position  // final
    }

    void updateCameraPose() // blends camera between orbit pose and overhead lock pose  // pose helper
    {
        if (cameraBody == null || lookTarget == null) return; // ensure references exist  // safety

        if (overheadLocked) // overhead mode (space held)  // mode
        {
            Vector3 overheadPos =
                lookTarget.position + // start at target position  // base
                Vector3.up * overheadHeight + // move straight up above target  // height
                transform.forward * overheadDistance; // add small forward offset for nicer framing  // offset

            cameraBody.position = Vector3.Lerp(cameraBody.position, overheadPos, Time.deltaTime * lockLerpSpeed); // smooth approach  // smoothing
            cameraBody.LookAt(lookTarget.position + lookOffset, Vector3.up); // keep camera aimed at target  // aim
        }
        else // orbit mode (space not held)  // mode
        {
            cameraBody.position = Vector3.Lerp(cameraBody.position, lastOrbitCamWorldPos, Time.deltaTime * lockLerpSpeed); // smooth return  // smoothing
            cameraBody.LookAt(lookTarget.position + lookOffset, Vector3.up); // keep aiming at target during orbit too  // aim
        }

        if (lockRoll) // optional roll lock to prevent camera tilt nausea  // option
        {
            Vector3 e = cameraBody.rotation.eulerAngles; // get rotation as euler angles  // read
            cameraBody.rotation = Quaternion.Euler(e.x, e.y, 0f); // zero roll (z) while preserving pitch/yaw  // apply
        }
    }
}
