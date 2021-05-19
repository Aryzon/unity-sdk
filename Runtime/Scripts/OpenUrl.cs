using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenUrl : MonoBehaviour
{
    public string url = "https://www.a-noble-squid.com";

    public void Trigger()
    {
        if(url.Contains("http://") == false && url.Contains("https://") == false)
        {
            url = "http://" + url;
            Debug.LogWarning("The provided url on " + gameObject.name + " did not contain 'http' or 'https' - please provide the correct url format. 'http' was automatically prepended.");
        }
        Application.OpenURL(url);
    }
}
