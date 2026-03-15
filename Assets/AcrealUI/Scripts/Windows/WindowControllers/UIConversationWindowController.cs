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
        #region Variables
        private Dictionary<int, TalkManager.ListItem> _instanceIdToTopicListItemDict = null;
        private UIConversationWindow _conversationWindowInstance = null;
        private Stack<List<TalkManager.ListItem>> _topicListStack = null;
        private SpeakingStyle _currentSpeakingStyle = SpeakingStyle.Normal;
        private DialogueInfo _pendingDialogueInfo = new DialogueInfo();
        private int _selectedTopicInstanceId = 0;
        #endregion


        #region Constructor
        public UIConversationWindowController(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null) : base(uiManager, previous)
        {
            _topicListStack = new Stack<List<TalkManager.ListItem>>();
            _instanceIdToTopicListItemDict = new Dictionary<int, TalkManager.ListItem>();

            _selectedTopicInstanceId = 0;

            _conversationWindowInstance = Object.Instantiate(UIManager.referenceManager.prefab_conversationWindow);
            if (_conversationWindowInstance != null)
            {
                // WINDOW INSTANCE //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                {
                    _conversationWindowInstance.Initialize();
                    _conversationWindowInstance.SetHeaderText("Conversation History"); // TODO(Acreal): localize string
                    _conversationWindowInstance.Hide();

                    _conversationWindowInstance.Event_ButtonClick_CloseWindow += () => { CancelWindow(); };

                    _conversationWindowInstance.normalSpeakingStyleToggle.DataSource_IsToggledOn = (_) => { return _currentSpeakingStyle == SpeakingStyle.Normal; };
                    _conversationWindowInstance.politeSpeakingStyleToggle.DataSource_IsToggledOn = (_) => { return _currentSpeakingStyle == SpeakingStyle.Polite; };
                    _conversationWindowInstance.bluntSpeakingStyleToggle.DataSource_IsToggledOn  = (_) => { return _currentSpeakingStyle == SpeakingStyle.Blunt; };

                    _conversationWindowInstance.normalSpeakingStyleToggle.Event_OnToggledOn += (_) => { SetSpeakingStyle(SpeakingStyle.Normal); };
                    _conversationWindowInstance.politeSpeakingStyleToggle.Event_OnToggledOn += (_) => { SetSpeakingStyle(SpeakingStyle.Polite); };
                    _conversationWindowInstance.bluntSpeakingStyleToggle.Event_OnToggledOn  += (_) => { SetSpeakingStyle(SpeakingStyle.Blunt); };

                    _conversationWindowInstance.previousTopicButton.DataSource_IsDisabled = (_) => { return _topicListStack == null || _topicListStack.Count <= 1; };
                    _conversationWindowInstance.previousTopicButton.Event_OnLeftClick += (_, _1) =>
                    {
                        PopTopicList();
                    };

                    _conversationWindowInstance.OnSubmitDialogueEntry += () =>
                    {
                        if (!string.IsNullOrWhiteSpace(_pendingDialogueInfo.dialogueText))
                        {
                            UIDialogueEntry dialogueEntry = _conversationWindowInstance.AddDialogueEntry(_pendingDialogueInfo);

                            if (_instanceIdToTopicListItemDict.TryGetValue(_selectedTopicInstanceId, out TalkManager.ListItem listItem))
                            {
                                DialogueInfo reply = new DialogueInfo
                                {
                                    entryPrefab = UIManager.referenceManager.prefab_npcDialogueEntry,
                                    speakerPortrait = texturePortrait,
                                    speakerName = TalkManager.Instance.NameNPC,
                                    dialogueText = TalkManager.Instance.GetAnswerText(listItem),
                                };
                                UIDialogueEntry replyEntry = _conversationWindowInstance.AddDialogueEntry(reply);
                            }

                            _selectedTopicInstanceId = 0;
                            _pendingDialogueInfo.dialogueText = null;
                            _conversationWindowInstance.SetPendingDialogue(null);
                        }
                    };
                }
            }
        }
        #endregion


        #region Show/Hide Window
        public void ShowWindow()
        {
            TalkManager.Instance.ForceTopicListsUpdate();

            _pendingDialogueInfo.entryPrefab = UIManager.referenceManager.prefab_playerDialogueEntry;
            _pendingDialogueInfo.speakerName = GameManager.Instance.PlayerEntity.Name;
            _pendingDialogueInfo.speakerPortrait = UIUtilityFunctions.GetPaperDollHeadTexture();
            _pendingDialogueInfo.dialogueText = null;

            if (_conversationWindowInstance != null)
            {
                // TOPICS ///////////////////////////////////////////////////////////////////////////////////////////
                {
                    TalkManager.ListItem tellMeAbt = new TalkManager.ListItem
                    {
                        type = TalkManager.ListItemType.ItemGroup,
                        caption = "Tell Me About",//UIUtilityFunctions.GetLocalizedText("button_tellmeabout"),
                        listChildItems = TalkManager.Instance.ListTopicTellMeAbout,
                    };

                    TalkManager.ListItem location = new TalkManager.ListItem
                    {
                        type = TalkManager.ListItemType.ItemGroup,
                        caption = "Location",//UIUtilityFunctions.GetLocalizedText("button_location"),
                        listChildItems = TalkManager.Instance.ListTopicLocation,
                    };

                    TalkManager.ListItem people = new TalkManager.ListItem
                    {
                        type = TalkManager.ListItemType.ItemGroup,
                        caption = "People",//UIUtilityFunctions.GetLocalizedText("button_people"),
                        listChildItems = TalkManager.Instance.ListTopicPerson,
                    };

                    TalkManager.ListItem things = new TalkManager.ListItem
                    {
                        type = TalkManager.ListItemType.ItemGroup,
                        caption = "Things",//UIUtilityFunctions.GetLocalizedText("button_things"),
                        listChildItems = TalkManager.Instance.ListTopicThings,
                    };

                    TalkManager.ListItem work = new TalkManager.ListItem
                    {
                        type = TalkManager.ListItemType.Item,
                        questionType = TalkManager.QuestionType.Work,
                        caption = "Work",//UIUtilityFunctions.GetLocalizedText("button_work"),
                    };

                    List<TalkManager.ListItem> whereIsList = new List<TalkManager.ListItem>
                    {
                        location,
                        people,
                        things,
                        work,
                    };

                    TalkManager.ListItem whereIs = new TalkManager.ListItem
                    {
                        type = TalkManager.ListItemType.ItemGroup,
                        caption = "Where Is",//UIUtilityFunctions.GetLocalizedText("button_whereis"),
                        listChildItems = whereIsList,
                    };

                    List<TalkManager.ListItem> defaultTopicList = new List<TalkManager.ListItem>
                    {
                        tellMeAbt,
                        whereIs,
                    };

                    PushTopicList(defaultTopicList);
                }

                // INITIAL DIALOGUE ////////////////////////////////////////////////////////////////////////////
                {
                    DialogueInfo greeting = new DialogueInfo
                    {
                        entryPrefab = UIManager.referenceManager.prefab_npcDialogueEntry,
                        speakerPortrait = texturePortrait,
                        speakerName = TalkManager.Instance.NameNPC,
                        dialogueText = TalkManager.Instance.NPCGreetingText,
                    };
                    UIDialogueEntry greetingEntry = _conversationWindowInstance.AddDialogueEntry(greeting);
                    _conversationWindowInstance.SetPendingDialogue(null);
                }

                _conversationWindowInstance.Show();
                _conversationWindowInstance.UpdateTopicPanelSize();
            }
        }

        public void HideWindow()
        {
            if (_conversationWindowInstance != null)
            {
                _conversationWindowInstance.ClearTopics();
                _conversationWindowInstance.ClearDialogue();
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

        //public override void Update() { }
        protected override void Setup() { }
        public override void Draw() { }
        #endregion


        #region Topics
        public void PushTopicList(List<TalkManager.ListItem> topics)
        {
            if (topics != null)
            {
                _selectedTopicInstanceId = 0;
                _topicListStack.Push(topics);
                PopulateTopicsFromList(topics);
                _conversationWindowInstance.previousTopicButton.Refresh();
            }
        }

        private void PopTopicList()
        {
            if (_topicListStack.Count > 1)
            {
                _selectedTopicInstanceId = 0;
                _topicListStack.Pop();
                PopulateTopicsFromList(_topicListStack.Peek());
                _conversationWindowInstance.previousTopicButton.Refresh();
            }
        }

        private void PopulateTopicsFromList(List<TalkManager.ListItem> topics)
        {
            if(_conversationWindowInstance == null) { return; }

            _conversationWindowInstance.ClearTopics();
            _instanceIdToTopicListItemDict.Clear();

            listCurrentTopics = topics;
            if (topics != null && topics.Count > 0)
            {
                List<TalkManager.ListItem> allGroupItems = new List<TalkManager.ListItem>();
                List<TalkManager.ListItem> allSingleItems = new List<TalkManager.ListItem>();

                for (int i = 0; i < topics.Count; i++)
                {
                    if (topics[i].type == TalkManager.ListItemType.ItemGroup)
                    {
                        allGroupItems.Add(topics[i]);
                    }
                    else if(topics[i].type == TalkManager.ListItemType.Item)
                    {
                        allSingleItems.Add(topics[i]);
                    }
                }

                foreach (TalkManager.ListItem item in allGroupItems)
                {
                    CreateTopicButton(item);
                }

                foreach (TalkManager.ListItem item in allSingleItems)
                {
                    CreateTopicButton(item);
                }

                if(allGroupItems.Count > 0 && allSingleItems.Count > 0)
                {
                    _conversationWindowInstance.ActivateTopicDividerWithSiblingIndex(allGroupItems.Count);
                }
                else
                {
                    _conversationWindowInstance.DeactivateTopicDivider();
                }

                _conversationWindowInstance.UpdateTopicPanelSize();
            }
        }

        private void CreateTopicButton(TalkManager.ListItem item)
        {
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

            UIButton btnPrefab = (item.type == TalkManager.ListItemType.ItemGroup) ?
                                 UIManager.referenceManager.prefab_button : UIManager.referenceManager.prefab_button_textOnly;

            if (btnPrefab != null)
            {
                UIButton topicBtn = _conversationWindowInstance.AddTopicEntry(item.caption, btnPrefab);
                if (topicBtn != null)
                {
                    _instanceIdToTopicListItemDict.Add(topicBtn.gameObject.GetInstanceID(), item);

                    topicBtn.SetDisplayValue(item.caption);
                    topicBtn.SetDisplayValueTextSize(28);

                    topicBtn.DataSource_IsDisabled = (GameObject btnObj) =>
                    {
                        _instanceIdToTopicListItemDict.TryGetValue(btnObj.GetInstanceID(), out TalkManager.ListItem listItem);
                        return listItem == null || (listItem.type == TalkManager.ListItemType.ItemGroup && (listItem.listChildItems == null || listItem.listChildItems.Count == 0));
                    };

                    topicBtn.Event_OnAnyClick += (UIButton btn, PointerEventData data) =>
                    {
                        _selectedTopicInstanceId = btn.gameObject.GetInstanceID();

                        if (_instanceIdToTopicListItemDict.TryGetValue(_selectedTopicInstanceId, out TalkManager.ListItem listItem))
                        {
                            if (listItem.type == TalkManager.ListItemType.ItemGroup)
                            {
                                List<TalkManager.ListItem> topicList = listItem.listChildItems;
                                if (topicList != null)
                                {
                                    PushTopicList(topicList);
                                }
                            }
                            else
                            {
                                _pendingDialogueInfo.dialogueText = TalkManager.Instance.GetQuestionText(listItem, UIUtilityFunctions.SpeakingStyleToTalkTone(_currentSpeakingStyle));
                                _conversationWindowInstance.SetPendingDialogue(_pendingDialogueInfo.dialogueText);
                            }
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

                    topicBtn.Refresh();
                }
            }
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
                    _pendingDialogueInfo.dialogueText = TalkManager.Instance.GetQuestionText(listItem, UIUtilityFunctions.SpeakingStyleToTalkTone(_currentSpeakingStyle));
                    _conversationWindowInstance.SetPendingDialogue(_pendingDialogueInfo.dialogueText);
                }
            }
        }
        #endregion
    }
}
