using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PuzzlePals.Core;
// using Fusion;

namespace PuzzlePals.Level
{
    public enum PuzzleElementType
    {
        PressurePlate,
        Button,
        Lever,
        DualLeverLock,
        SlidingDoor,
        MovingPlatform
    }

    public class PuzzleElement : MonoBehaviour // In production: public class PuzzleElement : NetworkBehaviour
    {
        [Header("Puzzle Setup")]
        [SerializeField] private string puzzleId;
        [SerializeField] private PuzzleElementType elementType;
        [SerializeField] private List<PuzzleElement> triggerSources; // Triggers that control this door/platform
        [SerializeField] private bool requiresAllTriggers = true;

        [Header("Movement Settings (For Platforms/Doors)")]
        [SerializeField] private Vector3 activeOffset;
        [SerializeField] private float moveSpeed = 3f;

        // In production: [Networked, OnChangedRender(nameof(OnStateChanged))]
        [Header("Synced State")]
        [SerializeField] private bool isActivated = false;
        public bool IsActivated => isActivated;

        private Vector3 startPosition;
        private Vector3 targetPosition;
        private int activatedTriggerCount = 0;

        // Keep track of dual-lever timestamps
        private static Dictionary<string, float> leverPullTimestamps = new Dictionary<string, float>();
        private const float CoOpTimeWindow = 1.0f; // 1 second window to pull both levers

        private void Start()
        {
            startPosition = transform.position;
            targetPosition = startPosition;

            if (triggerSources != null)
            {
                foreach (var source in triggerSources)
                {
                    source.OnElementToggled += HandleTriggerSourceToggled;
                }
            }
        }

        private void OnDestroy()
        {
            if (triggerSources != null)
            {
                foreach (var source in triggerSources)
                {
                    if (source != null)
                        source.OnElementToggled -= HandleTriggerSourceToggled;
                }
            }
        }

        public event System.Action<bool> OnElementToggled;

        // In production: [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void Rpc_ToggleState(bool state, string playerId = "")
        {
            // Set state on Authority, which automatically replicates to all clients
            if (elementType == PuzzleElementType.DualLeverLock)
            {
                ProcessDualLeverInteraction(playerId);
            }
            else
            {
                SetState(state);
            }
        }

        private void SetState(bool state)
        {
            if (isActivated == state) return;

            isActivated = state;
            Debug.Log($"[PuzzleElement] ID {puzzleId} ({elementType}) changed state to: {isActivated}");

            if (AudioManager.Instance != null && isActivated)
            {
                AudioManager.Instance.PlaySFX("puzzle_activate");
            }

            OnElementToggled?.Invoke(isActivated);

            // Trigger animations or displacements
            if (elementType == PuzzleElementType.SlidingDoor || elementType == PuzzleElementType.MovingPlatform)
            {
                targetPosition = isActivated ? startPosition + activeOffset : startPosition;
            }
        }

        private void Update()
        {
            // Smoothly move sliding doors and platforms
            if (elementType == PuzzleElementType.SlidingDoor || elementType == PuzzleElementType.MovingPlatform)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
            }
        }

        private void HandleTriggerSourceToggled(bool sourceState)
        {
            // Evaluate condition
            activatedTriggerCount = 0;
            foreach (var source in triggerSources)
            {
                if (source.IsActivated)
                    activatedTriggerCount++;
            }

            bool shouldActivate;
            if (requiresAllTriggers)
            {
                shouldActivate = (activatedTriggerCount == triggerSources.Count);
            }
            else
            {
                shouldActivate = (activatedTriggerCount > 0);
            }

            SetState(shouldActivate);
        }

        private void ProcessDualLeverInteraction(string playerId)
        {
            if (string.IsNullOrEmpty(playerId)) return;

            float now = Time.time;
            leverPullTimestamps[playerId] = now;

            // Check if another player pulled a lever within the allowed time window
            bool coOpSuccess = false;
            string otherPlayerId = "";

            foreach (var pair in leverPullTimestamps)
            {
                if (pair.Key != playerId && Mathf.Abs(now - pair.Value) <= CoOpTimeWindow)
                {
                    coOpSuccess = true;
                    otherPlayerId = pair.Key;
                    break;
                }
            }

            if (coOpSuccess)
            {
                Debug.Log($"[PuzzleElement] Co-Op Lever Lock opened by {playerId} and {otherPlayerId} within {CoOpTimeWindow}s!");
                SetState(true);
                // Clear timestamps
                leverPullTimestamps.Clear();
            }
            else
            {
                Debug.Log($"[PuzzleElement] Lever pulled by {playerId}. Waiting for partner...");
                SetState(false);
                
                // Reset lever back after timeout
                StartCoroutine(ResetLeverAfterTimeout(playerId));
            }
        }

        private IEnumerator ResetLeverAfterTimeout(string playerId)
        {
            yield return new WaitForSeconds(CoOpTimeWindow);
            if (!isActivated)
            {
                Debug.Log($"[PuzzleElement] Lever pull by {playerId} timed out.");
                leverPullTimestamps.Remove(playerId);
            }
        }

        // Collision logic for pressure plates
        private void OnTriggerEnter(Collider other)
        {
            // Check if player or pushable box enters pressure plate
            if (elementType == PuzzleElementType.PressurePlate && (other.CompareTag("Player") || other.CompareTag("Pushable")))
            {
                Rpc_ToggleState(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (elementType == PuzzleElementType.PressurePlate && (other.CompareTag("Player") || other.CompareTag("Pushable")))
            {
                Rpc_ToggleState(false);
            }
        }
    }
}
