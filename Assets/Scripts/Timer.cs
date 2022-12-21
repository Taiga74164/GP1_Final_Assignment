using System;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    private float _timer = 0;

    void Update()
    {
        _timer += Time.deltaTime;
        TimeSpan time = TimeSpan.FromSeconds(Mathf.Round(_timer));
        var str = time.ToString(@"mm\:ss");
        this.GetComponent<TextMeshProUGUI>().text = string.Format($"<color=green>{str}</color>");
    }
}
