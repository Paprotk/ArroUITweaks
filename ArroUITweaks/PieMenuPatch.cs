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
                }

                for (num2 = 0U; num2 < childCount; num2 += 1U)
                {
                    uint num6 = instance.mButtonIndices[(int)((UIntPtr)num2)];
                    instance.mItemButtons[(int)((UIntPtr)num6)].Visible = true;
                }
                TextEdit searchTextEdit = instance.GetChildByID(185745581U, true) as TextEdit;
                if (searchTextEdit != null)
                {
                    UIManager.SetFocus(InputContext.kICKeyboard, searchTextEdit);
                    Window firstButton = instance.mItemButtons[0];
                    Vector2 position = firstButton.Area.TopLeft;
                    Vector2 screenPosition = instance.mContainer.WindowToScreen(position);
                    float centerX = screenPosition.x + (firstButton.Area.Width * 0.5f);
                    float searchWidth = searchTextEdit.Area.Width;
                    searchTextEdit.Position = new Vector2(
                        centerX - (searchWidth * 0.5f),
                        screenPosition.y - 20f
                    );
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "SetupPieMenuButtons");
            }
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
            Audio.StartSound("ui_primary_button");
            Rect currentArea = sender.Area;
            Vector2 topLeft = currentArea.TopLeft;
            
            int characterCount = query.Length;
            float width = 8f * characterCount;
            float height = 17f;
            
            sender.Area = new Rect(
                topLeft.x,
                topLeft.y,
                topLeft.x + width,
                topLeft.y + height 
            );

            if (string.IsNullOrEmpty(query))
            {
                instance.mCurrent = instance.ValidateMenuStructure(instance.mTree.mRoot);
                instance.SetupPieMenuButtons(instance.mCurrent, instance.mPositionStack[instance.mPositionStackPtr], false);
            }
            else
            {
                FilterMenuItems(instance, query);
            }
        }

        private MenuItem mFilteredRoot;

        private void FilterMenuItems(PieMenu pieMenu, string query)
        {
            List<MenuItem> allItems;

            try
            {
                mFilteredRoot = null;
                var originalRoot = pieMenu.mTree.mRoot;
                allItems = new List<MenuItem>();
                FlattenMenuItems(originalRoot, allItems);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "FilterMenuItemsInitialSetup");
                return;
            }

            MenuItem newRoot;

            try
            {
                newRoot = new MenuItem { mTree = pieMenu.mTree };
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "FilterMenuItemsCreateNewRoot");
                return;
            }

            string[] queryWords;

            try
            {
                var normalizedQuery = RemoveDiacritics(query.ToLowerInvariant());
                queryWords = normalizedQuery.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "FilterMenuItemsProcessQuery");
                return;
            }

            try
            {
                foreach (MenuItem item in allItems)
                {
                    if (item.mStyle == MenuItem.Style.More) continue;

                    string[] itemWords;

                    try
                    {
                        var normalizedItemName = RemoveDiacritics(item.mName.ToLowerInvariant());
                        itemWords = normalizedItemName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    catch (Exception ex)
                    {
                        ExceptionHandler.HandleException(ex, "FilterMenuItemsProcessItemName");
                        continue;
                    }

                    bool allQueryWordsMatched = true;

                    try
                    {
                        foreach (string qWord in queryWords)
                        {
                            bool wordMatched = false;

                            foreach (string iWord in itemWords)
                            {
                                if (iWord.StartsWith(qWord))
                                {
                                    wordMatched = true;
                                    break;
                                }
                            }

                            if (!wordMatched)
                            {
                                allQueryWordsMatched = false;
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionHandler.HandleException(ex, "FilterMenuItemsCheckQueryWords");
                        allQueryWordsMatched = false;
                    }

                    if (allQueryWordsMatched)
                    {
                        try
                        {
                            MenuItem clonedItem = CloneMenuItem(item);
                            newRoot.AddChild(clonedItem);
                        }
                        catch (Exception ex)
                        {
                            ExceptionHandler.HandleException(ex, "FilterMenuItemsCloneMenuItem");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "FilterMenuItemsMainLoops");
            }

            try
            {
                if (newRoot.ChildCount == 0)
                {
                    var instance = (PieMenu)(this as object);
                    Sims3.Gameplay.UI.PieMenu.ShowGreyedOutTooltip(Localization.LocalizeString("Gameplay/Abstracts/GameObject:NoInteractions", new object[0]), UIManager.GetCursorPosition());
                    TextEdit searchTextEdit = instance.GetChildByID(185745581U, true) as TextEdit;
                    searchTextEdit.Caption = searchTextEdit.Caption.Substring(0, searchTextEdit.Caption.Length - 1);
                    return;
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "FilterMenuItemsShowGreyedOutTooltip");
                return;
            }
            try
            {
                mFilteredRoot = pieMenu.ValidateMenuStructure(newRoot);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "FilterMenuItemsValidateMenuStructure");
                return;
            }

            try
            {
                pieMenu.mCurrent = mFilteredRoot;
                pieMenu.SetupPieMenuButtons(mFilteredRoot, pieMenu.mPositionStack[pieMenu.mPositionStackPtr], false);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "FilterMenuItemsSetupPieMenuButtons");
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

        private void FlattenMenuItems(MenuItem root, List<MenuItem> items)
        {
            try
            {
                foreach (MenuItem child in root.mChildren)
                {
                    items.Add(child);
                    FlattenMenuItems(child, items);
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "FilterMenuItems");
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
    }
}