using UnityEngine;

public class BallFinishTrigger : MonoBehaviour
{
    [Tooltip("Tag of the object that triggers the win (e.g., 'Player' or 'Ball')")]
    public string ballTag = "Ball"; 

    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is the ball
        if (other.CompareTag(ballTag))
        {
            Debug.Log("Ball reached the finish!");
            
            // Notify the manager to stop the timer and calculate score
            if (GameTimeManager.Instance != null)
            {
                GameTimeManager.Instance.FinishGame();
            }
        }
    }
}