using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnClick : MonoBehaviour
{
    private void OnMouseDown()
    {
        gameObject.SetActive(false);
    }
}
