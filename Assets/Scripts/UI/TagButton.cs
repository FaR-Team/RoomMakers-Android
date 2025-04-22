using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TagButton : MonoBehaviour
{
    [SerializeField] private RoomTag tag;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    void OnClick()
    {
        TagSelectionUI.instance.SelectTag(tag, button);
    }
}
