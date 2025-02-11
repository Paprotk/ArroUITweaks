using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MonoPatcherLib;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using Sims3.UI.Hud;

namespace Arro.UITweaks
{
    public class PieMenuPatch
    {
        [ReplaceMethod(typeof(PieMenu), "Load")]
        public static void Load()
        {
            ResourceKey resKey = ResourceKey.CreateUILayoutKey("HUDPieMenuPatch", 0U);
            PieMenu.sLayout = UIManager.LoadLayoutAndAddToWindow(resKey, UICategory.PieMenu);
            PieMenu.sInstance.mPositionStack = new Vector2[1000];
        }

        [ReplaceMethod(typeof(PieMenu), "Begin")]
        public void Begin(MenuTree tree, Vector2 location)
        {
            var instance = (PieMenu)(this as object);
            instance.mTriggerHandle =
                instance.mContainer.AddTriggerHook("piemenu", TriggerActivationMode.kPermanent, 1);
            instance.mContainer.TriggerDown += instance.OnTriggerDown;
            instance.mTree = tree;
            instance.mCurrent = instance.ValidateMenuStructure(tree.mRoot);
            instance.SetupPieMenuButtons(instance.mCurrent, location, false);
            instance.mCurrentButtonSelected = -1;
            instance.mContainer.Visible = true;
            TextEdit searchTextEdit = instance.GetChildByID(185745581U, true) as TextEdit;
            if (searchTextEdit != null)
            {
                searchTextEdit.Caption = "";
                searchTextEdit.TextChange += OnTextChange;
                searchTextEdit.Visible = true;
                searchTextEdit.HideCaret = true;
                UIManager.SetFocus(InputContext.kICKeyboard, searchTextEdit);
            }

            Audio.StartSound("ui_piemenu_primary");
            UIManager.PushModal(instance);
        }

        [ReplaceMethod(typeof(PieMenu), "End")]
        public void End()
        {
            var instance = (PieMenu)(this as object);
            instance.mContainer.RemoveTriggerHook(instance.mTriggerHandle);
            TextEdit searchTextEdit = instance.GetChildByID(185745581U, true) as TextEdit;
            if (searchTextEdit != null)
            {
                searchTextEdit.TextChange -= OnTextChange;
                searchTextEdit.Visible = false;
            }

            instance.mContainer.TriggerDown -= instance.OnTriggerDown;
            instance.mTree = null;
            instance.mCurrent = null;
            mFilteredRoot = null;
            instance.mContainer.Visible = false;
            instance.mPositionStackPtr = -1;
            instance.mReturnButtonStackPtr = -1;
            instance.mCurrentButtonSelected = -1;
            instance.ResetSimHead();
            UIManager.PopModal(instance);
        }

        private void OnTextChange(WindowBase sender, UITextChangeEventArgs eventArgs)
        {
            var instance = (PieMenu)(this as object);
            string query = (sender as TextEdit).Caption;
            if (string.IsNullOrEmpty(query))
            {
                instance.mCurrent = instance.ValidateMenuStructure(instance.mTree.mRoot);
                instance.SetupPieMenuButtons(instance.mCurrent, instance.mPositionStack[instance.mPositionStackPtr],
                    false);
            }
            else
            {
                FilterMenuItems(instance, query);
            }

            SearchTextEditAutoSize();
        }

        private void SearchTextEditAutoSize()
        {
            var instance = (PieMenu)(this as object);
            TextEdit searchTextEdit = instance.GetChildByID(185745581U, true) as TextEdit;
            UIManager.SetFocus(InputContext.kICKeyboard, searchTextEdit);
            if (searchTextEdit != null && instance.mItemButtons != null && instance.mItemButtons.Length > 0)
            {
                Window firstButton = instance.mItemButtons[0];
                Vector2 position = firstButton.Area.TopLeft;
                Vector2 screenPosition = instance.mContainer.WindowToScreen(position);
                float centerX = screenPosition.x + (firstButton.Area.Width * 0.5f) - 2f;
                float searchWidth = searchTextEdit.Area.Width;
                searchTextEdit.Position = new Vector2(
                    centerX - (searchWidth * 0.5f),
                    screenPosition.y - 20f
                );
            }

            Rect currentArea = searchTextEdit.Area;
            Vector2 topLeft = currentArea.TopLeft;
            int characterCount = searchTextEdit.Caption.Length;
            float width = 13f * characterCount;
            float height = 17f;

            searchTextEdit.Area = new Rect(
                topLeft.x,
                topLeft.y,
                topLeft.x + width,
                topLeft.y + height
            );
        }

        private MenuItem mFilteredRoot;

        private struct MenuEntry
        {
            public MenuItem Item;
            public string ParentName;
        }

        private void FilterMenuItems(PieMenu pieMenu, string query, bool silent = false)
        {
            mFilteredRoot = null;
            var originalRoot = pieMenu.mTree.mRoot;
            var allEntries = new List<MenuEntry>();
            FlattenMenuItems(originalRoot, allEntries, "");
            var newRoot = new MenuItem { mTree = pieMenu.mTree };
            var normalizedQuery = RemoveDiacritics(query.ToLowerInvariant());
            var queryWords = normalizedQuery.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            var tempClonesWithParents = new List<MenuEntry>();

            foreach (MenuEntry entry in allEntries)
            {
                MenuItem item = entry.Item;
                if (item.mStyle == MenuItem.Style.More) continue;

                string normalizedItemName = RemoveDiacritics(item.mName.ToLowerInvariant());
                string[] itemWords = normalizedItemName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                bool allMatched = true;
                foreach (string qWord in queryWords)
                {
                    bool wordFound = false;
                    foreach (string iWord in itemWords)
                    {
                        if (iWord.StartsWith(qWord))
                        {
                            wordFound = true;
                            break;
                        }
                    }

                    if (!wordFound)
                    {
                        allMatched = false;
                        break;
                    }
                }

                if (allMatched)
                {
                    MenuItem clonedItem = CloneMenuItem(item);
                    tempClonesWithParents.Add(new MenuEntry
                    {
                        Item = clonedItem,
                        ParentName = entry.ParentName
                    });
                }
            }
            
            var nameGroups = new Dictionary<string, List<MenuEntry>>();
            foreach (MenuEntry entry in tempClonesWithParents)
            {
                string baseName = entry.Item.mName;
                if (!nameGroups.ContainsKey(baseName))
                {
                    nameGroups[baseName] = new List<MenuEntry>();
                }

                nameGroups[baseName].Add(entry);
            }
            
            foreach (string key in nameGroups.Keys)
            {
                List<MenuEntry> group = nameGroups[key];
                if (group.Count > 1)
                {
                    foreach (MenuEntry entry in group)
                    {
                        if (!string.IsNullOrEmpty(entry.ParentName) &&
                            !entry.Item.mName.Contains(" / "))
                        {
                            entry.Item.mName = entry.ParentName + " / " + entry.Item.mName;
                        }
                    }
                }
            }
            
            foreach (MenuEntry entry in tempClonesWithParents)
            {
                newRoot.AddChild(entry.Item);
            }

            if (newRoot.ChildCount == 0)
            {
                var instance = (PieMenu)(this as object);
                Sims3.Gameplay.UI.PieMenu.ShowGreyedOutTooltip(
                    Localization.LocalizeString("Gameplay/Abstracts/GameObject:NoInteractions", new object[0]),
                    UIManager.GetCursorPosition());
                TextEdit searchTextEdit = instance.GetChildByID(185745581U, true) as TextEdit;
                searchTextEdit.TextChange -= OnTextChange;
                searchTextEdit.Caption = searchTextEdit.Caption.Substring(0, searchTextEdit.Caption.Length - 1);
                SearchTextEditAutoSize();
                searchTextEdit.TextChange += OnTextChange;
                string newQuery = searchTextEdit.Caption;
                if (!string.IsNullOrEmpty(newQuery))
                {
                    FilterMenuItems(instance, newQuery, silent: true);
                }
                else
                {
                    instance.mCurrent = instance.ValidateMenuStructure(instance.mTree.mRoot);
                    instance.SetupPieMenuButtons(instance.mCurrent, instance.mPositionStack[instance.mPositionStackPtr],
                        false);
                }

                Audio.StartSound("ui_error");
                return;
            }

            mFilteredRoot = pieMenu.ValidateMenuStructure(newRoot);
            pieMenu.mCurrent = mFilteredRoot;
            pieMenu.SetupPieMenuButtons(mFilteredRoot,
                pieMenu.mPositionStack[pieMenu.mPositionStackPtr], false);
            if (!silent) Audio.StartSound("ui_primary_button");
        }


        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);

                if (c == 'ł')
                {
                    stringBuilder.Append('l');
                }
                else if (c == 'Ł')
                {
                    stringBuilder.Append('L');
                }
                else if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        private void FlattenMenuItems(MenuItem root, List<MenuEntry> items, string parentName)
        {
            try
            {
                foreach (MenuItem child in root.mChildren)
                {
                    items.Add(new MenuEntry { Item = child, ParentName = parentName });
                    FlattenMenuItems(child, items, child.mName);
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "FlattenMenuItems");
            }
        }

        private MenuItem CloneMenuItem(MenuItem original)
        {
            MenuItem clone = new MenuItem
            {
                mName = original.mName,
                mTag = original.mTag,
                mStyle = original.mStyle,
                mTree = original.mTree,
                mIconKey = original.mIconKey,
                mIconStyle = original.mIconStyle,
                mIconThumbnailKey = original.mIconThumbnailKey,
                mToolTip = original.mToolTip,
                mListObjs = original.mListObjs,
                mHeaders = original.mHeaders,
                mNumSelectableRows = original.mNumSelectableRows,
                mPickedObjects = original.mPickedObjects,
                mTitleDelegate = original.mTitleDelegate,
                mPickerTestDelegate = original.mPickerTestDelegate
            };

            foreach (MenuItem child in original.mChildren)
            {
                MenuItem clonedChild = CloneMenuItem(child);
                clone.AddChild(clonedChild);
            }

            return clone;
        }

        [ReplaceMethod(typeof(PieMenu), "SetupPieMenuButtons")]
        public void SetupPieMenuButtons(MenuItem menu, Vector2 origin, bool returnButtonVisible)
        {
            try
            {
                var instance = (PieMenu)(this as object);
                uint childCount = (uint)menu.ChildCount;
                int num = returnButtonVisible ? 12 : 13;
                for (int i = 0; i < num; i++)
                {
                    instance.mItemButtons[i].Visible = false;
                    instance.mItemButtons[i].Tag = null;
                }

                instance.mObjectPickerPanel.Visible = false;
                instance.mPieMenuContainer.Visible = true;
                Vector2[] array = new Vector2[childCount];
                instance.ComputeRadialLocations(ref array, childCount);
                instance.DetermineButtonIndices(ref array, childCount);
                instance.mButtonCount = childCount;
                Rect a = new Rect(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
                uint num2 = 0U;
                foreach (object obj in menu)
                {
                    MenuItem item = (MenuItem)obj;
                    if (num2 == 0U)
                    {
                        Rect b = instance.SetupButton(item, instance.mButtonIndices[(int)((UIntPtr)num2)], origin,
                            array[(int)((UIntPtr)num2)]);
                        a = Rect.Union(a, b);
                        num2 = instance.mButtonCount - 1U;
                    }
                    else
                    {
                        Rect b2 = instance.SetupButton(item, instance.mButtonIndices[(int)((UIntPtr)num2)], origin,
                            array[(int)((UIntPtr)num2)]);
                        a = Rect.Union(a, b2);
                        num2 -= 1U;
                    }
                }

                if (childCount == 1 && instance.mCurrent == mFilteredRoot)
                {
                    instance.mHeadSceneWindow.Visible = false;
                    Vector2 cursorPos = UIManager.GetCursorPosition();
                    Vector2 containerPos = instance.mContainer.ScreenToWindow(cursorPos);
                    Window button = instance.mItemButtons[0];
                    Rect buttonArea = button.Area;
                    float buttonWidth = buttonArea.Width;
                    float buttonHeight = buttonArea.Height;

                    buttonArea.TopLeft = new Vector2(
                        containerPos.x - (buttonWidth / 2f),
                        containerPos.y - (buttonHeight / 2f)
                    );
                    buttonArea.BottomRight = new Vector2(
                        buttonArea.TopLeft.x + buttonWidth,
                        buttonArea.TopLeft.y + buttonHeight
                    );
                    button.Area = buttonArea;
                    instance.mPositionStack[++instance.mPositionStackPtr] = containerPos;
                    instance.mPieMenuHitMask.Area = new Rect(0f, 0f, 0f, 0f);
                }
                else
                {
                    instance.mHeadSceneWindow.Visible = true;
                    float num3 = 0f;
                    float num4 = 0f;
                    Rect area = UIManager.GetMainWindow().Area;
                    if (a.TopLeft.x < area.TopLeft.x)
                    {
                        num3 = area.TopLeft.x - a.TopLeft.x;
                    }
                    else if (a.BottomRight.x > area.BottomRight.x)
                    {
                        num3 = area.BottomRight.x - a.BottomRight.x;
                    }

                    if (a.TopLeft.y < area.TopLeft.y)
                    {
                        num4 = area.TopLeft.y - a.TopLeft.y;
                    }
                    else if (a.BottomRight.y > area.BottomRight.y)
                    {
                        num4 = area.BottomRight.y - a.BottomRight.y;
                    }

                    instance.mPositionStack[++instance.mPositionStackPtr] = origin + new Vector2(num3, num4);


                    Rect rect = new Rect(a.TopLeft - instance.mPieMenuHitMaskPadding,
                        a.BottomRight + instance.mPieMenuHitMaskPadding);
                    rect = Rect.Offset(rect, num3, num4);
                    instance.mPieMenuHitMask.Area = rect;
                    if (num3 != 0f || num4 != 0f)
                    {
                        for (num2 = 0U; num2 < childCount; num2 += 1U)
                        {
                            uint num5 = instance.mButtonIndices[(int)((UIntPtr)num2)];
                            Rect area2 = Rect.Offset(instance.mItemButtons[(int)((UIntPtr)num5)].Area, num3, num4);
                            instance.mItemButtons[(int)((UIntPtr)num5)].Area = area2;
                        }

                        Rect area3 = Rect.Offset(instance.mItemButtons[(int)((UIntPtr)12)].Area, num3, num4);
                        instance.mItemButtons[(int)((UIntPtr)12)].Area = area3;
                        UIManager.SetCursorPosition(instance.mItemButtons[(int)((UIntPtr)12)].Parent
                            .WindowToScreen(instance.mPositionStack[instance.mPositionStackPtr]));
                    }

                    if (instance.mHeadObjectGuid.IsValid)
                    {
                        if (instance.mHeadUpdateTaskGuid.IsValid)
                        {
                            Simulator.DestroyObject(instance.mHeadUpdateTaskGuid);
                            instance.mHeadUpdateTaskGuid = ObjectGuid.InvalidObjectGuid;
                        }

                        Vector2 currentOrigin = instance.mPositionStack[instance.mPositionStackPtr];
                        float headSize = 128f;
                        Vector2 topLeft = new Vector2(currentOrigin.x - headSize, currentOrigin.y - headSize);
                        Vector2 bottomRight = new Vector2(currentOrigin.x + headSize, currentOrigin.y + headSize);
                        instance.mHeadSceneWindow.Area = new Rect(topLeft, bottomRight);
                        SimHeadUpdater newUpdater = new SimHeadUpdater(instance.mHeadSceneWindow, currentOrigin);
                        instance.mHeadUpdateTaskGuid = Simulator.AddObject(newUpdater);
                    }
                }

                for (num2 = 0U; num2 < childCount; num2 += 1U)
                {
                    uint num6 = instance.mButtonIndices[(int)((UIntPtr)num2)];
                    instance.mItemButtons[(int)((UIntPtr)num6)].Visible = true;
                }

                SearchTextEditAutoSize();
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "SetupPieMenuButtons");
            }
        }

        [ReplaceMethod(typeof(PieMenu), "OnTriggerDown")]
        public void OnTriggerDown(WindowBase sender, UITriggerEventArgs eventArgs)
        {
            if (114345170U == eventArgs.TriggerCode && PieMenu.IsVisible)
            {
                Audio.StartSound("ui_build_cancel");
                PieMenu.Hide();
                eventArgs.Handled = true;
            }

            if (114345171U == eventArgs.TriggerCode && PieMenu.IsVisible)
            {
                var pieMenu = (PieMenu)(this as object);
                if (pieMenu.mCurrent == mFilteredRoot && pieMenu.mCurrent.ChildCount == 1)
                {
                    if (pieMenu.mCurrent[0].ChildCount == 0 && pieMenu.mCurrent[0].mStyle != MenuItem.Style.Disabled)
                    {
                        pieMenu.SelectItem(pieMenu.mCurrent[0]);
                        Audio.StartSound("ui_piemenu_secondary");
                        PieMenu.Hide();
                        eventArgs.Handled = true;
                    }
                }
            }
        }
    }
}