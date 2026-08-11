using System.Collections;   
using System.Collections.Generic;
using UnityEngine;


public class movement : MonoBehaviour
{
    private CharacterController controller;
    public float speed = 5.0f;
    public float turnSpeed = 180.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movdir;
        transform.Rotate(0, Input.GetAxis("Horizontal") * turnSpeed * Time.deltaTime, 0);
        movdir = transform.forward * Input.GetAxis("Vertical") * speed;
        controller.Move(movdir * Time.deltaTime - Vector3.up * 0.1f);

    }
}
