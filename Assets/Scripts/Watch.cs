using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class Watch : MonoBehaviour
{
    const int HOUR_DEGREES = 30;
    const int MINUTE_DEGREES = 6;
    const int SECOND_DEGREES = 6;

    [SerializeField] GameObject Hour;
    [SerializeField] GameObject Minute;
    [SerializeField] GameObject Second;
    [SerializeField] DragAndDrop AlarmObject;

    [SerializeField] Transform Center;
    [SerializeField] GameObject PointClock;

    [SerializeField] TextMeshProUGUI AlarmText;
    [SerializeField] TextMeshProUGUI TimeText;
    
    [SerializeField] TMP_Dropdown HourDropdown;
    [SerializeField] TMP_Dropdown MinuteDropdown;
    [SerializeField] TMP_Dropdown SecondDropdown;

    [SerializeField] GameObject AlarmImage;

    float _angle = 0;
    float _posX, _posZ;
    DateTime _time;

    public TimeSpan GetTime => alarm == null ? 
        new TimeSpan(0, 0, 0) :
        new TimeSpan(alarm.Value.Hours, alarm.Value.Minutes, alarm.Value.Seconds);

    int _hours = 0;
    int _minutes = 0;
    int _seconds = 0;

    float _radiusPoints = 6.5f;

    AudioSource audio;
    Alarm? alarm = null;

    public static bool GetAlarm => _alarm;
    static bool _alarm; // set off watch and set alarm time

    private void Awake()
    {
        audio = GetComponent<AudioSource>();
    }

    private void Start()
    {
        _time = DateTime.Now;
        
        LoadAlarm();

        GeneratePoints();

        StartCoroutine(SecondTick());
    }

    private void OnEnable()
    {
        DragAndDrop.ChangedSetTimeEvent += SetDataAlarm;
        UIController.AlarmGroupOpenCloseEvent += AlarmGroupOpenCloseEventHandler;
    }

    private void OnDisable()
    {
        DragAndDrop.ChangedSetTimeEvent -= SetDataAlarm;
        UIController.AlarmGroupOpenCloseEvent -= AlarmGroupOpenCloseEventHandler;
    }

    public void UpdateNetworkTime()
    {
        _time = TimeScript.GetNetworkTime(TimeScript.ntpServer1);
    }

    IEnumerator SecondTick()
    {
        while (true)
        {
            if (alarm != null && _time.Hour == alarm.Value.Hours && _time.Minute == alarm.Value.Minutes && alarm.Value.Seconds == _time.Second)
            {
                if (!audio.isPlaying)
                {
                    AlarmStart();
                }
            }

            AlarmImage.SetActive(audio.isPlaying);

            if (!_alarm)
            {
                RotateArrows(_time.Hour, _time.Minute, _time.Second);
            }

            _time = _time.AddSeconds(1);
            TimeText.text = string.Format("{0}:{1}:{2}", _time.Hour, _time.Minute, _time.Second);

            yield return new WaitForSeconds(1f);
        }
    }

    void AlarmStart()
    {
        audio.Play();

        AlarmImage.SetActive(true);

        AlarmImage.transform.DOShakePosition(audio.clip.length, 10, 20).
            OnComplete(() =>
            {
                AlarmImage.SetActive(false);
            });
    }

    void GeneratePoints()
    {
        for(int i = 1; i <= 12; i++)
        {
            var point = Instantiate(PointClock, transform);

            var cos = Mathf.Cos(30f * i * Mathf.PI / 180f);
            var sin = Mathf.Sin(30f * i * Mathf.PI / 180f);
            var x = Center.position.x + cos * _radiusPoints;
            var z = Center.position.z + sin * _radiusPoints;

            point.transform.position = new Vector3(x, 0, z);
        }
    }

    public void SetDataAlarm(int? hours, int? minutes, int? seconds = null)
    {
        if (hours != null) this._hours = (int)hours;
        if (minutes != null) this._minutes = (int)minutes;
        if (seconds != null) this._seconds = (int)seconds;

        SetTimeDropdown();

        RotateArrows(_hours, _minutes, _seconds);
    }

    public void AlarmGroupOpenCloseEventHandler(bool isOpened)
    {
        _alarm = isOpened;
        AlarmText.gameObject.SetActive(!isOpened);

        if (_alarm)
        {
            RotateArrows(_hours, _minutes, _seconds);
        }
        else if (alarm != null)
        {
            AlarmObject.SetPositioinByTime(GetTime);
        }
    }

    public void ResetDataAlarm()
    {
        _hours = 0;
        _minutes = 0;
        _seconds = 0;

        AlarmText.text = "-";
        alarm = null;
        SaveAlarm();

        SetTimeDropdown();

        RotateArrows(_hours, _minutes, _seconds);
        //AlarmObject.SetPositioinByTime(GetTime);
        AlarmObject.gameObject.SetActive(false);
    }

    public void SetAlarmTime()
    {
        _alarm = true;

        alarm = new Alarm()
        {
            Hours = _hours,
            Minutes = _minutes,
            Seconds = _seconds
        };

        SaveAlarm();

        var text = string.Format("{0:t}", new TimeSpan(_hours, _minutes, _seconds));
        AlarmText.text = text;

        AlarmObject.gameObject.SetActive(true);
        AlarmObject.SetPositioinByTime(GetTime);
    }

    public void SetHourValueFromDropdown()
    {
        _hours = HourDropdown.value;
        RotateArrows(_hours, _minutes, _seconds);
    }
    public void SetMinuteValueFromDropdown()
    {
        _minutes = MinuteDropdown.value;
        RotateArrows(_hours, _minutes, _seconds);
    }
    public void SetSecondValueFromDropdown()
    {
        _seconds = SecondDropdown.value;
        RotateArrows(_hours, _minutes, _seconds);
    }

    void RotateArrows(int hours, int minutes, int seconds)
    {
        Hour.transform.localRotation = Quaternion.Euler(0f, hours * HOUR_DEGREES + minutes / 2, 0f);
        Minute.transform.localRotation = Quaternion.Euler(0f, minutes * MINUTE_DEGREES + seconds / 12, 0f);
        Second.transform.localRotation = Quaternion.Euler(0f, seconds * SECOND_DEGREES, 0f);
    }

    void SetTimeDropdown()
    {
        HourDropdown.SetValueWithoutNotify(_hours);
        MinuteDropdown.SetValueWithoutNotify(_minutes);
        SecondDropdown.SetValueWithoutNotify(_seconds);
    }


    void SaveAlarm()
    {
        if (alarm != null)
        {
            PlayerPrefs.SetInt("Hours", alarm.Value.Hours);
            PlayerPrefs.SetInt("Minutes", alarm.Value.Minutes);
            PlayerPrefs.SetInt("Seconds", alarm.Value.Seconds);

            PlayerPrefs.Save();
            Debug.Log("Data is saved!");
        }
        else
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("Data is deleted!");
        }
    }

    void LoadAlarm()
    {
        if (PlayerPrefs.HasKey("Hours"))
        {
            var hours = PlayerPrefs.GetInt("Hours");
            var minutes = PlayerPrefs.GetInt("Minutes");
            var seconds = PlayerPrefs.GetInt("Seconds");

            alarm = new Alarm()
            {
                Hours = hours,
                Minutes = minutes,
                Seconds = seconds
            };

            var text = string.Format("{0:t}", GetTime);
            AlarmText.text = text;

            AlarmObject.gameObject.SetActive(true);
            AlarmObject.SetPositioinByTime(GetTime);

            Debug.Log("Data is loaded!");
        }
        else
        {
            Debug.Log("There is no save data!");
        }
    }
}
