using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] float MinutesToCheck = 60;

    [SerializeField] Watch Watch;
    public static GameManager instance => _instance;
    static GameManager _instance;

    void Start()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        StartCoroutine(CheckTimeCoroutine());
    }

    IEnumerator CheckTimeCoroutine()
    {
        while (true)
        {
            Watch.UpdateNetworkTime();

            yield return new WaitForSeconds(60 * MinutesToCheck);
        }
    }

}
