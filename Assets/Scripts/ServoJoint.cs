using UnityEngine;
using System.Collections;

public enum Axis { X, Y, Z }

public class ServoJoint : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private ServoOffset servoOffset;
    [SerializeField] [Range(-180f, 180f)] private float startAngle;
    [SerializeField] [Range(-180f, 180f)] private float offset;
    [SerializeField] [Range(0, 360)] private int range = 180;
    [SerializeField] private int minPWM = 100;
    [SerializeField] private int maxPWM = 600;
    [SerializeField] private int motorID = 1;
    [SerializeField] private Axis axis;
    [SerializeField] private bool flip;
    [SerializeField] private bool useCustomAngleConversion;

    private Vector3 eulerAngles;
    private float lastPwm = -1f;
    private float angle;
    private int lastSignal = -69420;
    private int currentSignal = -69420;
    public int CurrentSignal
    {
        get { return currentSignal; }
        set { currentSignal = value; }
    }

    public int MotorId
    {
        get { return motorID; }
        set { motorID = value; }
    }

    VrMessages _vrmessage;
    private void Awake()
    {
        _vrmessage = FindObjectOfType<VrMessages>();
    }
    private void Start()
    {
        _ = StartCoroutine(CalculateSignal());
    }

    private IEnumerator CalculateSignal()
    {
        lastPwm = -1f;
        lastSignal = -69420;

        if (!target)
        {
            target = transform;
        }

        while (true)
        {
            if (!VRobot.IsPaused)
            {
                eulerAngles = useCustomAngleConversion ? QuaternionToEulerAngles(target.localRotation) : target.localEulerAngles;

                angle = axis switch
                {
                    Axis.X => eulerAngles.x,
                    Axis.Y => eulerAngles.y,
                    Axis.Z => eulerAngles.z,
                    _ => eulerAngles.z,
                };

                if (startAngle < 0f && angle >= 180f)
                {
                    angle -= 360f;
                }

                angle = Mathf.Clamp(angle + offset, startAngle, startAngle + range);

                float pwm01 = Mathf.Clamp01((angle - startAngle) / range);

                if (flip)
                {
                    pwm01 = 1f - pwm01;
                }

                if (lastPwm < 0f || Mathf.Abs(pwm01 - lastPwm) <= 0.25f)
                {
                    currentSignal = Mathf.Clamp(Mathf.RoundToInt(minPWM + ((maxPWM - minPWM) * pwm01)) + (servoOffset ? servoOffset.Offset : 0), minPWM, maxPWM);
                    if (Mathf.Abs(currentSignal - lastSignal) >= 15f)
                    {
                        if (SerialHandler.SendSerialData(motorID + "," + currentSignal))
                        {
                            lastSignal = currentSignal;
                            _vrmessage.createMessage(motorID.ToString(), currentSignal.ToString());
                        }
                    }
                }

                lastPwm = pwm01;
            }

            yield return null;
        }
    }

    private Vector3 QuaternionToEulerAngles(Quaternion q)
    {
        // Roll (x-axis rotation)
        float sinr_cosp = 2 * (q.w * q.x + q.y * q.z);
        float cosr_cosp = 1 - 2 * (q.x * q.x + q.y * q.y);
        float roll = Mathf.Atan2(sinr_cosp, cosr_cosp);

        // Pitch (y-axis rotation)
        float sinp = 2 * (q.w * q.y - q.z * q.x);
        float pitch;
        if (Mathf.Abs(sinp) >= 1)
            pitch = Mathf.Sign(sinp) * Mathf.PI / 2; // Use 90 degrees if out of range
        else
            pitch = Mathf.Asin(sinp);

        // Yaw (z-axis rotation)
        float siny_cosp = 2 * (q.w * q.z + q.x * q.y);
        float cosy_cosp = 1 - 2 * (q.y * q.y + q.z * q.z);
        float yaw = Mathf.Atan2(siny_cosp, cosy_cosp);

        // Convert radians to degrees
        return new Vector3(roll * Mathf.Rad2Deg, pitch * Mathf.Rad2Deg, yaw * Mathf.Rad2Deg);
    }
}
