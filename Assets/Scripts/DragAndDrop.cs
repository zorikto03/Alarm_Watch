using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public enum ObjectType
{
    Hour,
    Minute,
    Second,
    Alarm
}

public class DragAndDrop : MonoBehaviour
{
    [SerializeField] Transform Center;
    [SerializeField] TextMeshProUGUI AlarmText;
    [SerializeField] ObjectType Type;
    [SerializeField] GameObject child;


    float _radius = 5.6f;
    float _lastX;
    int _fullRotate = 0;

    int convertedHours;
    int convertedMinutes;
    int convertedSeconds;

    Color _color;
    Renderer _renderer;

    public delegate void ChangedSetTime(int? hours, int? minutes, int? seconds);
    public static ChangedSetTime ChangedSetTimeEvent;

    private void Awake()
    {
        _renderer = child.GetComponent<Renderer>();
        _color = _renderer.material.color;
    }

    private void OnEnable()
    {
        switch (Type)
        {
            case ObjectType.Hour:

            break;
        }
    }

    private void OnDisable()
    {
        switch (Type)
        {
            case ObjectType.Hour:

            break;
        }
    }


    private void OnMouseDown()
    {
        if (Watch.GetAlarm)
        {
            _renderer.material.color = Color.yellow;
        }
    }

    private void OnMouseDrag()
    {
        var position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        if (Watch.GetAlarm)
        {
            switch (Type)
            {
                case ObjectType.Alarm:
                    MoveObject(position);
                    RotateObject(position);
                    break;
                default:
                    RotateObject(position);
                    break;
            }
        }
    }

    private void OnMouseUp()
    {
        if (Watch.GetAlarm)
        {
            _renderer.material.color = _color;
        }
    }

    void MoveObject(Vector3 input)
    {
        float distanceToCenter = Mathf.Sqrt(Mathf.Pow(input.x, 2) + Mathf.Pow(input.z, 2));
        var delta = _radius / distanceToCenter;

        var newX = delta * input.x;
        var newZ = delta * input.z;

        transform.position = new Vector3(newX, 0, newZ);
    }

    void RotateObject(Vector3 input)
    {
        var forward = new Vector3(0f, 0f, 5f);
        
        var cosAngle = 
            (forward.x * input.x) + (forward.z * input.z) 
            / 
            (Mathf.Sqrt(Mathf.Pow(forward.x, 2) + Mathf.Pow(forward.z, 2)) * Mathf.Sqrt(Mathf.Pow(input.x, 2) + Mathf.Pow(input.z, 2)));

        var angle = Mathf.Acos(cosAngle) / (Mathf.PI / 180f); 
                
        if (input.x >= 0 && _lastX < 0 && input.z > 0)
        {
            _fullRotate = _fullRotate switch
            {
                0 => 360,
                360 => 0,
            };
        }

        if (input.x < 0 && _lastX >= 0 && input.z > 0)
        {
            _fullRotate = _fullRotate switch
            {
                0 => 360,
                360 => 0,
            };
        }

        if (input.x < 0)
        {
            angle = 360f - angle;
        }

        string typeTime = string.Empty;

        switch (Type)
        {
            case ObjectType.Alarm:
                angle += _fullRotate;
                convertedHours = ConvertAngleToHours(angle);
                convertedMinutes = ConvertAlarmAngleToMinutes(angle);

                typeTime = "; hour: " + convertedHours + "; minutes: " + convertedMinutes;

                ChangedSetTimeEvent?.Invoke(convertedHours, convertedMinutes, null);
                break;
            case ObjectType.Hour:
                angle += _fullRotate;
                convertedHours = ConvertAngleToHours(angle);
                convertedMinutes = ConvertAngleToMinutesByHourAngle(angle);

                typeTime = "; hour: " + convertedHours + "; minutes: " + convertedMinutes;

                ChangedSetTimeEvent?.Invoke(convertedHours, convertedMinutes, null);
                break;
            case ObjectType.Minute:
                convertedMinutes = ConvertAngleToMinutes(angle);

                typeTime = "; minutes: " + convertedMinutes;

                ChangedSetTimeEvent?.Invoke(null, convertedMinutes, null);
                break;
            case ObjectType.Second:
                convertedSeconds = ConvertAngleToSeconds(angle);

                typeTime = "; seconds: " + convertedSeconds;

                ChangedSetTimeEvent?.Invoke(null, null, convertedSeconds);
                break;
        }

        transform.localRotation = Quaternion.Euler(0f, angle, 0f);
        _lastX = input.x;
        //Debug.Log("x: " + input.x + "; z: " + input.z + "; cos Angle: " + cosAngle + "; angle: " + angle + typeTime);
    }

    int ConvertAngleToHours(float angle) => (int)angle / 30;
    int ConvertAngleToMinutes(float angle) => (int)angle / 6;
    int ConvertAngleToSeconds(float angle) => (int)angle / 6;
    int ConvertAlarmAngleToMinutes(float angle)
    {
        var counter = (int)angle / 30;
        float delta = angle - counter * 30;
        return (int)(delta * 2);
    }
    int ConvertAngleToMinutesByHourAngle(float angle)
    {
        var counter = (int)angle / 30;
        float delta = angle - counter * 30;
        return (int)(delta * 2);
    }

    public void SetPositioinByTime(TimeSpan time)
    {
        var angleMinutes = time.Hours * 30 + time.Minutes / 2;
        
        var hourAcos = angleMinutes * (Mathf.PI / 180f);
        var hourCos = Mathf.Cos(hourAcos);

        Debug.Log("hourAcos: " + hourAcos + "; hourCos: " + hourCos + "; angle: " + hourCos);

        var z = hourCos;
        var x = Mathf.Sqrt(1f - Mathf.Pow(z, 2));
        x = (time.Hours >= 6 && time.Hours <= 11 || time.Hours >= 18 && time.Hours <= 23) ? -x : x;

        var pos = new Vector3(x, 0, z);
        MoveObject(pos);
    }
}