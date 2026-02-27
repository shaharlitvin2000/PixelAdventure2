using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    public Image[] tabImages;
    public GameObject[] pages;

    void Start()
    {
        ActivateTab(0);
    }

    public void ActivateTab(int tabNo)
    {
        if (tabNo < 0 || tabNo >= pages.Length || tabNo >= tabImages.Length)
            return;

        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
            tabImages[i].color = Color.gray;
        }

        pages[tabNo].SetActive(true);
        tabImages[tabNo].color = Color.white;
    }
}