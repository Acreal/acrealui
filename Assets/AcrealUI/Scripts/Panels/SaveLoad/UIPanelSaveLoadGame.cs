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

using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIPanelSaveLoadGame : UIPanel
    {
        #region Variables
        [SerializeField] private string _gameObjName_rawImage_saveScreenShot = null;
        [SerializeField] private string _gameObjName_inputField_saveName = null;
        [SerializeField] private string _gameObjName_text_header = null;
        [SerializeField] private string _gameObjName_text_realTime = null;
        [SerializeField] private string _gameObjName_text_gameTime = null;
        [SerializeField] private string _gameObjName_text_version = null;
        [SerializeField] private string _gameObjName_text_saveOrLoad = null;
        [SerializeField] private string _gameObjName_text_importSave = null;
        [SerializeField] private string _gameObjName_text_noSaves = null;
        [SerializeField] private string _gameObjName_button_switchChar = null;
        [SerializeField] private string _gameObjName_button_rename = null;
        [SerializeField] private string _gameObjName_button_delete = null;
        [SerializeField] private string _gameObjName_button_saveLoad = null;
        [SerializeField] private string _gameObjName_button_import = null;
        [SerializeField] private string _gameObjName_saveHeaderParent = null;
        [SerializeField] private string _gameObjName_saveDetailsParent = null;

        private RawImage _saveScreenShotRawImage = null;
        private TMP_InputField _saveNameInputField = null;
        private TextMeshProUGUI _headerText = null;
        private TextMeshProUGUI _realTimeText = null;
        private TextMeshProUGUI _gameTimeText = null;
        private TextMeshProUGUI _versionText = null;
        private TextMeshProUGUI _saveLoadText = null;
        private TextMeshProUGUI _importSaveText = null;
        private TextMeshProUGUI _noSavesText = null;
        private UIButton _switchCharButton = null;
        private UIButton _renameButton = null;
        private UIButton _deleteButton = null;
        private UIButton _saveLoadButton = null;
        private UIButton _importButton = null;
        private UIToggleGroup _saveEntriesToggleGroup = null;
        private GameObject _saveHeaderParent = null;
        private GameObject _saveDetailsParent = null;

        private Dictionary<string, UISaveGameEntry> _idToSaveGameDataDict = null;
        private UISaveGameData _selectedSaveGameData = new UISaveGameData();
        private bool _isSaving = false;
        private bool _isSavingOrLoadingAllowed = false;
        #endregion


        #region Properties
        public int numSaveEntries
        {
            get { return _idToSaveGameDataDict != null ? _idToSaveGameDataDict.Count : 0; }
        }

        public UISaveGameData selectedSaveGameData
        {
            get { return _selectedSaveGameData; }
        }

        public bool isSaving
        {
            get { return _isSaving; }
        }

        public string inputFieldValue
        {
            get { return _saveNameInputField != null ? _saveNameInputField.text : null; }
        }

        public UIToggleGroup toggleGroup
        {
            get { return _saveEntriesToggleGroup; }
        }
        #endregion


        #region Events
        public event System.Action Event_OnButtonClicked_RenameSave = null;
        public event System.Action Event_OnButtonClicked_DeleteSave = null;
        public event System.Action Event_OnButtonClicked_SaveGame = null;
        public event System.Action Event_OnButtonClicked_LoadGame = null;
        public event System.Action Event_OnButtonClicked_SelectCharacter = null;
        public event System.Action Event_OnButtonClicked_ImportSave = null;
        public event System.Action<string> Event_OnInputFieldChanged_SaveFileName = null;
        #endregion


        #region Initialization/Cleanup
        public override void Initialize()
        {
            base.Initialize();

            _idToSaveGameDataDict = new Dictionary<string, UISaveGameEntry>();
            
            _saveEntriesToggleGroup = _scrollGroupParent.GetComponent<UIToggleGroup>();
            _saveEntriesToggleGroup.Initialize();
            _saveEntriesToggleGroup.canBeToggledOff = true;

            if (!string.IsNullOrEmpty(_gameObjName_saveHeaderParent))
            {
                Transform saveHeaderParentTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_saveHeaderParent);
                if (saveHeaderParentTform != null) { _saveHeaderParent = saveHeaderParentTform.gameObject; }
                else { Debug.LogError("[AcrealUI.UIPanelSaveLoadGame] Failed to find GameObject by name: \"" + _gameObjName_saveHeaderParent + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_saveDetailsParent))
            {
                Transform saveDetailsParentTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_saveDetailsParent);
                if (saveDetailsParentTform != null) { _saveDetailsParent = saveDetailsParentTform.gameObject; }
                else { Debug.LogError("[AcrealUI.UIPanelSaveLoadGame] Failed to find GameObject by name: \"" + _gameObjName_saveDetailsParent + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_inputField_saveName))
            {
                Transform inputFieldTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_inputField_saveName);
                if (inputFieldTform != null) { _saveNameInputField = inputFieldTform.GetComponent<TMP_InputField>(); }
                if (_saveNameInputField != null)
                {
                    _saveNameInputField.onValueChanged.AddListener((string inputText) =>
                    {
                        if (_saveNameInputField.isFocused)
                        {
                            Event_OnInputFieldChanged_SaveFileName?.Invoke(inputText);
                        }
                    });
                }
                else { Debug.LogError("[AcrealUI.UIPanelSaveLoadGame] Failed to find TMP_InputField script on GameObject \"" + _gameObjName_inputField_saveName + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_rawImage_saveScreenShot))
            {
                Transform screenshotTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_rawImage_saveScreenShot);
                if (screenshotTform != null) { _saveScreenShotRawImage = screenshotTform.GetComponent<RawImage>(); }
                else { Debug.LogError("[AcrealUI.UIPanelSaveLoadGame] Failed to find RawImage script on GameObject \"" + _gameObjName_rawImage_saveScreenShot + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_button_switchChar))
            {
                Transform switchTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_switchChar);
                if (switchTform != null) { _switchCharButton = switchTform.GetComponent<UIButton>(); }
                if (_switchCharButton != null)
                {
                    _switchCharButton.Initialize();
                    _switchCharButton.Event_OnClicked += (_) => { Event_OnButtonClicked_SelectCharacter?.Invoke(); };
                }
                else { Debug.LogError("[AcrealUI.UIPanelSaveLoadGame] Failed to find UIButton script on GameObject \"" + _gameObjName_button_switchChar + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_button_rename))
            {
                Transform renameTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_rename);
                if (renameTform != null) { _renameButton = renameTform.GetComponent<UIButton>(); }
                if (_renameButton != null)
                {
                    _renameButton.Initialize();
                    _renameButton.Event_OnClicked += (_) => { Event_OnButtonClicked_RenameSave?.Invoke(); };
                }
                else { Debug.LogError("[AcrealUI.UIPanelSaveLoadGame] Failed to find UIButton script on GameObject \"" + _gameObjName_button_rename + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_button_delete))
            {
                Transform deleteTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_delete);
                if (deleteTform != null) { _deleteButton = deleteTform.GetComponent<UIButton>(); }
                if (_deleteButton != null)
                {
                    _deleteButton.Initialize();
                    _deleteButton.Event_OnClicked += (_) => { Event_OnButtonClicked_DeleteSave?.Invoke(); };
                }
                else { Debug.LogError("[AcrealUI.UIPanelSaveLoadGame] Failed to find UIButton script on GameObject \"" + _gameObjName_button_delete + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_button_saveLoad))
            {
                Transform saveLoadTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_saveLoad);
                if (saveLoadTform != null) { _saveLoadButton = saveLoadTform.GetComponent<UIButton>(); }
                if (_saveLoadButton != null)
                {
                    _saveLoadButton.Initialize();
                    _saveLoadButton.Event_OnClicked += (_) =>
                    {
                        if (_isSaving) { Event_OnButtonClicked_SaveGame?.Invoke(); }
                        else { Event_OnButtonClicked_LoadGame?.Invoke(); }
                    };
                }
                else { Debug.LogError("[AcrealUI.UIPanelSaveLoadGame] Failed to find UIButton script on GameObject \"" + _gameObjName_button_saveLoad + "\""); }
            }

            if(!string.IsNullOrEmpty(_gameObjName_text_saveOrLoad))
            {
                Transform saveTextTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_saveOrLoad);
                if (saveTextTform != null) { _saveLoadText = saveTextTform.GetComponent<TextMeshProUGUI>(); }
                if (_saveLoadText == null) { Debug.LogError("[AcrealUI.UIPanelSaveLoadGame] Failed to find TextMeshProUGUI script on GameObject \"" + _gameObjName_text_saveOrLoad + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_button_import))
            {
                Transform importTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_import);
                if (importTform != null) { _importButton = importTform.GetComponent<UIButton>(); }
                if (_importButton != null)
                {
                    _importButton.Initialize();
                    _importButton.Event_OnClicked += (_) => { Event_OnButtonClicked_ImportSave?.Invoke(); };
                }
                else { Debug.LogError("[AcrealUI.UIPanelSaveLoadGame] Failed to find UIButton script on GameObject \"" + _gameObjName_button_import + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_text_realTime))
            {
                Transform rtTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_realTime);
                if (rtTform != null) { _realTimeText = rtTform.GetComponent<TextMeshProUGUI>(); }
                if (_realTimeText == null) { Debug.LogError("[AcrealUI.UIPanelSaveLoadGame] Failed to find TextMeshProUGUI script on GameObject \"" + _gameObjName_text_realTime + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_text_gameTime))
            {
                Transform gtTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_gameTime);
                if (gtTform != null) { _gameTimeText = gtTform.GetComponent<TextMeshProUGUI>(); }
                if (_gameTimeText == null) { Debug.LogError("[AcrealUI.UIPanelSaveLoadGame] Failed to find TextMeshProUGUI script on GameObject \"" + _gameObjName_text_gameTime + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_text_version))
            {
                Transform versTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_version);
                if (versTform != null) { _versionText = versTform.GetComponent<TextMeshProUGUI>(); }
                if (_versionText == null) { Debug.LogError("[AcrealUI.UIPanelSaveLoadGame] Failed to find TextMeshProUGUI script on GameObject \"" + _gameObjName_text_version + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_text_noSaves))
            {
                Transform noSavesTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_noSaves);
                if (noSavesTform != null) { _noSavesText = noSavesTform.GetComponent<TextMeshProUGUI>(); }
                if (_noSavesText != null) { _noSavesText.text = UIUtilityFunctions.GetLocalizedText("noSavesFound"); }
                else { Debug.LogError("[AcrealUI.UIPanelSaveLoadGame] Failed to find TextMeshProUGUI script on GameObject \"" + _gameObjName_text_noSaves + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_text_importSave))
            {
                Transform importTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_importSave);
                if (importTform != null) { _importSaveText = importTform.GetComponent<TextMeshProUGUI>(); }
                if (_importSaveText != null) { _importSaveText.text = UIUtilityFunctions.GetLocalizedText("classicSave"); }
                else { Debug.LogError("[AcrealUI.UIPanelSaveLoadGame] Failed to find TextMeshProUGUI script on GameObject \"" + _gameObjName_text_importSave + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_text_header))
            {
                Transform headerTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_header);
                if (headerTform != null) { _headerText = headerTform.GetComponent<TextMeshProUGUI>(); }
                if (_headerText == null) { Debug.LogError("[AcrealUI.UIPanelSaveLoadGame] Failed to find TextMeshProUGUI script on GameObject \"" + _gameObjName_text_header + "\""); }
            }
        }
        #endregion


        #region Show/Hide
        public override void Show()
        {
            if (!_isSaving)
            {
                SelectMostRecentSaveGameData();
            }
            else
            {
                ClearSelectedSaveData();
            }

            if(_saveEntriesToggleGroup != null)
            {
                _saveEntriesToggleGroup.ToggleDefault();
            }

            base.Show();
        }

        public override void Hide()
        {
            ClearSelectedSaveData();
            base.Hide();
        }
        #endregion


        #region Public API
        public void SetIsSaving(bool saving)
        {
            _isSaving = saving;
        }

        public void SetSavingOrLoadingAllowed(bool allowSavingOrLoading)
        {
            _isSavingOrLoadingAllowed = allowSavingOrLoading;
        }

        public void SetNoSavesTextActive(bool active)
        {
            if(_noSavesText != null)
            {
                _noSavesText.gameObject.SetActive(active);
            }
        }

        public void SetHeaderActive(bool active)
        {
            if(_saveHeaderParent != null)
            {
                _saveHeaderParent.SetActive(active);
            }
        }

        public void SetSaveDetailsActive(bool active)
        {
            if(_saveDetailsParent != null)
            {
                _saveDetailsParent.SetActive(active);
            }
        }

        public void SetImportButtonActive(bool active)
        {
            if(_importButton != null)
            {
                _importButton.gameObject.SetActive(active);
            }
        }

        public void SetSaveOrLoadButtonEnabled(bool enabled)
        {
            if (_saveLoadButton != null)
            {
                _saveLoadButton.isDisabled = !_isSavingOrLoadingAllowed && !enabled;
            }
        }

        public void SetInputFieldValue(string val, bool setWithoutNotify = true)
        {
            if(_saveNameInputField != null)
            {
                if (setWithoutNotify)
                {
                    _saveNameInputField.SetTextWithoutNotify(val);
                }
                else
                {
                    _saveNameInputField.text = val;
                }
            }
        }

        public void SelectMostRecentSaveGameData()
        {
            SetSelectedSaveGameData(UIUtilityFunctions.GetMostRecentSaveGameData());
        }

        public void ClearSelectedSaveData()
        {
            DaggerfallEntity playerEntity = GameManager.Instance.PlayerEntity;
            string characterName = playerEntity != null ? playerEntity.Name : null;
            SetSelectedSaveGameData(new UISaveGameData { characterName = characterName });
        }

        public void SetSelectedSaveGameData(UISaveGameData saveGameData)
        {
            _selectedSaveGameData = saveGameData;
            UpdateHeader();
            UpdateVersion();
            UpdateTimestamps();
            UpdateScreenshotImage();
            UpdateButtonText();
        }

        public bool HasSaveEntry(string entryId)
        {
            return _idToSaveGameDataDict.ContainsKey(entryId);
        }

        public UISaveGameEntry GetSaveEntryByFilename(string filename)
        {
            foreach (UISaveGameEntry saveEntry in _idToSaveGameDataDict.Values)
            {
                UISaveGameData saveData = saveEntry.GetSaveData();
                if(!string.IsNullOrWhiteSpace(saveData.saveFilePath) && saveData.saveFilePath == filename)
                {
                    return saveEntry;
                }
            }
            return null;
        }

        public UISaveGameEntry GetOrAddSaveEntry(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) { return null; }

            UISaveGameEntry entry = null;
            _idToSaveGameDataDict.TryGetValue(name, out entry);
            if(entry == null)
            {
                entry = Instantiate(UIManager.referenceManager.prefab_saveEntry, _scrollGroupParent);
                if (entry != null)
                {
                    entry.Initialize();

                    if (_saveEntriesToggleGroup != null)
                    {
                        _saveEntriesToggleGroup.AddToggle(entry);
                    }

                    entry.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                    {
                        UISaveGameEntry saveEntry = toggle as UISaveGameEntry;
                        if (saveEntry != null)
                        {
                            if (saveEntry.isToggledOn)
                            {
                                SetSelectedSaveGameData(saveEntry.GetSaveData());
                            }
                        }
                    };

                    _idToSaveGameDataDict[name] = entry;
                }
            }
            else
            {
                Debug.Log("Duplicate entry found for: " + name);
            }
            return entry;
        }

        public void RemoveSaveEntry(string name)
        {
            UISaveGameEntry entry = null;
            if(_idToSaveGameDataDict.TryGetValue(name, out entry))
            {
                if (entry != null)
                {
                    if (_saveEntriesToggleGroup != null)
                    {
                        _saveEntriesToggleGroup.RemoveToggle(entry);
                    }

                    _idToSaveGameDataDict.Remove(name);
                    Destroy(entry.gameObject);
                }
            }
        }

        public void ClearSaveEntries()
        {
            if (_idToSaveGameDataDict.Count > 0)
            {
                List<string> destroyList = new List<string>();
                foreach(KeyValuePair<string, UISaveGameEntry> kvp in _idToSaveGameDataDict)
                {
                    if(kvp.Value != null)
                    {
                        destroyList.Add(kvp.Key);
                    }
                }

                for(int i = 0; i < destroyList.Count; i++)
                {
                    RemoveSaveEntry(destroyList[i]);
                }
            }
        }
        #endregion


        #region UI Element Updates
        private void UpdateHeader()
        {
            if (_headerText != null)
            {
                string promptKey = _isSaving ? "savePrompt" : "loadPrompt";
                string promptText = UIUtilityFunctions.GetLocalizedText(promptKey);
                _headerText.text = string.Format(UIUtilityFunctions.GetLocalizedText("saveLoadPromptFormat"), promptText, _selectedSaveGameData.characterName);
            }
        }

        private void UpdateTimestamps()
        {
            if(_selectedSaveGameData.isValid)
            {
                if (_realTimeText != null)
                {
                    _realTimeText.text = _selectedSaveGameData.realTimestamp;
                }

                if (_gameTimeText != null)
                {
                    _gameTimeText.text = _selectedSaveGameData.gameTimestamp;
                }
            }
            else
            {
                if (_realTimeText != null)
                {
                    _realTimeText.text = null;
                }

                if (_gameTimeText != null)
                {
                    _gameTimeText.text = null;
                }
            }
        }

        private void UpdateVersion()
        {
            if (_versionText != null)
            {
                _versionText.text = _selectedSaveGameData.isValid ? _selectedSaveGameData.gameVersion : null;
            }
        }

        private void UpdateScreenshotImage()
        {
            if (_saveScreenShotRawImage != null)
            {
                _saveScreenShotRawImage.texture = _selectedSaveGameData.screenshot;
                _saveScreenShotRawImage.enabled = _selectedSaveGameData.screenshot != null;

                RectTransform rt = _saveScreenShotRawImage.transform.parent as RectTransform;
                if (rt != null)
                {
                    Vector2 size = rt.sizeDelta;
                    if (_selectedSaveGameData.screenshot != null)
                    {
                        size.y = size.x / (float)(_selectedSaveGameData.screenshot.width / (float)_selectedSaveGameData.screenshot.height);
                    }
                    else
                    {
                        size.y = size.x / (16f / 9f);
                    }

                    LayoutElement layoutElem = rt.GetComponent<LayoutElement>();
                    if (layoutElem != null)
                    {
                        layoutElem.minHeight = size.y;
                    }
                    else
                    {
                        rt.sizeDelta = size;
                    }
                }
            }
        }

        private void UpdateButtonText()
        {
            if (_saveLoadText != null)
            {
                string locKey = _isSaving ? "saveButton" : "loadButton";
                _saveLoadText.text = UIUtilityFunctions.GetLocalizedText(locKey);
            }
        }
        #endregion
    }
}
