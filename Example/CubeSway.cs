using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CubeSway : MonoBehaviour
{
    float RotationAmount = 6f, RotationSpeedX = 2.5f, RotationSpeedY = 1.75f;
    GameObject cube;

    private void Start()
    {
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = transform.position + new Vector3(0, 0, 3);
    }

    void FixedUpdate()
    {
        cube.transform.Rotate(new Vector3(1, 1, 1));

        transform.localRotation = Quaternion.Euler(new Vector3(Mathf.Cos(Time.time * RotationSpeedX), Mathf.Sin(Time.time * RotationSpeedY)) * RotationAmount);
    }
}
