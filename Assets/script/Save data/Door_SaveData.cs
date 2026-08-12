using UnityEngine; // ลบ UnityEditor ที่เกินมาออกให้แล้วครับ

// เพิ่มป้ายนี้เพื่อให้ JsonUtility สามารถมองเห็นและแพ็คข้อมูลได้
[System.Serializable]
public struct MiniDataBox
{
    public bool savedBool;
    public int Doorstate;
}

public class Door_SaveData : MonoBehaviour, Isaveable
{
    SaveableEntity SaveableEntity;
    Door Door_Script;

    private void Awake()
    {
        if (!TryGetComponent<Door>(out Door_Script)) Debug.Log("!!!!!!!! Door_SaveData หา Door_Script ไม่เจอ !!!!!!");

        if (!TryGetComponent<SaveableEntity>(out SaveableEntity)) Debug.Log("!!!!!!!! Door_SaveData หา SaveableEntity ไม่เจอ !!!!!!");
    }

    public string GetSaveID()
    {
        return SaveableEntity.uniqueID;
    }

    public void LoadState(string stateData)
    {
        MiniDataBox MinidataBox = JsonUtility.FromJson<MiniDataBox>(stateData);

        if (Door_Script != null)
        {
            Door_Script.currentState = (Door.DoorState)MinidataBox.Doorstate;
            Door_Script.openByAi = MinidataBox.savedBool;
        }
    }

    public string SaveState()
    {
        MiniDataBox MinidataBox = new MiniDataBox();
        MinidataBox.savedBool = Door_Script.openByAi;
        MinidataBox.Doorstate = (int)Door_Script.currentState;

        return JsonUtility.ToJson(MinidataBox);
    }
}