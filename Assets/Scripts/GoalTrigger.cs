using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Placed inside the goal volume. Fires OnGoalScored when the ball enters.
/// </summary>
public class GoalTrigger : MonoBehaviour
{
    public static event UnityAction OnGoalScored;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            OnGoalScored?.Invoke();
        }
    }
}
