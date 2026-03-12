/*
Copyright (c) 2025-2026 Acreal (https://github.com/acreal)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
documentation files (the "Software"), to deal in the Software without restriction, including without 
limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of 
the Software, and to permit persons to whom the Software is furnished to do so, subject to the following 
conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions 
of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
DEALINGS IN THE SOFTWARE.
*/

using DaggerfallWorkshop.Game.Utility.ModSupport;
using UnityEngine;

namespace AcrealUI
{
    [ImportedComponent]
    public class UISettingsMenuPanel : UIPanel
    {
        #region Variables
        [SerializeField] private string _gameObjName_button_generalSettings = null;
        [SerializeField] private string _gameObjName_button_videoSettings = null;
        [SerializeField] private string _gameObjName_button_audioSettings = null;
        [SerializeField] private string _gameObjName_button_controlSettings = null;

        private UIButton button_generalSettings = null;
        private UIButton button_videoSettings = null;
        private UIButton button_audioSettings = null;
        private UIButton button_controlSettings = null;
        #endregion


        #region Events
        public System.Action Event_OnButtonClicked_GeneralSettings = null;
        public System.Action Event_OnButtonClicked_VideoSettings = null;
        public System.Action Event_OnButtonClicked_AudioSettings = null;
        public System.Action Event_OnButtonClicked_ControlSettings = null;
        #endregion


        #region Initialization
        public override void Initialize()
        {
            base.Initialize();

            Transform generalTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_generalSettings);
            if (generalTform != null)
            {
                button_generalSettings = generalTform.GetComponent<UIButton>();
                if (button_generalSettings != null)
                {
                    button_generalSettings.Initialize();
                    button_generalSettings.Event_OnAnyClick += (_, _1) => { Event_OnButtonClicked_GeneralSettings?.Invoke(); };
                }
                else { Debug.LogError("[AcrealUI.UISettingsMenuPanel] Failed to load UIButton Script on GameObject \"" + _gameObjName_button_generalSettings + "\""); }
            }

            Transform videoTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_videoSettings);
            if (videoTform != null)
            {
                button_videoSettings = videoTform.GetComponent<UIButton>();
                if (button_videoSettings != null)
                {
                    button_videoSettings.Initialize();
                    button_videoSettings.Event_OnAnyClick += (_, _1) => { Event_OnButtonClicked_VideoSettings?.Invoke(); };
                }
                else { Debug.LogError("[AcrealUI.UISettingsMenuPanel] Failed to load UIButton Script on GameObject \"" + _gameObjName_button_videoSettings + "\""); }
            }

            Transform audioTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_audioSettings);
            if (audioTform != null)
            {
                button_audioSettings = audioTform.GetComponent<UIButton>();
                if (button_audioSettings != null)
                {
                    button_audioSettings.Initialize();
                    button_audioSettings.Event_OnAnyClick += (_, _1) => { Event_OnButtonClicked_AudioSettings?.Invoke(); };
                }
                else { Debug.LogError("[AcrealUI.UISettingsMenuPanel] Failed to load UIButton Script on GameObject \"" + _gameObjName_button_audioSettings + "\""); }
            }

            Transform controlTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_controlSettings);
            if (controlTform != null)
            {
                button_controlSettings = controlTform.GetComponent<UIButton>();
                if (button_controlSettings != null)
                {
                    button_controlSettings.Initialize();
                    button_controlSettings.Event_OnAnyClick += (_, _1) => { Event_OnButtonClicked_ControlSettings?.Invoke(); };
                }
                else { Debug.LogError("[AcrealUI.UISettingsMenuPanel] Failed to load UIButton Script on GameObject \"" + _gameObjName_button_controlSettings + "\""); }
            }
        }
        #endregion
    }
}
