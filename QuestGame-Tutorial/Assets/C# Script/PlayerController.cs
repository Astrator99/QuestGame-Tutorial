using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
   #region Inspector

   [Header("Movement")]
   
   [Min(0)]
   [Tooltip("The maximum speed of the player in uu/s.")]
   [SerializeField] private float movementSpeed = 8f;
   
   [Min(0)]
   [Tooltip("How fast the movement speed is in-/decreasing.")]
   [SerializeField] private float speedChangeRate = 10f;

   [Min(0)]
   [Tooltip("How fast the character rotates around it´s y-axis.")]
   [SerializeField] private float rotationSpeed = 10f;
   
   [Header("Camera")]
   [Tooltip("The focus and rotation point of the camera.")]
   [SerializeField] private Transform cameraTarget;
   
   [Range(-89f, 0)]
   [Tooltip("The minimum vertical camera angle. Lower half of the horizon.")]
   [SerializeField] private float verticalCameraRotationMin = -30f;
   
   [Range(0f, 89f)]
   [Tooltip("The maximum vertical camera angle. Upper half of the horizon.")]
   [SerializeField] private float verticalCameraRotationMax = 70f;
   
   [Min(0)]
   [Tooltip("Sensitivity of the horizontal camera rotation.")]
   [SerializeField] private float cameraHorizontalSpeed = 200f;
   
   [Min(0)]
   [Tooltip("Sensitivity of the vertical camera rotation.")]
   [SerializeField] private float cameraVerticalSpeed = 130f;
   
   
   [Header("Mouse Settings")]
   
   // TODO Put in PlayerPrefs and show in settings.
   [Range(0f, 2)]
   [Tooltip("Additional mouse rotation speed multiplayer.")]
   [SerializeField] private float mouseCameraSensitivity = 1f;
   
   
   [Header("Controller Settings")]
   
   // TODO Put in PlayerPrefs and show in settings.
   [Range(0f, 2)]
   [Tooltip("Additional controller rotation speed multiplayer.")]
   [SerializeField] private float controllerCameraSensitivity = 1f;
   
   // TODO Put in PlayerPrefs and show in settings.
   [Tooltip("Invert y-axis for controller.")]
   [SerializeField] private bool invertY = true;
   
   private CharacterController characterController;
   private GameInput input;
   private InputAction lookAction;
   private InputAction moveAction;
   
   private Vector2 lookInput;
   private Vector2 moveInput;
   
   private Quaternion characterTargetRotation = Quaternion.identity;
   private Vector2 cameraRotation;
   private Vector3 lastMovement;

   #endregion

   #region Unity Event Functions

   private void Awake()
   {
      characterController = GetComponent<CharacterController>();
      
      //Create new input.
      input = new GameInput();
      moveAction = input.Player.Move;
      lookAction = input.Player.Look;

      // TODO Subscribe to input events
   }

   private void OnEnable()
   {
      input.Enable();
   }

   private void Update()
   {
      lookInput = lookAction.ReadValue<Vector2>();
      moveInput = moveAction.ReadValue<Vector2>();
      
      Rotate(moveInput);
      Move(moveInput);
   }

   private void LateUpdate()
   {
      RotateCamera(lookInput);
   }

   private void OnDisable()
   {
      input.Disable();
   }

   

   private void OnDestroy()
   {
      // TODO Unsubscribe form input events.
   }
   #endregion
   #region Movement

   private void Rotate(Vector2 moveInput)
   {
      if (moveInput != Vector2.zero)
      {
         Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

         Vector3 worldInputDirection = cameraTarget.TransformDirection(inputDirection);
         worldInputDirection.y = 0;
         
         characterTargetRotation = Quaternion.LookRotation(worldInputDirection);
      }

      if (Quaternion.Angle(transform.rotation, characterTargetRotation) > 0.1f)
      {
         transform.rotation = Quaternion.Slerp(transform.rotation, characterTargetRotation, rotationSpeed * Time.deltaTime);
      }
      else
      {
         transform.rotation = characterTargetRotation;
      }
   }
   private void Move(Vector2 moveInput)
   {
      float targetSpeed = moveInput == Vector2.zero ? 0f : movementSpeed * moveInput.magnitude;

      Vector3 currentVelocity = lastMovement;
      currentVelocity.y = 0;
      float currentSpeed = currentVelocity.magnitude;

      if (Mathf.Abs(currentSpeed - targetSpeed) > 0.01f)
      {
         currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, speedChangeRate * Time.deltaTime);
      }
      else
      {
         currentSpeed = targetSpeed;
      }

      Vector3 targetDirection = characterTargetRotation * Vector3.forward;
      
      Vector3 movement = targetDirection * currentSpeed;
      
      characterController.SimpleMove(movement);

      lastMovement = movement;
   }

   #endregion

   #region Camera

   private void RotateCamera(Vector2 lookInput)
   {
      if (lookInput != Vector2.zero)
      {
         bool isMouseLook = IsMouseLook();

         float deltaTimeMultiplier = isMouseLook ? 1f : Time.deltaTime;

         float sensitivity = isMouseLook ? mouseCameraSensitivity : controllerCameraSensitivity;

         lookInput *= deltaTimeMultiplier * sensitivity;
         
         cameraRotation.x += lookInput.y * cameraVerticalSpeed * (!isMouseLook && invertY ? -1 : 1);
         cameraRotation.y += lookInput.x * cameraHorizontalSpeed;

         cameraRotation.x = NormalizeAngle(cameraRotation.x);
         cameraRotation.y = NormalizeAngle(cameraRotation.y);

         cameraRotation.x = Mathf.Clamp(cameraRotation.x, verticalCameraRotationMin, verticalCameraRotationMax);
      }
      
      cameraTarget.rotation = Quaternion.Euler(cameraRotation.x, cameraRotation.y, 0f);
   }

   private float NormalizeAngle(float angle)
   {
      // Limit the angle of (-360, 360)
      angle %= 360;

      // Limits the angle to (0, 360)
      if (angle < 0)
      {
         angle += 360;
      }

      // Remaps the angle from [0, 360) to [-180, 180)
      if (angle > 180)
      {
         angle -= 360;
      }

      return angle;
   }

   private bool IsMouseLook()
   {
      if (lookAction.activeControl == null)
      {
         return true;
      }

      return lookAction.activeControl.name == "delta";
   }

   #endregion
}