using UnityEngine;
using System.Collections;

public class InstanceRunner : MonoBehaviour
{
    private static InstanceRunner instance;

    public static InstanceRunner Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("InstanceRunner");
                instance = obj.AddComponent<InstanceRunner>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    public static Coroutine Run(IEnumerator routine)
    {
        return Instance.StartCoroutine(routine);
    }
}