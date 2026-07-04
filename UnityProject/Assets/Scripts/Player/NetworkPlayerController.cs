using System;
using UnityEngine;
using PuzzlePals.Core;
// using Fusion;

namespace PuzzlePals.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class NetworkPlayerController : MonoBehaviour // In production: public class NetworkPlayerController : NetworkBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Interaction Settings")]
        [SerializeField] private Transform carryPoint;
        [SerializeField] private float throwForce = 12f;
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private LayerMask interactableLayer;

        // Synchronized network variables
        // In production: [Networked, OnChangedRender(nameof(OnEmoteChanged))]
        [Header("Networked Emote State")]
        [SerializeField] private string activeEmoteId = "";
        public string ActiveEmoteId => activeEmoteId;

        // In production: [Networked]
        [SerializeField] private bool isCarrying = false;
        public bool IsCarrying => isCarrying;

        private Rigidbody rb;
        private Vector2 movementInput;
        private bool jumpRequest = false;
        private GameObject carriedObject = null;
        private bool isGrounded = true;

        public event Action<string> OnEmoteTriggered;
        public event Action OnCarryStateChanged;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            // Prevent tipping over
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        private void Update()
        {
            // Only read inputs for the local player who has input authority
            // In production: if (!Object.HasInputAuthority) return;

            ReadInputs();
        }

        private void FixedUpdate()
        {
            // In production: if (!Object.HasStateAuthority) return;

            CheckGrounded();
            MoveCharacter();
            ProcessJump();
        }

        private void ReadInputs()
        {
            // Reading from Virtual Joystick or Touch UI
            movementInput.x = Input.GetAxisRaw("Horizontal");
            movementInput.y = Input.GetAxisRaw("Vertical");

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                jumpRequest = true;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                InteractOrThrow();
            }
        }

        private void CheckGrounded()
        {
            // Cast a sphere downwards to check for ground
            float sphereRadius = 0.4f;
            Vector3 origin = transform.position + Vector3.up * 0.2f;
            isGrounded = Physics.CheckSphere(origin, sphereRadius, groundLayer);
        }

        private void MoveCharacter()
        {
            Vector3 direction = new Vector3(movementInput.x, 0, movementInput.y).normalized;

            if (direction.magnitude >= 0.1f)
            {
                // Align to camera direction or isometric rotation
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, targetAngle, 0), Time.fixedDeltaTime * 15f);

                Vector3 targetVelocity = direction * moveSpeed;
                targetVelocity.y = rb.linearVelocity.y; // Preserve gravity
                rb.linearVelocity = targetVelocity;
            }
            else
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
        }

        private void ProcessJump()
        {
            if (jumpRequest)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                jumpRequest = false;

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX("jump");
                }
            }
        }

        public void SetInputVector(Vector2 input)
        {
            // Used by mobile UI virtual joystick
            movementInput = input;
        }

        public void TriggerJump()
        {
            if (isGrounded)
            {
                jumpRequest = true;
            }
        }

        public void InteractOrThrow()
        {
            if (isCarrying)
            {
                ThrowCarriedObject();
            }
            else
            {
                TryPickUpObject();
            }
        }

        private void TryPickUpObject()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position + transform.forward * 1f, interactionRange, interactableLayer);
            
            foreach (var col in colliders)
            {
                // Verify if it is pickable (like a puzzle box or another player)
                if (col.CompareTag("Pickable") || col.CompareTag("Player"))
                {
                    carriedObject = col.gameObject;
                    
                    // Attach to carry point
                    carriedObject.transform.SetParent(carryPoint);
                    carriedObject.transform.localPosition = Vector3.zero;
                    
                    var objRb = carriedObject.GetComponent<Rigidbody>();
                    if (objRb != null)
                    {
                        objRb.isKinematic = true;
                    }

                    isCarrying = true;
                    OnCarryStateChanged?.Invoke();
                    Debug.Log($"[Player] Picked up: {carriedObject.name}");
                    break;
                }
            }
        }

        private void ThrowCarriedObject()
        {
            if (carriedObject == null) return;

            Debug.Log($"[Player] Throwing: {carriedObject.name}");

            GameObject obj = carriedObject;
            carriedObject = null;
            obj.transform.SetParent(null);

            var objRb = obj.GetComponent<Rigidbody>();
            if (objRb != null)
            {
                objRb.isKinematic = false;
                // Add throw force in facing direction + slight upward lift
                Vector3 forceDirection = (transform.forward + Vector3.up * 0.5f).normalized;
                objRb.AddForce(forceDirection * throwForce, ForceMode.Impulse);
            }

            isCarrying = false;
            OnCarryStateChanged?.Invoke();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("throw");
            }
        }

        // Emote system triggered via HUD panel
        // In production: [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void Rpc_TriggerEmote(string emoteId)
        {
            activeEmoteId = emoteId;
            Debug.Log($"[Player] Triggered emote: {activeEmoteId}");
            OnEmoteTriggered?.Invoke(activeEmoteId);

            // Emote disappears after 3 seconds
            CancelInvoke(nameof(ClearEmote));
            Invoke(nameof(ClearEmote), 3.0f);
        }

        private void ClearEmote()
        {
            activeEmoteId = "";
            OnEmoteTriggered?.Invoke("");
        }

        private static void OnEmoteChanged(object changedObject)
        {
            // Photon callback for remote client rendering
            var player = changedObject as NetworkPlayerController;
            player?.OnEmoteTriggered?.Invoke(player.activeEmoteId);
        }
    }
}
