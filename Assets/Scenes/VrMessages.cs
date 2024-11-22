using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;
using Byn.Unity.Examples;
using System;

public class VrMessages : MonoBehaviour
{
    private InputDevice leftController;
    private InputDevice rightController;

    private ChatApp _chatAppobj;
    public string deviceMessage = "";
    public string[] servoInfo = new string[14];
    public ServoJoint[] _servoJoints;
    private void Awake()
    {
        _chatAppobj = FindObjectOfType<ChatApp>();
    }
    void Start()
    {
        InitMessage();
    }

  
    private void InitMessage()
    {
        Array.Sort(_servoJoints, (a, b) => a.MotorId.CompareTo(b.MotorId));

        for (int i = 0; i < _servoJoints.Length; i++)
        {
            servoInfo[i] = _servoJoints[i].MotorId.ToString();
        }
    }
    public void createMessage(string id, string signalValue) //update the sorvo motor info after singal is received
    {
        for (int i = 0; i < servoInfo.Length; i++)
        {
            if (id == servoInfo[i])
            {
                servoInfo[i] = _servoJoints[i].MotorId.ToString() + " " + _servoJoints[i].CurrentSignal.ToString();
            }
        }
    }

    public void SendMsgToRobot() // Call this function to send message
    {
        deviceMessage = string.Join(",", servoInfo); // join all the info
        Debug.Log(deviceMessage);
        _chatAppobj.SendButtonPressed(deviceMessage); //send message
    }
}
