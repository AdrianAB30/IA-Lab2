using UnityEngine;
using System.Collections.Generic;

public enum TypeSteeringBehaviour
{
    Seek,
    Flee,
    Evade,
    Arrive,
    Pursuit,
    Wander,
    PathFollowing,
    ObstacleAdvoice,
}
public class AI : MonoBehaviour
{
    [Header("Steering Settings")]
    public TypeSteeringBehaviour currentBehaviour = TypeSteeringBehaviour.Seek;
    [SerializeField] Transform target;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float maxForce = 5f;
    [SerializeField] private float mass = 1f;
    [SerializeField] private float slowingRadius = 5f; 
    [SerializeField] private float evasionPredictionTime = 2f;

    [Header("Wander Settings")]
    [SerializeField] private float wanderRadius = 1f;
    [SerializeField] private float wanderDistance = 3f;
    [SerializeField] private float wanderJitter = 0.5f;

    private Vector3 velocity;
    private Vector3 targetPreviousPosition;
    private Vector3 wanderTarget;

    private void Start()
    {
        velocity = transform.forward;
    }

    private void Update()
    {
        HandleInput();

        Vector3 steeringForce = CalculateSteeringForce();

        if (currentBehaviour != TypeSteeringBehaviour.Wander)
        {
            steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);
            steeringForce /= mass;
            velocity = Vector3.ClampMagnitude(velocity + steeringForce, maxSpeed);
        }

        transform.position += velocity * Time.deltaTime;

        if (velocity != Vector3.zero)
        {
            transform.forward = velocity.normalized;
        }

        velocity = Vector3.Lerp(velocity, velocity.normalized * maxSpeed, Time.deltaTime * 2f);
    }
    private void LateUpdate()
    {
        if (target != null)
        {
            targetPreviousPosition = target.position;
        }
    }
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentBehaviour = TypeSteeringBehaviour.Seek;
            Debug.Log("Cambiado a Seek");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentBehaviour = TypeSteeringBehaviour.Flee;
            Debug.Log("Cambiado a Flee");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentBehaviour = TypeSteeringBehaviour.Evade;
            Debug.Log("Cambiado a Evade");
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            currentBehaviour = TypeSteeringBehaviour.Arrive;
            Debug.Log("Cambiado a Arrive");
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            currentBehaviour = TypeSteeringBehaviour.Pursuit;
            Debug.Log("Cambiado a Pursuit");
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            currentBehaviour = TypeSteeringBehaviour.Wander;
            Debug.Log("Cambiado a Wander");
        }
    }
    private Vector3 CalculateSteeringForce()
    {
        if (target == null) return Vector3.zero;

        switch (currentBehaviour)
        {
            case TypeSteeringBehaviour.Seek:
                return CalculateSeek(target.position);
            case TypeSteeringBehaviour.Flee:
                return CalculateFlee(target.position);
            case TypeSteeringBehaviour.Evade:
                return CalculateEvade();
            case TypeSteeringBehaviour.Arrive:
                return CalculateArrive(target.position);
            case TypeSteeringBehaviour.Pursuit:
                return CalculatePursuit();
            case TypeSteeringBehaviour.Wander:
                 CalculateWander();
                return Vector3.zero;

            default:
                return Vector3.zero;
        }
    }
    private Vector3 CalculateSeek(Vector3 targetPosition)
    {
        Vector3 desiredVelocity = (targetPosition - transform.position).normalized * maxSpeed;
        return desiredVelocity - velocity;
    }

    private Vector3 CalculateFlee(Vector3 targetPosition)
    {
        Vector3 desiredVelocity = (transform.position - targetPosition).normalized * maxSpeed;
        return desiredVelocity - velocity;
    }
    private Vector3 CalculateEvade()
    {
        Vector3 targetVelocity = (target.position - targetPreviousPosition) / Time.deltaTime;

        Vector3 targetFuturePosition = target.position + targetVelocity * evasionPredictionTime;

        return CalculateFlee(targetFuturePosition);
    }
    private Vector3 CalculateArrive(Vector3 targetPosition)
    {
        Vector3 toTarget = targetPosition - transform.position;
        float distance = toTarget.magnitude;

        if (distance < 0.1f)
            return -velocity; 

        float rampedSpeed = maxSpeed * (distance / slowingRadius);
        float clampedSpeed = Mathf.Min(rampedSpeed, maxSpeed);

        Vector3 desiredVelocity = (toTarget / distance) * clampedSpeed;
        return desiredVelocity - velocity;
    }
    private Vector3 CalculatePursuit()
    {
        Vector3 targetVelocity = (target.position - targetPreviousPosition) / Time.deltaTime;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        float predictionTime = distanceToTarget / (maxSpeed + targetVelocity.magnitude);

        Vector3 predictedPosition = target.position + targetVelocity * predictionTime;

        return CalculateSeek(predictedPosition);
    }
    private void CalculateWander()
    {
        wanderTarget += new Vector3(
        Random.Range(-1f, 1f) * wanderJitter * Time.deltaTime,
        Random.Range(-1f, 1f) * wanderJitter * Time.deltaTime
    );

        wanderTarget = wanderTarget.normalized * wanderRadius;

        Vector3 targetLocal = wanderTarget + Vector3.forward * wanderDistance;
        Vector3 targetWorld = transform.TransformPoint(targetLocal);

        targetWorld.y = transform.position.y;

        Vector3 desiredVelocity = (targetWorld - transform.position).normalized * maxSpeed;
        desiredVelocity.y = 0;

        velocity += (desiredVelocity - velocity) * Time.deltaTime;
        velocity.y = 0;
    }
}
