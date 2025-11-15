using System;
using UnityEngine;

namespace FreelanceOdyssey.Core
{
    public class TimeService : MonoBehaviour
    {
        public DateTime Now => DateTime.UtcNow;

        public float CalculateOfflineMinutes(DateTime lastPlayedUtc, DateTime nowUtc, float maxHours)
        {
            var delta = nowUtc - lastPlayedUtc;
            if (delta.TotalMinutes < 0)
            {
                return 0f;
            }

            var minutes = (float)Math.Min(delta.TotalMinutes, maxHours * 60f);
            return Mathf.Max(0f, minutes);
        }
    }
}
