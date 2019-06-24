﻿using UnityEngine;
using WebRTC;
using WebSocketSharp;
using UniRx;
using System;
using Utf8Json;
public class Connect : MonoBehaviour
{
    WebSocket ws;
    Signaling signaling;
    public string roomId = "test2";

    void Start()
    {

#if UNITY_ANDROID
        AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaClass utilityClass = new AndroidJavaClass("org.webrtc.UnityUtility");
        utilityClass.CallStatic("InitializePeerConncectionFactory", new object[1] { activity });
#endif

        Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe(_ => Join());

        Debug.Log("start");
        ws = new WebSocket("ws://192.168.0.5:8080");

        ws.OnMessage += (_, e) => OnMessage(e.Data);

        ws.Connect();

        signaling = new Signaling(roomId);
        signaling.OnConnectMethod += OnConnet;
        signaling.OnDataMethod += OnData;
        signaling.OnSdpMethod += OnSdp;
    }

    void OnConnet(string str)
    {
        Debug.Log("connect");
        signaling.peer.SendDataViaDataChannel("test from unity");
    }

    public void Send(string str)
    {
        signaling.peer.SendDataViaDataChannel(str);
    }

    void OnData(string s)
    {
        Debug.Log("data " + s);
    }

    void OnSdp(string s)
    {
        Debug.Log("sendsdp " + s);
        ws.Send(s);
    }

    class Action
    {
        public string type;
        public string payload;
    }

    public void Join()
    {
        Debug.Log("join");
        var data = new Action();
        data.type = "join";
        data.payload = "join";
        var json = JsonUtility.ToJson(data);
        ws.Send(json);
    }


    class OnMessageS
    {
        public string type;
        public string payload;
    }
    void OnMessage(string s)
    {
        Debug.Log("onmessage " + s);
        var data = JsonUtility.FromJson<OnMessageS>(s);
        Debug.Log(data.type);
        switch (data.type)
        {
            case "offer":
                signaling.SetSdp(data.payload);
                break;
        }
    }

}