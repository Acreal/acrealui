using System.Collections.Generic;
using DaggerfallWorkshop.Game;
using UnityEngine;

namespace AcrealUI
{
    public static class UIDelegates
    {
        public delegate bool DataSourceDelegate_Bool(GameObject sender);
        public delegate int DataSourceDelegate_Int(GameObject sender);
        public delegate float DataSourceDelegate_Float(GameObject sender);
        public delegate string DataSourceDelegate_String(GameObject sender);

        public delegate void DataSourceDelegate_MinMaxInt(GameObject sender, out int minValue, out int maxValue);

        public delegate bool DataSourceDelegate_AxisInverted(GameObject sender, InputManager.AxisActions axisAction);
        public delegate string DataSourceDelegate_ControlBindingNameString(GameObject sender, string controlBindingEnumAsString);
        public delegate string DataSourceDelegate_ControlBindingValueString(GameObject sender, string controlBindingEnumAsString, bool primaryBinding);

        public delegate UISaveGameData DataSourceDelegate_SaveGameInfo(GameObject sender, string characterName, string saveGameName);

        public delegate List<UIItemData> DataSourceDelegate_ItemDataList(GameObject sender);
    }
}
