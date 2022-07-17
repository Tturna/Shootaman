using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Serialized settings
    [SerializeField] float moveAcceleration;
    [SerializeField] float minWalkSpeed;
    [SerializeField] float maxWalkSpeed;
    [SerializeField] float maxSprintSpeed;
    [SerializeField] float horizontalSensitivity;
    [SerializeField] float verticalSensitivity;

    // Component/GameObject references
    Rigidbody rb;
    Transform camTransform;

    // Utility variables
    Vector3 inputVector;
    Vector3 lookVector;
    [SerializeField] float currentWalkSpeed;
    float maxMoveSpeed;

    void Start()
    {
        // Get component and objext references
        rb = GetComponent<Rigidbody>();
        camTransform = transform.GetChild(0);

        // Initialize variables
        currentWalkSpeed = maxWalkSpeed;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Cursor.visible = false;

        inputVector.x = Input.GetAxis("Horizontal");
        inputVector.z = Input.GetAxis("Vertical");

        lookVector = new Vector3(Input.GetAxis("Mouse X") * horizontalSensitivity, Input.GetAxis("Mouse Y") * -verticalSensitivity);

        // Rotate player and camera with mouse
        camTransform.Rotate(Vector3.right * lookVector.y * Time.deltaTime, Space.Self);
        transform.Rotate(Vector3.up * lookVector.x * Time.deltaTime, Space.Self);

        // Scroll to select walk speed
        float scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta != 0)
        {
            currentWalkSpeed = Mathf.Clamp(currentWalkSpeed + scrollDelta * .1f, minWalkSpeed, maxWalkSpeed);
        }

        // Get sprint input
        maxMoveSpeed = Input.GetKey(KeyCode.LeftShift) ? maxSprintSpeed : currentWalkSpeed;
    }

    private void FixedUpdate()
    {
        if (rb.velocity.magnitude < maxMoveSpeed)
        {
            rb.AddRelativeForce(inputVector * moveAcceleration);
        }
    }
}
