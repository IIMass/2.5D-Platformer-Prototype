using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerController : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private Animator animator;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float gravity;
    [SerializeField] private float maxDownForce;

    private Vector3 moveDir = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        MoveController();
    }

    private void MoveController()
    {
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
}