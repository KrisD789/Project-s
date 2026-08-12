using System.Linq.Expressions;
using UnityEngine;

public class MissionTrigger : MonoBehaviour
{
    public MissionData Mission_Data;
    public bool OnInteract = false; //เช็กว่าตอนนี้ผู้เล่นกำลังกดทำเควสอยู่รึป่าว
    public float Max_timer = 10f;
    public float timer = 0;

    private void Update()
    {
        if (Mission_Data.type == MissionType.Hack && !Mission_Data.isCompleted)
        {
            if (OnInteract)
            {
                OnHackQuest();
            }

            else
            {
                timer = 0;
            }
            
        }
    }

    

    private void OnTriggerStay(Collider other)
    {
        switch (Mission_Data.type)
        {
            case MissionType.ReachLocation:

                if (other.CompareTag("Player"))
                {
                    MissionManager.Instance.OnMissionEventReceived(Mission_Data);
                    gameObject.SetActive(false);
                }
                break;

            case MissionType.CaptureTarget:
                if (other.CompareTag("ScamCommander"))
                {
                    MissionManager.Instance.OnMissionEventReceived(Mission_Data);
                }
                break;

            case MissionType.Extraction:

                if (other.CompareTag("Player") && MissionManager.Instance.CanPlayerExit())
                {
                    MissionManager.Instance.OnMissionEventReceived(Mission_Data);
                    gameObject.SetActive(false);
                }
                break;
        }
    }

    public void OnInteractionQuest()
    {
        MissionManager.Instance.OnMissionEventReceived(Mission_Data);
    }

    public void startHackQuest()
    {
        OnInteract = true;
        print("startHackQuest()");
    }

    public void OnHackQuest()
    {
        timer += Time.deltaTime;
        print("Hacking" + timer);
        if (timer >= Max_timer)
        {
            MissionManager.Instance.OnMissionEventReceived(Mission_Data);
            timer = 0;
            cancel_HackQuest();
        }
    }

    public void cancel_HackQuest()
    {
        OnInteract = false ;
    }
}
