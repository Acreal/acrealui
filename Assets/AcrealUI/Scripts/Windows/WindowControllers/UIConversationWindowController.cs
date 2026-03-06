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
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AcrealUI
{
    public class UIConversationWindowController : DaggerfallTalkWindow, IWindowController
    {
        #region Definitions
        public enum TopicState
        {
            Default = 0,
            TellMeAbout = 1,
            WhereIs = 2,
            Location = 3,
            People = 4,
            Things = 5,
            Work = 6,
        }
        #endregion


        #region Variables
        private UIConversationWindow _conversationWindowInstance = null;

        private bool _isDisplayingSubList = false;
        private List<UIButton> _topicButtonEntries = null;
        private List<UIButton> _topicEntries = null;
        private List<UIDialogueEntry> _dialogueEntries = null;
        private Dictionary<int, TalkManager.ListItem> _instanceIdToTopicListItemDict = null;
        private int _selectedTopicInstanceId = 0;
        private TopicState _currentState = TopicState.Default;
        private SpeakingStyle _currentSpeakingStyle = SpeakingStyle.Normal;

        #endregion


        #region Constructor
        public UIConversationWindowController(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null) : base(uiManager, previous)
        {
            _topicButtonEntries = new List<UIButton>();
            _topicEntries = new List<UIButton>();
            _instanceIdToTopicListItemDict = new Dictionary<int, TalkManager.ListItem>();

            _conversationWindowInstance = Object.Instantiate(UIManager.referenceManager.prefab_conversationWindow);
            if (_conversationWindowInstance != null)
            {
                _selectedTopicInstanceId = 0;

                // WINDOW INSTANCE //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                {
                    _conversationWindowInstance.Initialize();
                    _conversationWindowInstance.Hide();

                    _conversationWindowInstance.normalSpeakingStyleToggle.DataSource_IsToggledOn = (_) => { return _currentSpeakingStyle == SpeakingStyle.Normal; };
                    _conversationWindowInstance.politeSpeakingStyleToggle.DataSource_IsToggledOn = (_) => { return _currentSpeakingStyle == SpeakingStyle.Polite; };
                    _conversationWindowInstance.bluntSpeakingStyleToggle.DataSource_IsToggledOn  = (_) => { return _currentSpeakingStyle == SpeakingStyle.Blunt; };

                    _conversationWindowInstance.normalSpeakingStyleToggle.Event_OnToggledOn += (_) => { SetSpeakingStyle(SpeakingStyle.Normal); };
                    _conversationWindowInstance.politeSpeakingStyleToggle.Event_OnToggledOn += (_) => { SetSpeakingStyle(SpeakingStyle.Polite); };
                    _conversationWindowInstance.bluntSpeakingStyleToggle.Event_OnToggledOn  += (_) => { SetSpeakingStyle(SpeakingStyle.Blunt); };

                    _conversationWindowInstance.OnSubmitDialogueEntry += () =>
                    {
                        if (_selectedTopicInstanceId != 0)
                        {
                            _selectedTopicInstanceId = 0;
                        }
                        _conversationWindowInstance.SetPendingDialogue(null);
                    };
                }

                // TOPIC CATEGORY BUTTONS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                {
                    UIButton backBtn = _conversationWindowInstance.AddTopicCategoryEntry("Back", UIManager.referenceManager.prefab_button_textOnly); // TODO(Acreal): localize string
                    if (backBtn != null)
                    {
                        _topicButtonEntries.Add(backBtn);

                        backBtn.DataSource_GameObjectActive = (_) => { return _currentState != TopicState.Default; };

                        backBtn.Event_OnClicked += (UIButton btn, PointerEventData pointerData) =>
                        {
                            if (pointerData.button == PointerEventData.InputButton.Left && pointerData.clickCount == 1)
                            {
                                if (_isDisplayingSubList)
                                {
                                    SetTopicState(_currentState);
                                    _isDisplayingSubList = false;
                                }
                                else
                                {
                                    switch (_currentState)
                                    {
                                        case TopicState.TellMeAbout:
                                        case TopicState.WhereIs:
                                            SetTopicState(TopicState.Default);
                                            break;

                                        case TopicState.Location:
                                        case TopicState.People:
                                        case TopicState.Things:
                                        case TopicState.Work:
                                            SetTopicState(TopicState.WhereIs);
                                            break;
                                    }
                                }
                                RefreshTopicButtons();
                            }
                        };
                    }

                    UIButton tellMeBtn = _conversationWindowInstance.AddTopicCategoryEntry("Tell me about...", UIManager.referenceManager.prefab_button); // TODO(Acreal): localize string
                    if (tellMeBtn != null)
                    {
                        _topicButtonEntries.Add(tellMeBtn);

                        tellMeBtn.DataSource_IsDisabled = (_) => { return TalkManager.Instance.ListTopicTellMeAbout.Count <= 1; };
                        tellMeBtn.DataSource_GameObjectActive = (_) => { return _currentState == TopicState.Default; };

                        tellMeBtn.Event_OnClicked += (UIButton btn, PointerEventData pointerData) =>
                        {
                            if (pointerData.button == PointerEventData.InputButton.Left && pointerData.clickCount == 1)
                            {
                                SetTopicState(TopicState.TellMeAbout);
                                RefreshTopicButtons();
                            }
                        };
                    }

                    UIButton whereBtn = _conversationWindowInstance.AddTopicCategoryEntry("Where is...", UIManager.referenceManager.prefab_button); // TODO(Acreal): localize string
                    if (whereBtn != null)
                    {
                        _topicButtonEntries.Add(whereBtn);

                        whereBtn.DataSource_GameObjectActive = (_) => { return _currentState == TopicState.Default; };

                        whereBtn.Event_OnClicked += (UIButton btn, PointerEventData pointerData) =>
                        {
                            if (pointerData.button == PointerEventData.InputButton.Left && pointerData.clickCount == 1)
                            {
                                SetTopicState(TopicState.WhereIs);
                                RefreshTopicButtons();
                            }
                        };
                    }

                    UIButton locationBtn = _conversationWindowInstance.AddTopicCategoryEntry("Location", UIManager.referenceManager.prefab_button); // TODO(Acreal): localize string
                    if (locationBtn != null)
                    {
                        _topicButtonEntries.Add(locationBtn);

                        locationBtn.DataSource_IsDisabled = (_) => { return TalkManager.Instance.ListTopicLocation.Count <= 1; };
                        locationBtn.DataSource_GameObjectActive = (_) => { return _currentState == TopicState.WhereIs; };

                        locationBtn.Event_OnClicked += (UIButton btn, PointerEventData pointerData) =>
                        {
                            if (pointerData.button == PointerEventData.InputButton.Left && pointerData.clickCount == 1)
                            {
                                SetTopicState(TopicState.Location);
                                RefreshTopicButtons();
                            }
                        };
                    }

                    UIButton peopleBtn = _conversationWindowInstance.AddTopicCategoryEntry("People", UIManager.referenceManager.prefab_button); // TODO(Acreal): localize string
                    if (peopleBtn != null)
                    {
                        _topicButtonEntries.Add(peopleBtn);

                        peopleBtn.DataSource_IsDisabled = (_) => { return TalkManager.Instance.ListTopicPerson.Count <= 1; };
                        peopleBtn.DataSource_GameObjectActive = (_) => { return _currentState == TopicState.WhereIs; };

                        peopleBtn.Event_OnClicked += (UIButton btn, PointerEventData pointerData) =>
                        {
                            if (pointerData.button == PointerEventData.InputButton.Left && pointerData.clickCount == 1)
                            {
                                SetTopicState(TopicState.People);
                                RefreshTopicButtons();
                            }
                        };
                    }

                    UIButton thingsBtn = _conversationWindowInstance.AddTopicCategoryEntry("Things", UIManager.referenceManager.prefab_button); // TODO(Acreal): localize string
                    if (thingsBtn != null)
                    {
                        _topicButtonEntries.Add(thingsBtn);

                        thingsBtn.DataSource_IsDisabled = (_) => { return TalkManager.Instance.ListTopicThings.Count <= 1; };
                        thingsBtn.DataSource_GameObjectActive = (_) => { return _currentState == TopicState.WhereIs; };

                        thingsBtn.Event_OnClicked += (UIButton btn, PointerEventData pointerData) =>
                        {
                            if (pointerData.button == PointerEventData.InputButton.Left && pointerData.clickCount == 1)
                            {
                                SetTopicState(TopicState.Things);
                                RefreshTopicButtons();
                            }
                        };
                    }

                    UIButton workBtn = _conversationWindowInstance.AddTopicCategoryEntry("Work", UIManager.referenceManager.prefab_button); // TODO(Acreal): localize string
                    if (workBtn != null)
                    {
                        _topicButtonEntries.Add(workBtn);

                        workBtn.DataSource_GameObjectActive = (_) => { return _currentState == TopicState.WhereIs; };

                        workBtn.Event_OnClicked += (UIButton btn, PointerEventData pointerData) =>
                        {
                            if (pointerData.button == PointerEventData.InputButton.Left && pointerData.clickCount == 1)
                            {
                                SetTopicState(TopicState.Work);
                                RefreshTopicButtons();
                            }
                        };
                    }
                }
            }
        }
        #endregion


        #region Show/Hide Window
        public void ShowWindow()
        {
            RefreshTopicButtons();

            if (_conversationWindowInstance != null)
            {
                _conversationWindowInstance.SetPendingDialogue(null);
                _conversationWindowInstance.Show();
            }
        }

        public void HideWindow()
        {
            if (_conversationWindowInstance != null)
            {
                _conversationWindowInstance.Hide();
            }
        }
        #endregion


        #region Base Class Overrides
        public override void OnPush()
        {
            base.OnPush();
            ShowWindow();
        }

        public override void OnPop()
        {
            base.OnPop();
            HideWindow();
        }

        public override void Draw() { }
        #endregion


        #region Topic Categories
        private void SetTopicState(TopicState state)
        {
            _currentState = state;
            switch (_currentState)
            {
                case TopicState.Default:
                case TopicState.WhereIs:
                    SetTopicList(null);
                    break;

                case TopicState.TellMeAbout:
                    SetTopicList(TalkManager.Instance.ListTopicTellMeAbout);
                    break;

                case TopicState.Location:
                    SetTopicList(TalkManager.Instance.ListTopicLocation);
                    break;

                case TopicState.People:
                    SetTopicList(TalkManager.Instance.ListTopicPerson);
                    break;

                case TopicState.Things:
                    SetTopicList(TalkManager.Instance.ListTopicThings);
                    break;

                case TopicState.Work:
                    SetTopicList(null);
                    break;
            }

            _selectedTopicInstanceId = 0;
            RefreshTopicButtons();
        }

        public void SetTopicList(List<TalkManager.ListItem> topics)
        {
            if (_topicEntries == null) { return; }

            for (int i = _topicEntries.Count - 1; i > -1; i--)
            {
                Object.Destroy(_topicEntries[i].gameObject);
            }
            _topicEntries.Clear();

            if (topics != null)
            {
                foreach (TalkManager.ListItem item in topics)
                {
                    if (item.type == TalkManager.ListItemType.NavigationBack) { continue; }

                    if (item.caption == null) // this is a check to detect problems arising from old save data - where caption end up as null
                    {
                        item.caption = item.key; //  just try to take key as caption then (answers might still be broken)
                        if (item.caption == string.Empty)
                        {
                            item.caption = TextManager.Instance.GetLocalizedText("resolvingError");
                        }
                    }
                    else if (item.caption == string.Empty)
                    {
                        item.caption = TextManager.Instance.GetLocalizedText("resolvingError");
                    }

                    UIButton topicBtn = _conversationWindowInstance.AddTopicEntry();
                    if (topicBtn != null)
                    {
                        topicBtn.SetDisplayValue(item.caption);
                        topicBtn.SetDisplayValueTextSize(28);

                        _instanceIdToTopicListItemDict.Add(topicBtn.gameObject.GetInstanceID(), item);

                        topicBtn.Event_OnClicked += (UIButton btn, PointerEventData data) =>
                        {
                            _selectedTopicInstanceId = btn.gameObject.GetInstanceID();

                            if (_instanceIdToTopicListItemDict.TryGetValue(_selectedTopicInstanceId, out TalkManager.ListItem listItem))
                            {
                                _conversationWindowInstance.SetPendingDialogue(TalkManager.Instance.GetQuestionText(listItem, UIUtilityFunctions.SpeakingStyleToTalkTone(_currentSpeakingStyle)));
                            }
                        };

                        LayoutElement layoutElem = topicBtn.GetComponent<LayoutElement>();
                        if (layoutElem != null)
                        {
                            layoutElem.minWidth = 0;
                            layoutElem.minHeight = 42;
                            layoutElem.flexibleWidth = 1f;
                            layoutElem.flexibleHeight = 0f;
                        }

                        _topicEntries.Add(topicBtn);
                    }
                }
            }

            _conversationWindowInstance.SetTopicDividerActive(_topicEntries != null && _topicEntries.Count > 0);
        }

        private void RefreshTopicButtons()
        {
            if (_topicButtonEntries != null)
            {
                foreach (UIButton btn in _topicButtonEntries)
                {
                    btn.Refresh();
                }
            }

            _conversationWindowInstance.UpdateTopicPanelSize();
        }
        #endregion


        #region TalkTone/SpeakingStyle
        private void SetSpeakingStyle(SpeakingStyle speakingStyle)
        {
            _currentSpeakingStyle = speakingStyle;

            if (_conversationWindowInstance != null)
            {
                _conversationWindowInstance.normalSpeakingStyleToggle?.Refresh();
                _conversationWindowInstance.politeSpeakingStyleToggle?.Refresh();
                _conversationWindowInstance.bluntSpeakingStyleToggle?.Refresh();
            }

            if (_selectedTopicInstanceId != 0 && TalkManager.Instance != null)
            {
                if (_instanceIdToTopicListItemDict.TryGetValue(_selectedTopicInstanceId, out TalkManager.ListItem listItem))
                {
                    _conversationWindowInstance.SetPendingDialogue(TalkManager.Instance.GetQuestionText(listItem, UIUtilityFunctions.SpeakingStyleToTalkTone(_currentSpeakingStyle)));
                }
            }
        }
        #endregion
    }
}
