using UnityEngine;

[CreateAssetMenu(fileName = "Base_Item", menuName = "Scriptable Objects/Base_Item")]
public class Base_Item : ScriptableObject
{
    [Header("Item Info")]
    public Sprite itemIcon;
    public string itemName;
    [TextArea] public string description;

    [Header("In-Game Model")]
    public GameObject itemPrefab;
}
