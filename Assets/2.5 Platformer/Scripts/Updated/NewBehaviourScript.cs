using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

[ExecuteInEditMode]
public class NewBehaviourScript : MonoBehaviour
{
    [ContextMenu("Replace _")]
    void ReplaceUnderscore()
    {
        transform.name = transform.name.Replace('_', ':');
        Debug.Log(transform.name);

        List<Transform> o = new List<Transform>(transform.GetComponentsInChildren<Transform>());
        Debug.Log(o.Count);

        for (int i = 0; i < o.Count; i++)
        {
            o[i].name = o[i].name.Replace('_', ':');
        }
    }
}
