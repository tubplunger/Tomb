using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Text;
using Tomb.Core.Events;

namespace Tomb.Core.Debugging.Timeline
{
    public static class EventTimelineFormatter
    {
        public static string Format(IGameEvent gameEvent)
        {
            if (gameEvent == null)
                return string.Empty;

            Type eventType = gameEvent.GetType();
            FieldInfo[] fields = eventType.GetFields(
                BindingFlags.Instance |
                BindingFlags.Public
            );

            if (fields.Length == 0)
                return string.Empty;

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                object value = field.GetValue(gameEvent);

                if (i > 0)
                    builder.Append(" | ");

                builder.Append(field.Name);
                builder.Append(": ");
                builder.Append(value ?? "null");
            }

            return builder.ToString();
        }
    }
}