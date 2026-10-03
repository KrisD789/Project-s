using UnityEngine;

public class BillboadUI : MonoBehaviour
{
    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCam != null)
        {
            // หันหน้าสู้กล้องตามทิศทางระนาบของกล้องผู้เล่นเสมอ
            transform.LookAt(transform.position + mainCam.transform.forward);
        }
    }
}
