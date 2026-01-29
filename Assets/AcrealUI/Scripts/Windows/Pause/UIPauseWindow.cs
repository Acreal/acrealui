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
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace AcrealUI
{
    [ImportedComponent]
    public class UIPauseWindow : UIWindow
    {
        #region Definitions
        public enum PauseWindowState
        {
            None = 0,
            Paused = 1,
            Settings = 2,
            Settings_General = 3,
            Settings_Video = 4,
            Settings_Audio = 5,
            Settings_Controls = 6,
            SimpleMessage = 7,
            Confirmation = 8,
            KeyBinding = 9,
        }
        #endregion


        #region Variables
        [SerializeField] private string _gameObjName_panelPaused = null;
        [SerializeField] private string _gameObjName_panelSettings = null;
        [SerializeField] private string _gameObjName_panelGeneralSettings = null;
        [SerializeField] private string _gameObjName_panelAudioSettings = null;
        [SerializeField] private string _gameObjName_panelVideoSettings = null;
        [SerializeField] private string _gameObjName_panelControlSettings = null;
        [SerializeField] private string _gameObjName_panelExitGame = null;
        [SerializeField] private string _gameObjName_headerText = null;

        private UIPanelPaused _panelPaused = null;
        private UIPanelSettings _panelSettings = null;
        private UIPanelControls _panelControlSettings = null;
        private UIPanel _panelGeneralSettings = null;
        private UIPanel _panelAudioSettings = null;
        private UIPanel _panelVideoSettings = null;
        private UIPanel _panelExitGame = null;
        private TextMeshProUGUI _headerText = null;

        private PauseWindowState _currentState = PauseWindowState.None;
        private Vector2 _panelSizeOffset = Vector2.zero;
        private UIPanel _activePanel = null;
        #endregion


        #region Properties
        public PauseWindowState currentState { get { return _currentState; } }
        public UIPanelPaused panelPaused { get { return _panelPaused; } }
        public UIPanelSettings panelSettings { get { return _panelSettings; } }
        public UIPanelControls panelControlSettings { get { return _panelControlSettings; } }
        public UIPanel panelGeneralSettings { get { return _panelGeneralSettings; } }
        public UIPanel panelAudioSettings { get { return _panelAudioSettings; } }
        public UIPanel panelVideoSettings { get { return _panelVideoSettings; } }
        public UIPanel panelExitGame { get { return _panelExitGame; } }
        #endregion


        #region Initialization/Cleanup
        public override void Initialize()
        {
            base.Initialize();

            Transform pauseTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_panelPaused);
            _panelPaused = pauseTform != null ? pauseTform.GetComponent<UIPanelPaused>() : null;
            if (_panelPaused)
            { 
                _panelPaused.Initialize();
                _panelPaused.Event_OnPanelSizeChanged += OnActivePanelSizeChanged;
            }
            else { Debug.LogError("[AcrealUI.UIPauseWindow] Unable to get UIPanelPaused script from GameObject \"" + (pauseTform != null ? pauseTform.gameObject.name : "NULL")); }

            Transform settingsTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_panelSettings);
            _panelSettings = settingsTform != null ? settingsTform.GetComponent<UIPanelSettings>() : null;
            if (_panelSettings) 
            { 
                _panelSettings.Initialize();
                _panelSettings.Event_OnPanelSizeChanged += OnActivePanelSizeChanged;
            }
            else { Debug.LogError("[AcrealUI.UIPauseWindow] Unable to get UIPanelSettings script from GameObject \"" + (settingsTform != null ? settingsTform.gameObject.name : "NULL")); }

            Transform generalTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_panelGeneralSettings);
            _panelGeneralSettings = generalTform != null ? generalTform.GetComponent<UIPanel>() : null;
            if (_panelGeneralSettings) 
            {
                _panelGeneralSettings.Initialize();
                _panelGeneralSettings.Event_OnPanelSizeChanged += OnActivePanelSizeChanged;
            }
            else { Debug.LogError("[AcrealUI.UIPauseWindow] Unable to get UIPanel_GeneralSettings script from GameObject \"" + (generalTform != null ? generalTform.gameObject.name : "NULL")); }

            Transform audioTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_panelAudioSettings);
            _panelAudioSettings = audioTform != null ? audioTform.GetComponent<UIPanel>() : null;
            if (_panelAudioSettings) 
            {
                _panelAudioSettings.Initialize();
                _panelAudioSettings.Event_OnPanelSizeChanged += OnActivePanelSizeChanged;
            }
            else { Debug.LogError("[AcrealUI.UIPauseWindow] Unable to get UIPanel_AudioSettings script from GameObject \"" + (audioTform != null ? audioTform.gameObject.name : "NULL")); }

            Transform videoTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_panelVideoSettings);
            _panelVideoSettings = videoTform != null ? videoTform.GetComponent<UIPanel>() : null;
            if (_panelVideoSettings) 
            { 
                _panelVideoSettings.Initialize();
                _panelVideoSettings.Event_OnPanelSizeChanged += OnActivePanelSizeChanged;
            }
            else { Debug.LogError("[AcrealUI.UIPauseWindow] Unable to get UIPanel_VideoSettings script from GameObject \"" + (videoTform != null ? videoTform.gameObject.name : "NULL")); }

            Transform controlTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_panelControlSettings);
            _panelControlSettings = controlTform != null ? controlTform.GetComponent<UIPanelControls>() : null;
            if (_panelControlSettings)
            {
                _panelControlSettings.Initialize();
                _panelControlSettings.Event_OnPanelSizeChanged += OnActivePanelSizeChanged;
            }
            else { Debug.LogError("[AcrealUI.UIPauseWindow] Unable to get UIPanelControls script from GameObject \"" + (controlTform != null ? controlTform.gameObject.name : "NULL")); }

            Transform exitTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_panelExitGame);
            _panelExitGame = exitTform != null ? exitTform.GetComponent<UIPanel>() : null;
            if (_panelExitGame)
            {
                _panelExitGame.Initialize();
                _panelExitGame.Event_OnPanelSizeChanged += OnActivePanelSizeChanged;
            }
            else { Debug.LogError("[AcrealUI.UIPauseWindow] Unable to get UIPanel_ExitGame script from GameObject \"" + (exitTform != null ? exitTform.gameObject.name : "NULL")); }

            Transform headerTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_headerText);
            _headerText = headerTform != null ? headerTform.GetComponent<TextMeshProUGUI>() : null;

            _currentState = PauseWindowState.None;
        }
        #endregion


        #region Open/Close
        protected override void ShowInternal()
        {
            gameObject.SetActive(true);
            SetState(PauseWindowState.Paused);
            base.ShowInternal();
        }

        protected override void OnFinishedClosing()
        {
            SetState(PauseWindowState.None);
            base.OnFinishedClosing();
        }
        #endregion


        #region State Management
        public void SetState(PauseWindowState stateToSet)
        {
            if (_currentState != stateToSet)
            {
                if (_activePanel != null)
                {
                    _activePanel.Hide();
                    _activePanel = null;
                }

                PauseWindowState prevState = currentState;
                _currentState = stateToSet;
                switch (_currentState)
                {
                    case PauseWindowState.Confirmation:
                        _activePanel = _panelExitGame;
                        break;

                    case PauseWindowState.Settings:
                        _activePanel = _panelSettings;
                        if (_headerText != null)
                        {
                            //TODO(Acreal): localize this string
                            _headerText.text = "Settings";
                        }
                        break;

                    case PauseWindowState.Settings_General:
                        _activePanel = _panelGeneralSettings;
                        if (_headerText != null)
                        {
                            //TODO(Acreal): localize this string
                            _headerText.text = "General";
                        }
                        break;

                    case PauseWindowState.Settings_Video:
                        _activePanel = _panelVideoSettings;
                        if (_headerText != null)
                        {
                            //TODO(Acreal): localize this string
                            _headerText.text = "Video";
                        }
                        break;

                    case PauseWindowState.Settings_Audio:
                        _activePanel = _panelAudioSettings;
                        if (_headerText != null)
                        {
                            //TODO(Acreal): localize this string
                            _headerText.text = "Audio";
                        }
                        break;

                    case PauseWindowState.Settings_Controls:
                        _activePanel = _panelControlSettings;
                        if (_headerText != null)
                        {
                            //TODO(Acreal): localize this string
                            _headerText.text = "Controls";
                        }
                        break;

                    default:
                        _activePanel = _panelPaused;
                        if (_headerText != null)
                        {
                            //TODO(Acreal): localize this string
                            _headerText.text = "Paused";
                        }
                        break;
                }

                if (_activePanel != null)
                {
                    _activePanel.gameObject.SetActive(true);
                    _activePanel.Show();
                }
            }
        }

        private void OnActivePanelSizeChanged(Vector2 panelSize)
        {
            if (_canvasGroup != null)
            {
                RectTransform rt = _canvasGroup != null ? _canvasGroup.transform as RectTransform : transform as RectTransform;
                StartCoroutine(TweenSizeRoutine(rt.sizeDelta, panelSize + _panelSizeOffset, 0.1f));
            }
        }
        #endregion


        #region Coroutines
        private IEnumerator<float> TweenSizeRoutine(Vector2 from, Vector2 to, float duration)
        {
            Transform tForm = _canvasGroup != null ? _canvasGroup.transform as RectTransform : transform as RectTransform;
            LayoutElement layoutElem = tForm.GetComponent<LayoutElement>();

            if (layoutElem != null)
            {
                layoutElem.minWidth = from.x;
                layoutElem.minHeight = from.y;
            }

            float t = duration;
            while(t > 0f)
            {
                t -= Time.unscaledDeltaTime;
                float lerpT = 1f - Mathf.InverseLerp(0f, duration, t);

                if (layoutElem != null)
                {
                    layoutElem.minWidth = Mathf.Lerp(from.x, to.x, lerpT);
                    layoutElem.minHeight = Mathf.Lerp(from.y, to.y, lerpT);
                }

                yield return 0f;
            }

            if (layoutElem != null)
            {
                layoutElem.minWidth = to.x;
                layoutElem.minHeight = to.y;
            }
        }
        #endregion
    }
}
