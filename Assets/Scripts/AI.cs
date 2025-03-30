using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

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
    [SerializeField] private float minX, maxX, minZ, maxZ;

    [Header("Wander Settings")]
    [SerializeField] private float wanderRadius = 1f;
    [SerializeField] private float wanderDistance = 3f;
    [SerializeField] private float wanderJitter = 0.5f;

    [Header("Path Following")]
    [SerializeField] private PathFollowing pathFollowing;

    [Header("Obstacle Avoidance")]
    [SerializeField] private float obstacleDetectionDistance = 5f;
    [SerializeField] private float avoidanceStrength = 10f;

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

        if (velocity != Vector3.zero)
        {
            transform.forward = velocity.normalized;
        }

        velocity = Vector3.Lerp(velocity, velocity.normalized * maxSpeed, Time.deltaTime * 2f);

        Vector3 newPosition = transform.position + velocity * Time.deltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);

        transform.position = newPosition;
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
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            currentBehaviour = TypeSteeringBehaviour.PathFollowing;
            Debug.Log("Cambiado a PathFollowing");
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            currentBehaviour = TypeSteeringBehaviour.ObstacleAdvoice;
            Debug.Log("Cambiado a Obstacle Avoidance");
        }
    }
    private Vector3 CalculateSteeringForce()
    {
        if (target == null) return Vector3.zero;

        Vector3 steeringForce = Vector3.zero;

        switch (currentBehaviour)
        {
            case TypeSteeringBehaviour.Seek:
                steeringForce = CalculateSeek(target.position);
                break;
            case TypeSteeringBehaviour.Flee:
                steeringForce = CalculateFlee(target.position);
                break;
            case TypeSteeringBehaviour.Evade:
                steeringForce = CalculateEvade();
                break;
            case TypeSteeringBehaviour.Arrive:
                steeringForce = CalculateArrive(target.position);
                break;
            case TypeSteeringBehaviour.Pursuit:
                steeringForce = CalculatePursuit();
                break;
            case TypeSteeringBehaviour.Wander:
                CalculateWander();
                return Vector3.zero;
            case TypeSteeringBehaviour.PathFollowing:
                steeringForce = CalculatePathFollowing();
                break;
            case TypeSteeringBehaviour.ObstacleAdvoice:
                steeringForce = CalculateObstacleAvoidance();
                break;
        }

        steeringForce += CalculateObstacleAvoidance();

        return steeringForce;
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
        targetFuturePosition.y = transform.position.y;

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
    private Vector3 CalculatePathFollowing()
    {
        if (pathFollowing == null) return Vector3.zero;

        Transform nextPoint = pathFollowing.NextPoint(transform.position);
        if (nextPoint == null) return Vector3.zero;

        return CalculateSeek(nextPoint.position);
    }
    private Vector3 CalculateObstacleAvoidance()
    {
        RaycastHit hit;
        Vector3 avoidanceForce = Vector3.zero;
        bool obstacleDetected = false;

        // Rayo central
        if (Physics.Raycast(transform.position, transform.forward, out hit, obstacleDetectionDistance))
        {
            avoidanceForce += Vector3.Reflect(transform.forward, hit.normal) * avoidanceStrength;
            obstacleDetected = true;
        }

        // Rayo izquierdo
        if (Physics.Raycast(transform.position, Quaternion.Euler(0, -30, 0) * transform.forward, out hit, obstacleDetectionDistance * 0.75f))
        {
            avoidanceForce += Vector3.Reflect(transform.forward, hit.normal) * (avoidanceStrength * 0.5f);
            obstacleDetected = true;
        }

        // Rayo derecho
        if (Physics.Raycast(transform.position, Quaternion.Euler(0, 30, 0) * transform.forward, out hit, obstacleDetectionDistance * 0.75f))
        {
            avoidanceForce += Vector3.Reflect(transform.forward, hit.normal) * (avoidanceStrength * 0.5f);
            obstacleDetected = true;
        }

        if (obstacleDetected)
        {
            if (velocity.magnitude < 0.1f)
            {
                transform.Rotate(0, Random.Range(-90f, 90f), 0);
                avoidanceForce += -transform.forward * 15f; 
            }

            return avoidanceForce.normalized * avoidanceStrength;
        }
        return Vector3.zero;
    }
}
