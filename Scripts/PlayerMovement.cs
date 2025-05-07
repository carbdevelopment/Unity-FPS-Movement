using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float jumpPower = 7f;
    public float gravity = 10f;

    [Header("Camera Settings")]
    public Camera playerCamera;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float maxTiltAngle = 10f;
    public float cameraSmoothing = 0.06f;

    [Header("Footstep Settings")]
    public AudioSource footstepAudioSource;
    public AudioClip[] walkFootstepSounds;
    public AudioClip[] runFootstepSounds;
    public float walkStepInterval = 0.5f;
    public float runStepInterval = 0.3f;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private float rotationY = 0;
    private float nextStepTime = 0f;

    [Header("State Management")]
    public bool canMove = true;
    public bool canRun = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rotationY = transform.eulerAngles.y;
        rotationX = transform.eulerAngles.x;
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleFootsteps();
    }

    private void HandleMovement()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        bool isRunning = canRun && Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleRotation()
    {
        if (!canMove) return;

        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        rotationY += Input.GetAxis("Mouse X") * lookSpeed;
        float tiltZ = -Input.GetAxis("Mouse X") * maxTiltAngle;
        Quaternion targetCameraRotation = Quaternion.Euler(rotationX, 0, tiltZ);
        playerCamera.transform.localRotation = Quaternion.Slerp(playerCamera.transform.localRotation, targetCameraRotation, cameraSmoothing);
        Quaternion targetBodyRotation = Quaternion.Euler(0, rotationY, 0);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetBodyRotation, cameraSmoothing);
    }

    private void HandleFootsteps()
    {
        if (!characterController.isGrounded || characterController.velocity.magnitude <= 0) return;

        float stepInterval = Input.GetKey(KeyCode.LeftShift) ? runStepInterval : walkStepInterval;

        if (Time.time >= nextStepTime)
        {
            PlayFootstepSound(Input.GetKey(KeyCode.LeftShift));
            nextStepTime = Time.time + stepInterval;
        }
    }
    
    private void PlayFootstepSound(bool isRunning)
    {
        AudioClip[] selectedFootstepSounds = isRunning ? runFootstepSounds : walkFootstepSounds;

        if (selectedFootstepSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, selectedFootstepSounds.Length);
            footstepAudioSource.clip = selectedFootstepSounds[randomIndex];
            footstepAudioSource.Play();
        }
    }
}

