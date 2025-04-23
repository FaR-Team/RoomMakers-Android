using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TagButton : MonoBehaviour, ISelectHandler
{
    [SerializeField] private RoomTag tag;
    [SerializeField] private TagButton selectOnUp;
    [SerializeField] private TagButton selectOnDown;
    [SerializeField] private TagButton selectOnLeft;
    [SerializeField] private TagButton selectOnRight;
    
    private Button button;
    
    
    public RoomTag Tag => this.tag;

    private void Awake()
    {
        button = GetComponent<Button>();
    }
    public void OnSelect(BaseEventData eventData)
    {
        TagSelectionUI.instance.SelectTag(tag, this);
    }

    public void Select()
    {
        button.Select();
    }

    public void SelectNext(Vector2 dir)
    {
        if(dir.x < 0) selectOnLeft.Select();
        else if (dir.x > 0) selectOnRight.Select();
        else if(dir.y > 0) selectOnUp.Select();
        else if(dir.y < 0) selectOnDown.Select();
    }
}
