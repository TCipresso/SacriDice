using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public Transform cam;
    public float mouseSensitivity = 130f;
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.6f;
    public float jumpHeight = 1.2f;
    public float gravity = -30f;

    CharacterController cc;
    float xRot;
    Vector3 velocity;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        xRot -= my;
        xRot = Mathf.Clamp(xRot, -89f, 89f);
        if (cam) cam.localRotation = Quaternion.Euler(xRot, 0f, 0f);
        transform.Rotate(Vector3.up * mx);

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        bool sprint = Input.GetKey(KeyCode.LeftShift);
        float speed = moveSpeed * (sprint ? sprintMultiplier : 1f);
        Vector3 move = transform.right * input.x + transform.forward * input.y;
        cc.Move(move * speed * Time.deltaTime);

        if (cc.isGrounded && velocity.y < 0) velocity.y = -2f;
        if (cc.isGrounded && Input.GetButtonDown("Jump")) velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
