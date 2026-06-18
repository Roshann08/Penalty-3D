using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Orchestrates game state: Aim → Kick → Result → Reset.
/// Attach to an empty "GameManager" GameObject in the scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("References")]
    public BallController      ball;
    public GoalkeeperController goalkeeper;

    [Header("Game Rules")]
    public int totalKicks = 5;

    [Header("Events — wire these to UI")]
    public UnityEvent<int, int> OnScoreChanged;   // (goals, kicks taken)
    public UnityEvent<string>   OnResultMessage;  // "GOAL!", "SAVED!", "MISS!"
    public UnityEvent           OnGameOver;

    public enum State { Idle, Aiming, InFlight, Result, GameOver }
    public State CurrentState { get; private set; } = State.Idle;

    int _goals;
    int _kicksTaken;

    float _resultDisplayTime = 2.5f;

    void OnEnable()
    {
        GoalTrigger.OnGoalScored += HandleGoal;
    }

    void OnDisable()
    {
        GoalTrigger.OnGoalScored -= HandleGoal;
    }

    void Start()
    {
        CurrentState = State.Aiming;
    }

    /// <summary>
    /// Called by BallLauncher when the player kicks.
    /// direction: normalised world-space kick direction.
    /// power: 0–1.
    /// sideOffset: -1 to +1 curl.
    /// </summary>
    public void OnKick(Vector3 direction, float power, float sideOffset)
    {
        if (CurrentState != State.Aiming) return;

        CurrentState = State.InFlight;
        _kicksTaken++;

        ball.Kick(direction, power, sideOffset);

        // Predict where ball will cross the goal line and tell keeper
        Vector3 predicted = PredictGoalLineCross(direction, power);
        goalkeeper.ReactToBall(predicted);

        StartCoroutine(WaitForResult());
    }

    IEnumerator WaitForResult()
    {
        // Give ball time to reach the goal or land
        yield return new WaitForSeconds(2.0f);

        // If we're still InFlight here, ball didn't score (miss or saved)
        if (CurrentState == State.InFlight)
        {
            HandleMiss();
        }
    }

    void HandleGoal()
    {
        if (CurrentState != State.InFlight) return;
        CurrentState = State.Result;

        _goals++;
        OnScoreChanged?.Invoke(_goals, _kicksTaken);
        OnResultMessage?.Invoke("GOAL!");

        StartCoroutine(NextKickOrEnd());
    }

    void HandleMiss()
    {
        CurrentState = State.Result;

        // Simple heuristic: if goalkeeper dived toward ball it's a save, else miss
        OnResultMessage?.Invoke("SAVED!");

        OnScoreChanged?.Invoke(_goals, _kicksTaken);
        StartCoroutine(NextKickOrEnd());
    }

    IEnumerator NextKickOrEnd()
    {
        yield return new WaitForSeconds(_resultDisplayTime);

        if (_kicksTaken >= totalKicks)
        {
            CurrentState = State.GameOver;
            OnGameOver?.Invoke();
        }
        else
        {
            ball.ResetBall();
            goalkeeper.ResetKeeper();
            CurrentState = State.Aiming;
        }
    }

    /// <summary>
    /// Simple linear prediction of where the ball crosses Z = goal Z.
    /// Returns a world position the goalkeeper should dive toward.
    /// </summary>
    Vector3 PredictGoalLineCross(Vector3 direction, float power)
    {
        if (direction.z <= 0f) return goalkeeper.transform.position; // ball going away

        float goalZ  = goalkeeper.transform.position.z;
        float ballZ  = ball.transform.position.z;
        float travelZ = goalZ - ballZ;

        // t = distance / horizontal speed (approximation)
        float speed = power * ball.maxKickForce / ball.GetComponent<Rigidbody>().mass;
        float t = travelZ / (direction.z * speed);

        float predX = ball.transform.position.x + direction.x * speed * t;
        float predY = ball.transform.position.y + direction.y * speed * t
                      - 0.5f * 9.81f * t * t; // gravity

        return new Vector3(predX, Mathf.Max(0f, predY), goalZ);
    }

    public void RestartGame()
    {
        _goals = 0;
        _kicksTaken = 0;
        ball.ResetBall();
        goalkeeper.ResetKeeper();
        OnScoreChanged?.Invoke(0, 0);
        CurrentState = State.Aiming;
    }
}
