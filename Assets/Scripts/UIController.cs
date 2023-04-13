using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] GameObject AlarmGroup;
    [SerializeField] GameObject OpenAlarmButton;
    [SerializeField] GameObject SetAlarmTimeButton;
    [SerializeField] GameObject AddAlarmButton;
    [SerializeField] GameObject RemoveAlarmButton;

    public delegate void AlarmGroupOpenClose(bool value);
    public static AlarmGroupOpenClose AlarmGroupOpenCloseEvent;

    public void OpenAlarmGroup()
    {
        AlarmGroup.SetActive(true);
        OpenAlarmButton.SetActive(false);
        AlarmGroupOpenCloseEvent.Invoke(true);
    }

    public void CloseAlarmGroup()
    {
        AlarmGroup.SetActive(false);
        OpenAlarmButton.SetActive(true);
        AlarmGroupOpenCloseEvent.Invoke(false);
    }

    public void ResetDataAlarm()
    {
        AddAlarmButton.SetActive(true);
        RemoveAlarmButton.SetActive(false);
    }

    public void AddAlarm()
    {
        AddAlarmButton.SetActive(false);
        RemoveAlarmButton.SetActive(true);
        SetAlarmTimeButton.SetActive(true);
    }
}
