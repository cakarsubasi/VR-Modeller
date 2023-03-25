using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float movementSpeed = 15f;
    public float rotationSpeed = 500;

    void Update()
    {
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        transform.Translate(movement * movementSpeed * Time.deltaTime);

        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            transform.RotateAround(transform.position, Vector3.up, mouseX * rotationSpeed * Time.deltaTime);
            transform.RotateAround(transform.position, transform.right, -mouseY * rotationSpeed * Time.deltaTime);
        }
    }
}
