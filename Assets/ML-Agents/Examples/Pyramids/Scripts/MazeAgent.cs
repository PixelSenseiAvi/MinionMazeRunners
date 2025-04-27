using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MazeAgent : Agent
{
    [Header("Navigation Parameters")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 200f;
    public Transform goalTransform; // Assign button's transform in Unity

    [Header("Sensors")]
    public float rayMaxDistance = 5f;
    public LayerMask detectableLayers; // Include Wall/Button layers

    private Rigidbody rb;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private float previousDistance;
    private bool buttonPressed;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
        buttonPressed = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent velocity (normalized)
        sensor.AddObservation(rb.linearVelocity.normalized);

        // Raycast setup
        float[] rayAngles = { -90f, -45f, 0f, 45f, 90f }; // 5 rays at 45° intervals

        var rayInput = new RayPerceptionInput
        {
            RayLength = rayMaxDistance,
            DetectableTags = new[] { "Wall", "Button" },
            Angles = rayAngles,
            StartOffset = 0f,
            EndOffset = 0f,
            Transform = transform,
            CastType = RayPerceptionCastType.Cast3D,
            LayerMask = detectableLayers
        };

        var rayOutput = RayPerceptionSensor.Perceive(rayInput, batched: false);

        // Flatten raycast results into observations
        foreach (var ray in rayOutput.RayOutputs)
        {
            sensor.AddObservation(ray.HasHit ? 1f : 0f); // Hit detected
            sensor.AddObservation(ray.HitFraction);      // Distance (0-1)
            sensor.AddObservation((float)ray.HitTagIndex); // Tag index
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Discrete actions: [0] Movement, [1] Rotation
        int moveAction = actions.DiscreteActions[0];
        int rotateAction = actions.DiscreteActions[1];

        // Movement
        Vector3 move = Vector3.zero;
        switch (moveAction)
        {
            case 1: // Forward
                move = transform.forward * moveSpeed;
                break;
            case 2: // Backward
                move = -transform.forward * moveSpeed;
                break;
        }

        // Rotation
        float rotation = 0f;
        switch (rotateAction)
        {
            case 1: // Right
                rotation = 1f;
                break;
            case 2: // Left
                rotation = -1f;
                break;
        }

        // Apply movement and rotation
        rb.AddForce(move, ForceMode.VelocityChange);
        transform.Rotate(0f, rotation * rotationSpeed * Time.deltaTime, 0f);

        // Reward for approaching button
        float currentDistance = Vector3.Distance(transform.position, goalTransform.position);
        AddReward((previousDistance - currentDistance) * 0.1f); // Scale reward
        previousDistance = currentDistance;

        // Time penalty to encourage speed
        AddReward(-0.002f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0; // Reset movement
        discreteActions[1] = 0; // Reset rotation

        // Keyboard controls for testing
        if (Input.GetKey(KeyCode.W)) discreteActions[0] = 1;
        else if (Input.GetKey(KeyCode.S)) discreteActions[0] = 2;

        if (Input.GetKey(KeyCode.D)) discreteActions[1] = 1;
        else if (Input.GetKey(KeyCode.A)) discreteActions[1] = 2;
    }

    public override void OnEpisodeBegin()
    {
        // Reset agent state
        transform.localPosition = startPosition;
        transform.localRotation = startRotation;
        rb.linearVelocity = Vector3.zero;
        previousDistance = Vector3.Distance(transform.position, goalTransform.position);
        buttonPressed = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Button") && !buttonPressed)
        {
            AddReward(1.0f); // Success reward
            buttonPressed = true;
            EndEpisode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.1f); // Wall collision penalty
        }
    }

    // Optional: Visualize rays in Scene view
    private void Update()
    {
        foreach (var angle in new float[] { -90f, -45f, 0f, 45f, 90f })
        {
            Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;
            Debug.DrawRay(transform.position, dir * rayMaxDistance, Color.cyan);
        }
    }
}