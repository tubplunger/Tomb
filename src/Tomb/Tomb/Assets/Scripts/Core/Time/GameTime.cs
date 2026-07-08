using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tomb.Core.Time
{
    [Serializable]
    public struct GameTime
    {
        public int Day;
        public int Hour;
        public int Minute;

        public GameTime(int day, int hour, int minute)
        {
            Day = day;
            Hour = hour;
            Minute = minute;
            Normalize();
        }

        public void AddMinutes(int minutes)
        {
            Minute += minutes;
            Normalize();
        }

        private void Normalize()
        {
            while (Minute >= 60)
            {
                Minute -= 60;
                Hour++;
            }

            while (Hour >= 24)
            {
                Hour -= 24;
                Day++;
            }

            if (Day < 1)
            {
                Day = 1;
            }
        }

        public override string ToString()
        {
            return $"Day {Day}, {Hour:00}:{Minute:00}";
        }
    }
}
