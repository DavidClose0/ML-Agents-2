using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquidFishMover : MonoBehaviour {

    public event EventHandler OnReachedTargetPosition;
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;

    public class OnStateChangedEventArgs : EventArgs {
        public State state;
    }


    public enum State {
        Idle,
        Moving,
        Looking,
    }

    [SerializeField] private float moveSpeed = 6f;

    private State state;
    private Rigidbody moverRigidbody;
    [SerializeField] private Vector3 targetPosition;
    private Vector3 lookAtPosition;

    private void Awake() {
        moverRigidbody = GetComponent<Rigidbody>();

        SetTargetPosition(transform.position);
        SetLookAtPosition(transform.position + transform.forward);
    }

    private void Update() {
        switch (state) {
            case State.Idle:
                moverRigidbody.linearVelocity = Vector3.zero;
                moverRigidbody.angularVelocity = Vector3.zero;
                break;
            case State.Moving:
                if (Vector3.Distance(transform.position, targetPosition) > .1f) {
                    // Move closer
                    Vector3 moveDir = (targetPosition - transform.position).normalized;
                    moverRigidbody.linearVelocity = moveDir * moveSpeed;
                } else {
                    // Close enough
                    transform.position = targetPosition;
                    moverRigidbody.linearVelocity = Vector3.zero;
                    state = State.Looking;
                    OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });
                }
                break;
            case State.Looking:
                moverRigidbody.linearVelocity = Vector3.zero;
                break;
        }
    }

    private void FixedUpdate() {
        switch (state) {
            case State.Moving:
                Vector3 lookDir = (targetPosition - transform.position).normalized;
                transform.forward = Vector3.Lerp(transform.forward, lookDir, Time.fixedDeltaTime * 5f);
                break;
            case State.Looking:
                lookDir = (lookAtPosition - transform.position).normalized;
                transform.forward = Vector3.Lerp(transform.forward, lookDir, Time.fixedDeltaTime * 5f);

                if (Vector3.Dot(transform.forward, lookDir) > .999f) {
                    // Looking at target
                    state = State.Idle;
                    OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });

                    OnReachedTargetPosition?.Invoke(this, EventArgs.Empty);
                }
                break;
        }
    }

    public void SetTargetPosition(Vector3 targetPosition) {
        this.targetPosition = targetPosition;
        state = State.Moving;
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });
    }

    public void SetLookAtPosition(Vector3 lookAtPosition) {
        this.lookAtPosition = lookAtPosition;
        state = State.Moving;
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });
    }

}
