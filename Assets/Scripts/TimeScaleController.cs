// SerialID: [1ba2ce2c-2b2a-4e6d-9764-8ce1f38e28f0]
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TimeScaleController : MonoBehaviour
{
    public void OnTimeScaleChanged(float timeScale)
    {
        Time.timeScale = timeScale;
    }
}
