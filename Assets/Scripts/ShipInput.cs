using UnityEngine;

/// <summary>
/// Class specifically to deal with input.
/// </summary>
public class ShipInput : MonoBehaviour
{
    [Tooltip("When true, the mouse and mousewheel are used for ship input and A/D can be used for strafing like in many arcade space sims.\n\nOtherwise, WASD/Arrows/Joystick + R/T are used for flying, representing a more traditional style space sim.")]
    public bool useMouseInput = true;
    [Tooltip("When using Keyboard/Joystick input, should roll be added to horizontal stick movement. This is a common trick in traditional space sims to help ships roll into turns and gives a more plane-like feeling of flight.")]
    public bool addRoll = true;

    public float inputSensitivity;
    public float rotationSensitivity;
    public float thrustMultiplier;

    [Space]

    [Range(-1, 1)]
    public float pitch;
    [Range(-1, 1)]
    public float yaw;
    [Range(-1, 1)]
    public float roll;
    [Range(-1, 1)]
    public float strafe;
    [Range(0, 1)]
    public float throttle;

    // How quickly the throttle reacts to input.
    private const float THROTTLE_SPEED = 0.5f;

    // Keep a reference to the ship this is attached to just in case.
    private Ship ship;

    // Keep a reference to the engines thrusters to adjust visuals based on thrust
    public GameObject mainThrusters;
    public GameObject auxilaryThrusters;

    private void Awake()
    {
        ship = GetComponent<Ship>();
    }

    private void Update()
    {
        if (useMouseInput)
        {
            strafe = Input.GetAxis("Horizontal") * 0.5f * thrustMultiplier;
            if (addRoll)
                roll = -Input.GetAxis("Roll") * 0.5f * rotationSensitivity;
            SetStickCommandsUsingMouse();
            UpdateMouseWheelThrottle();
            UpdateKeyboardThrottle(KeyCode.W, KeyCode.S);

            mainThrusters.transform.localScale = new Vector3(mainThrusters.transform.localScale.x, mainThrusters.transform.localScale.y, throttle);
            auxilaryThrusters.transform.localScale = new Vector3(auxilaryThrusters.transform.localScale.x, auxilaryThrusters.transform.localScale.y, throttle);
        }
        else
        {
            pitch = Input.GetAxis("Vertical");
            yaw = Input.GetAxis("Horizontal");

            if (addRoll)
                roll = -Input.GetAxis("Horizontal") * 0.5f;

            strafe = 0.0f;
            UpdateKeyboardThrottle(KeyCode.R, KeyCode.F);
        }
    }

    /// <summary>
    /// Freelancer style mouse controls. This uses the mouse to simulate a virtual joystick.
    /// When the mouse is in the center of the screen, this is the same as a centered stick.
    /// </summary>
    private void SetStickCommandsUsingMouse()
    {
        pitch = -Input.GetAxis("Mouse Y") * inputSensitivity;
        yaw = Input.GetAxis("Mouse X") * inputSensitivity;
    }

    /// <summary>
    /// Uses R and F to raise and lower the throttle.
    /// </summary>
    private void UpdateKeyboardThrottle(KeyCode increaseKey, KeyCode decreaseKey)
    {
        float target = throttle;

        if (Input.GetKey(increaseKey))
            target = 1.0f;
        else if (Input.GetKey(decreaseKey))
            target = 0.0f;

        throttle = Mathf.MoveTowards(throttle, target, Time.deltaTime * THROTTLE_SPEED);
    }

    /// <summary>
    /// Uses the mouse wheel to control the throttle.
    /// </summary>
    private void UpdateMouseWheelThrottle()
    {
        throttle += Input.GetAxis("Mouse ScrollWheel");
        throttle = Mathf.Clamp(throttle, 0.0f, 1.0f);
    }
}