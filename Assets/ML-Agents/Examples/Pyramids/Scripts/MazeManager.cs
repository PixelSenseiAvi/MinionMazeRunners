using UnityEngine;

public class MazeManager : MonoBehaviour
{
    public MazeGoal button; // Drag your single button here
    public Transform agentSpawnPoint;

    public void RandomizeGoalPosition()
    {
        // Example: Move the button to a random position
        //if (buttons.Length > 0)
        //{
        if (button != null)
        {
            Vector3 newPosition = new Vector3(
                Random.Range(-10f, 10f),
                0.5f,
                Random.Range(-10f, 10f)
            );
            button.transform.position = newPosition;
        }
            //}
    }

    public void OnButtonPressed()
    {
        Debug.Log("Button pressed! Gate opened.");
    }
}
