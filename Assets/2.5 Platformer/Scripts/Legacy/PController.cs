using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer.Legacy
{
    public class PController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CharacterController controller;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform ledgeChecker;

        [Header("Movement")]
        [SerializeField] private float moveSpeed;
        [SerializeField] private float jumpForce;
        [SerializeField] private float gravity;
        [SerializeField] private float maxDownForce;

        [Header("States")]
        [SerializeField] private bool onLedge;
        [SerializeField] private bool climbing;

        [Header("Ledge Climb")]
        [SerializeField] private Vector2 climbOffset;

        private Vector3 moveDir = Vector3.zero;

        // Update is called once per frame
        void Update()
        {
            MoveController();
            LedgeStartClimb();
        }

        private void MoveController()
        {
            if (onLedge) return;

            if (controller.isGrounded)
            {
                moveDir.z = Input.GetAxisRaw("Horizontal") * moveSpeed;
            }

            moveDir.y = Mathf.Clamp(moveDir.y - (gravity * Time.deltaTime), -maxDownForce, float.MaxValue);

            if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
            {
                moveDir.y = jumpForce;
                animator.SetTrigger("Jump");
            }

            controller.Move(moveDir * Time.deltaTime);

            if (moveDir.z > 0f)
            {
                animator.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else if (moveDir.z < 0f)
            {
                animator.transform.rotation = Quaternion.Euler(0f, -180f, 0f);
            }


            animator.SetFloat("Speed", controller.velocity.magnitude);
            animator.SetBool("Grounded", controller.isGrounded);
        }


        private void LedgeStartClimb()
        {
            // If the player presses Space, and is on a ledge and not currently climbing it...
            if (Input.GetKeyDown(KeyCode.Space) && onLedge && !climbing)
            {
                animator.SetTrigger("Climb");   // Trigger the Climb animation
                climbing = true;                // This state avoids triggering again the Climb animation
            }
        }

        public void LedgeGrab()
        {
            onLedge = true;                     // This state disables the character movement
            moveDir = Vector3.zero;             // Resets the controller's momentum
            animator.SetTrigger("OnLedge");     // Triggers the Ledge Grab animation
            animator.SetFloat("Speed", 0f);     // Resets the movement speed animator value

            // Sets the position of the character controller near the ledge
            transform.position = new Vector3(transform.position.x, transform.position.y, ledgeChecker.position.z);
        }


        public void LedgeClimb()
        {
            // Moves the character controller to an approximate position.
            // Z offset is applied differently taking into account the direction the controller is facing
            transform.position = new Vector3
                (transform.position.x,
                transform.position.y + climbOffset.y,
                transform.position.z + (Mathf.Sign(animator.transform.eulerAngles.y) * climbOffset.x));

            onLedge = false;
            climbing = false;
        }
    }
}