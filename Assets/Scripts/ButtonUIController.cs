using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonUIController : MonoBehaviour
{
    public int type;
    [SerializeField] private GameObject obj;
    private ProgressBar bar;

    // Start is called before the first frame update
    void Start()
    {
        bar = obj.GetComponent<ProgressBar>();
        bar.minimum = 0.0f;
        bar.maximum = 1.0f;
        bar.current = GameManager.instance.GetSoonestToMatureCoralPercent(type); // maybe crappy dependency but whatever
    }

    // Update is called once per frame
    void Update()
    {
        bar.current = GameManager.instance.GetSoonestToMatureCoralPercent(type);
    }
}
