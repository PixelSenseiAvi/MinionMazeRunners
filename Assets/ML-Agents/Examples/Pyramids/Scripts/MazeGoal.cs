using UnityEngine;

public class MazeGoal : MazeManager
{
    [Header("Visual Settings")]
    public Material onMaterial; // Material when button is pressed
    public Material offMaterial; // Material when button is idle
    public Renderer buttonRenderer; // Assign the button's renderer in Inspector

    [Header("Gate Settings")]
    public GameObject gateObject; // Assign the gate GameObject (disable collider/renderer when opened)
    public bool isEpisodic = true; // Reset button/gate on episode end

    private bool m_State; // Tracks if button is pressed
    private MazeManager m_MazeManager;

    void Start()
    {
        // Initialize button to "off" state
        buttonRenderer.material = offMaterial;
        m_MazeManager = GetComponentInParent<MazeManager>(); // Works if button is a child of MazeManager
    }

    // Called when the agent interacts with the button
    public void PressButton()
    {
        if (!m_State)
        {
            m_State = true;
            buttonRenderer.material = onMaterial;

            //// Open the gate (disable collider/renderer or trigger animation)
            if (gateObject != null)
            {
                gateObject.SetActive(false); // Simple disable
                // Alternatively: gateObject.GetComponent<Collider>().enabled = false;
            }

            // Optional: Notify MazeManager to track completion
            if (m_MazeManager != null)
            {
                m_MazeManager.OnButtonPressed();
            }
        }
    }

    // Reset the button and gate (call this in OnEpisodeBegin)
    public void ResetButton()
    {
        m_State = false;
        buttonRenderer.material = offMaterial;

        

        // Optional: Randomize button position (if using procedural mazes)
        if (m_MazeManager != null)
        {
            m_MazeManager.RandomizeGoalPosition();
        }
    }

    // Detect agent collision (ensure agent has a Rigidbody and Collider)
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Agent") && !m_State)
        {
            PressButton();
        }
    }
}
