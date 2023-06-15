﻿using System;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class NetworkButtons : MonoBehaviour
{
    public UnityEvent<NetworkButton.Mode> request = new();
    [SerializeField] private NetworkButton[] networkButtons = { };

    private void Start()
    {
        foreach (var networkButton in networkButtons)
        {
            networkButton.request.AddListener(mode => request.Invoke(mode));
        }
    }

    public void Hide()
    {
        foreach (var button in networkButtons)
        {
            button.gameObject.SetActive(false);
        }
    }
}