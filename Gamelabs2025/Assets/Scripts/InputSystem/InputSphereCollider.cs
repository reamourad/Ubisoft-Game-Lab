using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputSphereCollider : MonoBehaviour
{
    [SerializeField] private InputActionReference inputAction;
    [SerializeField] private string beforeString = "";
    [SerializeField] private string afterString = "";
    [SerializeField] private Canvas textCanvas;
    
    public UnityEvent<GameObject> onPlayerEnter;
    public UnityEvent onPlayerExit;
    
    

    
    private void Start()
    {
        textCanvas.GetComponentInChildren<TMP_Text>().text = beforeString + " " + InputReader.GetCurrentBindingText(inputAction.action) + " " + afterString;
        textCanvas.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
        if (other.CompareTag("Hider"))
        {
            textCanvas.gameObject.SetActive(true);
            onPlayerEnter.Invoke(other.gameObject);
        }
    }
    
    private void OnTriggerExit(Collider other) 
    {
        if (other.CompareTag("Hider"))
        {
            textCanvas.gameObject.SetActive(false);
            onPlayerExit.Invoke();
        }
    }
    
    private void OnDisable()
    {
        onPlayerExit.Invoke();
    }
}
