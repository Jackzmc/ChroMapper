﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    [SerializeField]
    Vector3[] presetPositions;

    [SerializeField]
    Vector3[] presetRotations;

    [SerializeField]
    float movementSpeed;

    [SerializeField]
    float mouseSensitivity;

    [SerializeField]
    float sprintMult;

    [SerializeField]
    float sprintMultPerSecond;

    [Header("Debug")]
    [SerializeField] float movMult = 0;
    [SerializeField] float sprintTime = 0;
    [SerializeField] float x;
    [SerializeField] float y;
    [SerializeField] float z;

    private void Start() {
        GoToPreset(1);
    }

    void Update () {
        if (PauseManager.IsPaused) return; //Dont move camera if we are in pause menu
        if (Input.GetMouseButton(1)) {
            SetLockState(true);

            x = Input.GetAxisRaw("Horizontal");
            y = Input.GetAxisRaw("Vertical");
            z = Input.GetAxisRaw("Forward");

            movMult = movementSpeed;
            if (Input.GetKey(KeyCode.LeftShift)) {
                sprintTime += Time.deltaTime;
                movMult = movMult * (sprintMult * Mathf.Pow(sprintMultPerSecond, sprintTime));
            } else {
                sprintTime = 0;
            }

            transform.Translate(Vector3.right * x * movMult * Time.deltaTime);
            transform.position = transform.position + (Vector3.up * y * movMult * Time.deltaTime); //This one is different because we don't want the player to move vertically relatively - this should use global directions
            transform.Translate(Vector3.forward * z * movMult * Time.deltaTime);

            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");

            //We want to force it to never rotate Z
            Vector3 eulerAngles = transform.rotation.eulerAngles;
            float ex = eulerAngles.x;
            ex = (ex > 180) ? ex - 360 : ex;
            eulerAngles.x = Mathf.Clamp(ex + (-my * mouseSensitivity),-89.5f,89.5f); //pepega code to fix pepega camera :)
            eulerAngles.y = eulerAngles.y + (mx * mouseSensitivity);
            eulerAngles.z = 0;
            transform.rotation = Quaternion.Euler(eulerAngles);

        } else {
            SetLockState(false);
        }

        if (Input.GetKeyDown(KeyCode.Keypad0)) GoToPreset(0);
        if (Input.GetKeyDown(KeyCode.Keypad1)) GoToPreset(1);

    }

    private void GoToPreset(int id) {
        if (presetPositions.Length < id && presetRotations.Length < id) {
            transform.position = presetPositions[id];
            transform.rotation = Quaternion.Euler(presetRotations[id]);
        }
    }

    private bool isLocked = false;
    void SetLockState(bool lockMouse) {
        if (lockMouse && !isLocked) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isLocked = true;
        } else if (!lockMouse && isLocked) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isLocked = false;
        }
    }

}
