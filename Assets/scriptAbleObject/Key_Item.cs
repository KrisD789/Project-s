using UnityEngine;

[CreateAssetMenu(fileName = "Key_Item", menuName = "Scriptable Objects/Key_Item")]
public class Key_Item : Base_Item
{
    [Header("Key Settings")]
    public string doorID;
}
