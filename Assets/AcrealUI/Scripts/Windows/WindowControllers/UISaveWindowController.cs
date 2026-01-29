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
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using System.Collections.Generic;
using UnityEngine;

namespace AcrealUI
{
    public class UISaveWindowController : DaggerfallUnitySaveGameWindow, IWindowController
    {
        #region Definitions
        public enum SaveWindowState
        {
            None = 0,
            Save = 1,
            Load = 2,
            SelectCharacter = 3,
            ImportSave = 4,
        }
        #endregion


        #region Variables
        private UIWindowSaveLoad _saveLoadGameWindow = null;
        private UISaveGameData _selectedSaveGameData = new UISaveGameData();
        private SaveWindowState _currentState = SaveWindowState.None;
        #endregion


        #region Constructor
        public UISaveWindowController(IUserInterfaceManager uiManager, Modes mode, DaggerfallBaseWindow previous = null, bool displayMostRecentChar = false) : base(uiManager, mode, previous, displayMostRecentChar)
        {
            if (UIManager.referenceManager.prefab_saveLoadWindow != null)
            {
                // TODO(Acreal): keep track of window instances internally so there's no need
                // to instantiate every time
                _saveLoadGameWindow = Object.Instantiate(UIManager.referenceManager.prefab_saveLoadWindow);

                if (_saveLoadGameWindow != null)
                {
                    _saveLoadGameWindow.Initialize();
                    _saveLoadGameWindow.SetIsSaving(mode == Modes.SaveGame);

                    _saveLoadGameWindow.Event_ButtonClick_SaveGame += () =>
                    {
                        string characterName = GameManager.Instance.PlayerEntity.Name;
                        string saveName = _saveLoadGameWindow.inputFieldValue;

                        if (string.IsNullOrWhiteSpace(saveName))
                        {
                            DaggerfallUI.MessageBox(TextManager.Instance.GetLocalizedText("youMustEnterASaveName"));
                            return;
                        }

                        int key = GameManager.Instance.SaveLoadManager.FindSaveFolderByNames(characterName, saveName);
                        if (key != -1)
                        {
                            DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);
                            messageBox.SetText(new string[] { TextManager.Instance.GetLocalizedText("confirmOverwriteSave"), "" });
                            messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
                            messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No);
                            messageBox.OnButtonClick += (DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton) =>
                            {
                                if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
                                {
                                    GameManager.Instance.SaveLoadManager.Save(characterName, saveName);
                                    DaggerfallUI.Instance.PopToHUD();
                                }
                                else
                                {
                                    CloseWindow();
                                }
                            };
                            uiManager.PushWindow(messageBox);
                        }
                        else
                        {
                            GameManager.Instance.SaveLoadManager.Save(characterName, saveName);
                            DaggerfallUI.Instance.PopToHUD();
                        }
                    };

                    _saveLoadGameWindow.Event_ButtonClick_LoadGame += () =>
                    {
                        UISaveGameData saveData = _saveLoadGameWindow.selectedSaveGameData;
                        if (saveData.saveKey >= 0)
                        {
                            GameManager.Instance.SaveLoadManager.Load(saveData.saveKey);
                            DaggerfallUI.Instance.PopToHUD();
                        }
                    };

                    _saveLoadGameWindow.Event_ButtonClick_RenameSave += () =>
                    {
                        // Must have a save selected
                        if (_selectedSaveGameData.saveKey < 0)
                        {
                            return;
                        }

                        // Input save name
                        DaggerfallInputMessageBox messageBox = new DaggerfallInputMessageBox(uiManager, this);
                        messageBox.SetTextBoxLabel(TextManager.Instance.GetLocalizedText("enterSaveName") + ": ");
                        messageBox.TextBox.Text = _selectedSaveGameData.saveName;
                        messageBox.OnGotUserInput += RenameSaveButton_OnGotUserInput;
                        uiManager.PushWindow(messageBox);
                    };

                    _saveLoadGameWindow.Event_ButtonClick_DeleteSave += () =>
                    {
                        // Must have a save selected
                        if (string.IsNullOrWhiteSpace(_selectedSaveGameData.saveName))
                        {
                            return;
                        }

                        // Confirmation
                        DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);
                        messageBox.SetText(new string[] { TextManager.Instance.GetLocalizedText("confirmDeleteSave"), "" });
                        messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Delete);
                        messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Cancel);
                        messageBox.OnButtonClick += ConfirmDelete_OnButtonClick;
                        uiManager.PushWindow(messageBox);
                    };

                    _saveLoadGameWindow.Event_ButtonClick_ImportSave += () =>
                    {
                        UIUtilityFunctions.PlayButtonClick();
                        SetState(SaveWindowState.ImportSave);
                    };

                    _saveLoadGameWindow.Event_ButtonClick_SelectCharacter += () =>
                    {
                        UIUtilityFunctions.PlayButtonClick();
                        SetState(SaveWindowState.SelectCharacter);
                    };

                    _saveLoadGameWindow.Event_InputFieldChanged_SaveFileName += (string saveFileName) =>
                    {
                        UISaveGameEntry saveEntry = _saveLoadGameWindow.GetSaveEntryBySaveName(saveFileName);
                        if (saveEntry != null)
                        {
                            saveEntry.isToggledOn = true;

                            if (mode == Modes.SaveGame)
                            {
                                _saveLoadGameWindow.SetSaveOrLoadButtonEnabled(true);
                            }
                            else
                            {
                                _saveLoadGameWindow.SetSaveOrLoadButtonEnabled(saveEntry.GetSaveData().saveKey >= 0);
                            }
                        }
                        else
                        {
                            _saveLoadGameWindow.ClearSelectedSaveData();
                            _saveLoadGameWindow.toggleGroup.SetActiveToggle(null);
                            _saveLoadGameWindow.SetSaveOrLoadButtonEnabled(mode == Modes.SaveGame);
                        }
                    };
                }
            }
        }
        #endregion


        #region Base Class Overrides
        public override void OnPush()
        {
            currentPlayerName = GameManager.Instance != null && GameManager.Instance.PlayerEntity != null ? GameManager.Instance.PlayerEntity.Name : null;

            // TODO(Acreal): find a better way to determine this rather than checking for "Nameless"
            // assuming players can name themselves this intentionally
            if (string.IsNullOrWhiteSpace(currentPlayerName) || currentPlayerName == "Nameless")
            {
                currentPlayerName = UIUtilityFunctions.GetMostRecentSaveGameData().characterName;
            }

            if (!_selectedSaveGameData.isValid)
            {
                _selectedSaveGameData = UIUtilityFunctions.GetMostRecentSaveGameData(currentPlayerName);
            }

            if (mode == Modes.SaveGame)
            {
                SetState(SaveWindowState.Save);
            }
            else
            {
                SetState(SaveWindowState.Load);
            }

            ShowWindow();
        }

        public override void OnPop()
        {
            base.OnPop();

            _selectedSaveGameData = new UISaveGameData();

            // TODO(Acreal): keep track of window instances internally so there's no need
            // to destroy every time
            if (_saveLoadGameWindow != null)
            {
                _saveLoadGameWindow.Hide();
                Object.Destroy(_saveLoadGameWindow.gameObject);
            }
            _saveLoadGameWindow = null;
        }

        protected override void Setup()
        {
            ParentPanel.BackgroundColor = ScreenDimColor;
        }

        protected void UpdateSavesEntries()
        {
            if(_saveLoadGameWindow == null) { return; }

            _saveLoadGameWindow.ClearSaveEntries();

            switch(_currentState)
            {
                case SaveWindowState.Save:
                case SaveWindowState.Load:
                    bool hasSaves = false;
                    if (!string.IsNullOrWhiteSpace(currentPlayerName))
                    {
                        // Get list of all save files (and any relevant meta info) for this character name
                        List<UISaveGameData> allSaveData = UIUtilityFunctions.GetAllCharacterSaveGameData(currentPlayerName);
                        if (allSaveData != null)
                        {
                            hasSaves = allSaveData.Count > 0;
                            for (int i = 0; i < allSaveData.Count; i++)
                            {
                                UISaveGameData saveData = allSaveData[i];
                                if (saveData.saveKey < 0)
                                {
                                    return;
                                }

                                // Use saveKey.ToString() here (rather than saveName) so that we can
                                // retrieve the correct entry even if the saveName changes
                                UISaveGameEntry saveEntry = _saveLoadGameWindow.GetOrAddSaveEntry(saveData.saveKey.ToString());
                                if (saveEntry != null)
                                {
                                    saveEntry.SetSaveData(saveData);
                                    saveEntry.SetDisplayName(saveData.saveName);

                                    saveEntry.transform.SetSiblingIndex(i);

                                    saveEntry.Event_OnToggledOn += (UIToggle toggle) =>
                                    {
                                        _saveLoadGameWindow.SetInputFieldValue(toggle.displayName, false);
                                    };
                                }
                            }

                            if (_selectedSaveGameData.saveKey >= 0)
                            {
                                UISaveGameEntry selectedEntry = _saveLoadGameWindow.GetSaveEntryBySaveName(_selectedSaveGameData.saveName);
                                if(selectedEntry != null)
                                {
                                    selectedEntry.isToggledOn = true;
                                }
                            }
                        }
                    }

                    if (_saveLoadGameWindow != null)
                    {
                        _saveLoadGameWindow.SetNoSavesTextActive(!hasSaves);
                    }
                    break;

                case SaveWindowState.SelectCharacter:
                    string[] allChars = UIUtilityFunctions.GetAllSavedCharacterNames();
                    if (allChars != null)
                    {
                        // Populate list with character names instead of save file names
                        foreach (string charName in allChars)
                        {
                            UISaveGameData saveData = new UISaveGameData
                            {
                                isValid = true,
                                characterName = charName,
                            };

                            UISaveGameEntry saveEntry = _saveLoadGameWindow.GetOrAddSaveEntry(saveData.characterName);
                            if (saveEntry != null)
                            {
                                saveEntry.SetSaveData(saveData);
                                saveEntry.SetDisplayName(saveData.characterName);

                                saveEntry.Event_OnToggledOn += (UIToggle toggle) =>
                                {
                                    UISaveGameEntry entry = toggle as UISaveGameEntry;
                                    if (entry != null)
                                    {
                                        currentPlayerName = entry.GetSaveData().characterName;

                                        _saveLoadGameWindow.SetSelectedSaveGameData(UIUtilityFunctions.GetMostRecentSaveGameData(currentPlayerName));

                                        if (mode == Modes.SaveGame)
                                        {
                                            SetState(SaveWindowState.Save);
                                        }
                                        else
                                        {
                                            SetState(SaveWindowState.Load);
                                        }
                                    }
                                };
                            }
                        }
                    }
                    break;
            }
        }

        protected override void RenameSaveButton_OnGotUserInput(DaggerfallInputMessageBox sender, string saveName)
        {
            if (string.IsNullOrEmpty(saveName))
            {
                return;
            }

            // NOTE(Acreal): base func used 'saveNameTextBox.Text' here so we can't call it
            int key = GameManager.Instance.SaveLoadManager.FindSaveFolderByNames(currentPlayerName, _selectedSaveGameData.saveName);
            if (key == -1)
            {
                return;
            }

            GameManager.Instance.SaveLoadManager.Rename(key, saveName);
            _selectedSaveGameData.saveName = saveName;
            UpdateSavesEntries();
            _saveLoadGameWindow.SetSelectedSaveGameData(_selectedSaveGameData);
        }

        protected override void ConfirmDelete_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Delete)
            {
                // Get save key and delete file
                int key = GameManager.Instance.SaveLoadManager.FindSaveFolderByNames(_selectedSaveGameData.characterName, _selectedSaveGameData.saveName);
                if (key == -1)
                {
                    return;
                }
                GameManager.Instance.SaveLoadManager.DeleteSaveFolder(key);

                UpdateSavesEntries();

                // Reselect save entry or indicate no saves are found
                UISaveGameData saveData = UIUtilityFunctions.GetMostRecentSaveGameData();
                UISaveGameEntry saveEntry = _saveLoadGameWindow.GetOrAddSaveEntry(saveData.saveKey.ToString());
                if (saveEntry != null)
                {
                    saveEntry.isToggledOn = true;
                }
                else
                {
                    _selectedSaveGameData = new UISaveGameData();
                    _saveLoadGameWindow.ClearSelectedSaveData();
                    _saveLoadGameWindow.SetNoSavesTextActive(_saveLoadGameWindow.numSaveEntries == 0);
                }
            }

            CloseWindow();
        }

        public override void CancelWindow()
        {
            HideWindow();
            base.CancelWindow();
        }

        // NOTE(Acreal): I would normally override this instead of using UpdateSaveEntries(),
        // but the compiler gets angry if I do that, so we empty this function and use a
        // separate one instead
        protected override void RefreshSavesList() { }
        public override void Draw() { }
        #endregion


        #region IWindowController
        public void ShowWindow()
        {
            if(PreviousWindow != null)
            {
                IWindowController prev = PreviousWindow as IWindowController;
                if(prev != null)
                {
                    prev.HideWindow();
                }
            }

            if(_saveLoadGameWindow != null)
            {
                _saveLoadGameWindow.Show();
            }
        }

        public void HideWindow()
        {
            if (_saveLoadGameWindow != null)
            {
                _saveLoadGameWindow.Hide();
            }

            if (PreviousWindow != null)
            {
                IWindowController prev = PreviousWindow as IWindowController;
                if (prev != null)
                {
                    prev.ShowWindow();
                }
            }
        }
        #endregion


        #region State Management
        private void SetState(SaveWindowState stateToSet)
        {
            if (_currentState != stateToSet)
            {
                SaveWindowState prevState = _currentState;
                _currentState = stateToSet;

                if (_saveLoadGameWindow != null)
                {
                    _saveLoadGameWindow.SetScrollListActive(_currentState != SaveWindowState.ImportSave);

                    switch (_currentState)
                    {
                        case SaveWindowState.Save:
                        case SaveWindowState.Load:
                            _saveLoadGameWindow.SetInputFieldValue(null);
                            _saveLoadGameWindow.SetHeaderActive(true);
                            _saveLoadGameWindow.SetSaveDetailsActive(true);
                            _saveLoadGameWindow.SetImportButtonActive(_currentState == SaveWindowState.Load);
                            _saveLoadGameWindow.SetSelectCharacterButtonActive(_currentState == SaveWindowState.Load);
                            break;

                        case SaveWindowState.ImportSave:
                        case SaveWindowState.SelectCharacter:
                            _saveLoadGameWindow.SetHeaderActive(false);
                            _saveLoadGameWindow.SetSaveDetailsActive(false);
                            _saveLoadGameWindow.SetImportButtonActive(false);
                            break;
                    }

                    UpdateSavesEntries();
                }
            }
        }
        #endregion
    }
}
