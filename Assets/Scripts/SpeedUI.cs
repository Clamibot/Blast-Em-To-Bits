using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows throttle and speed of the player ship.
/// </summary>
public class SpeedUI : MonoBehaviour
{
    private Text text;

    private void Awake()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (text != null && Ship.PlayerShip != null)
        {
            text.text = string.Format("THROTTLE: {0}%\nSPEED: {1}%", (Ship.PlayerShip.Throttle * 100.0f).ToString("000"), Ship.PlayerShip.Velocity.magnitude.ToString("000"));
        }
    }
}
