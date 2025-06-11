using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILookable
{
    void StartLook(Transform playerTransform);
    void EndLook();
}
