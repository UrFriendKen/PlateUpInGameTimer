using Kitchen.Modules;
using TMPro;
using UnityEngine;

namespace KitchenInGameTimer.Modules
{
    public class InfoLabelElement : LabelElement
    {
        string _title;
        string _value;

        private void UpdateLabel()
        {
            Label.text = $"{_title}: {_value}";
        }

        public InfoLabelElement SetTitle(string title)
        {
            if (Label != null)
            {
                _title = title;
                UpdateLabel();
            }
            return this;
        }

        public InfoLabelElement SetValue(string value)
        {
            if (Label != null)
            {
                _value = value;
                UpdateLabel();
            }
            return this;
        }

        public InfoLabelElement SetTextColor(Color color)
        {
            if (Label != null)
            {
                Label.color = color;
            }
            return this;
        }

        public InfoLabelElement SetAlignment(TextAlignmentOptions alignment)
        {
            if (Label != null)
            {
                Label.alignment = alignment;
            }
            return this;
        }
    }
}
