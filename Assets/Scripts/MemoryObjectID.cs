using UnityEngine;

public class MemoryObjectID : MonoBehaviour
{
    public string objectID;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(objectID))
        {
            objectID = gameObject.name.Replace("(Clone)", "").Trim();
        }
    }

    private void Awake()
    {
        if (string.IsNullOrEmpty(objectID))
        {
            objectID = gameObject.name.Replace("(Clone)", "").Trim();
        }
    }
}