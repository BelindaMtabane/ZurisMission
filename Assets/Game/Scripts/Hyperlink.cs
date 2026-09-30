using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Hyperlink : MonoBehaviour
{
    public void Openlink(string link)
    {
        Application.OpenURL(link);
    }
}
