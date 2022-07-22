using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Tturna.ThreeD.Weapons;
using Tturna.Interaction;

namespace Tturna.ThreeD.FPS
{
    public class Tt_FPSController : Tt_Agent
    {
        // Serialized settings
        [SerializeField] float moveSpeed;
        [SerializeField] float sprintMultiplier;
        [SerializeField] float slideMultiplier;
        [SerializeField] float moveSmoothSpeed;
        [SerializeField] float gravityMultiplier;
        [SerializeField] float horizontalSensitivity;
        [SerializeField] float verticalSensitivity;
        [SerializeField] AnimationCurve jumpVelocityCurve;
        [SerializeField] float armsSmoothSpeed;

        // Required references
        [SerializeField] GameObject fpsArms;
        [SerializeField] Image hurtVignette;

        // Component/GameObject references
        CharacterController cc;
        Camera cam;
        Tt_WeaponInventory wi;

        // point in space the player is looking at
        public RaycastHit lookRayHit;
        public Vector3 lookPoint, lookDirection;
        public float lookPointDistance;

        // Utility variables
        Vector3 inputVector;
        Vector3 lookVector;
        Vector3 velocityVector;
        Vector3 slideVector;
        float camXAngle;
        float jumpEvalTime;
        float moveSpeedMultiplier = 1;
        bool isJumping, isSprinting;
        bool lastGroundCheckHit;
        int rayMask => LayerMask.GetMask(new string[] { "Default", "Enemy", "Interactable" });

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;

            cc = GetComponent<CharacterController>();
            cam = Camera.main;
            wi = Tt_WeaponInventory.Instance;
            Tt_WeaponInventory.WeaponFired += OnWeaponFired;
        }

        private void Update()
        {
            Cursor.visible = false;

            #region Inputs
            inputVector.x = Input.GetAxis("Horizontal");
            inputVector.z = Input.GetAxis("Vertical");

            Vector3 camForward = cam.transform.forward;
            Vector3 camRight = cam.transform.right;

            // disregard Y because the forward vector will change when the camera looks up or down
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 localInputVector = camForward * inputVector.z + camRight * inputVector.x;

            lookVector = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            #endregion

            #region Raycasting and updating values
            velocityVector = Vector3.Lerp(velocityVector, localInputVector.normalized * moveSpeed * moveSpeedMultiplier, Time.deltaTime * moveSmoothSpeed);

            // Get the point in the world that the player is looking at
            bool hit = Physics.Raycast(cam.transform.position, cam.transform.forward, out lookRayHit, 100, rayMask);

            lookPoint = hit ? lookRayHit.point : cam.transform.position + cam.transform.forward * 10;
            Vector3 camToPoint = lookPoint - cam.transform.position;
            lookDirection = camToPoint.normalized;
            lookPointDistance = camToPoint.magnitude;
            #endregion

            #region Movement
            if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
            {
                jumpEvalTime = 0;
                isJumping = true;
            }

            if (isJumping)
            {
                float eval = jumpVelocityCurve.Evaluate(jumpEvalTime);
                cc.Move(Vector3.up * eval * Time.deltaTime);
                jumpEvalTime += Time.deltaTime;
            }

            // Crouching
            if (Input.GetKeyDown(KeyCode.C))
            {
                cc.height = 1;
                Tt_ChildCameraController.instance.SetPosition(Vector3.up * .5f);

                // Check if sprinting -> slide
                if (isSprinting)
                {
                    slideVector = velocityVector * slideMultiplier;
                }
                else
                {
                    moveSpeedMultiplier = .5f;
                }
            }
            else if (Input.GetKey(KeyCode.C) && slideVector != Vector3.zero)
            {
                slideVector = Vector3.Lerp(slideVector, Vector3.zero, Time.deltaTime * 2);
                velocityVector = slideVector;
            }
            else if (Input.GetKeyUp(KeyCode.C))
            {
                cc.height = 2;
                Tt_ChildCameraController.instance.ResetPosition();
                moveSpeedMultiplier = 1;
                slideVector = Vector3.zero;
            }

            // Sprinting
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                isSprinting = true;
                moveSpeedMultiplier = sprintMultiplier;
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                isSprinting = false;
                moveSpeedMultiplier = 1;
            }

            cc.Move((Vector3.down * gravityMultiplier + velocityVector) * Time.deltaTime);

            // Rotation
            Quaternion currentArmsRotation = fpsArms.transform.rotation;
            transform.Rotate(Vector3.up * lookVector.x * horizontalSensitivity * Time.deltaTime);
            fpsArms.transform.rotation = Quaternion.Lerp(currentArmsRotation, cam.transform.rotation, Time.deltaTime * armsSmoothSpeed);

            camXAngle = Mathf.Clamp(camXAngle + lookVector.y * -verticalSensitivity * Time.deltaTime, -90, 90);
            cam.transform.localEulerAngles = Vector3.right * camXAngle;

            // Check if grounded
            hit = Physics.Raycast(transform.position + Vector3.down, Vector3.down, .25f);

            if (hit && !lastGroundCheckHit) isJumping = false;
            lastGroundCheckHit = hit;
            #endregion

            #region Inventory/Weapon inputs
            if (Input.GetKeyDown(KeyCode.T)) wi.DropCurrentWeapon();

            if (Input.GetKeyDown(KeyCode.Mouse0)) wi.FireWeapon(cam.transform.position, lookRayHit.transform, lookPoint, lookPointDistance);

            if (Input.GetKeyDown(KeyCode.R)) wi.ReloadWeapon();
            #endregion
        }

        public override void TakeDamage(float amount, float knockback, GameObject hitLimb, Vector3 hitPoint)
        {
            base.TakeDamage(amount, knockback, hitLimb, hitPoint);

            StartCoroutine(FlashHurtVignette(1));
            Tt_ChildCameraController.instance.CameraShake(knockback * .001f, 0.1f);

            IEnumerator FlashHurtVignette(float duration)
            {
                float timer = duration;
                Color color = Color.white;
                hurtVignette.color = color;

                while (timer > 0)
                {
                    color.a = Mathf.Lerp(0, color.a, timer / duration);
                    hurtVignette.color = color;
                    timer -= Time.deltaTime;
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        public override void Death()
        {
            base.Death();
        }

        void OnWeaponFired(Tt_Interactable weapon)
        {
            // Add a feeling of recoil by turning the player and the camera a little bit
            transform.Rotate(Vector3.up * Random.Range(-1f, 1f));
            camXAngle = Mathf.Clamp(camXAngle + Random.Range(-1.5f, .5f), -90, 90);
        }
    }
}