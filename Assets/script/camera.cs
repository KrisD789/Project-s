using UnityEngine;

public class camera : MonoBehaviour
{
    Camera cam;
    public Transform player;
    public Transform look_at_spot;
    public float distance = 1.5f;
    public float height = 3f;
    public float mouseSensitivity = 150f;

    float yaw = 0;
    float pitch = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime; 
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime; 
        
        pitch = Mathf.Clamp(pitch, -30f, 60f);
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0); 
        Vector3 offset = rotation * new Vector3(0, height, -distance);

        Vector3 CamPos = player.position + offset;
        transform.position = CamPos;
        player.rotation = Quaternion.Euler(0, yaw, 0); ;

        transform.LookAt(look_at_spot);
    }
}
