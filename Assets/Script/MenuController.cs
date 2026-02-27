using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{

    public GameObject MenuCanvas;
    // Start is called before the first frame update
    void Start()
    {
        MenuCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            MenuCanvas.SetActive(!MenuCanvas.activeSelf);

        }
    }
}
