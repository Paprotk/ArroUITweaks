using System;
using System.Collections.Generic;
using MonoPatcherLib;
using Sims3.SimIFace;
using Sims3.UI;
using Sims3.UI.Hud;

namespace Arro.UITweaks
{
    public class PieMenuPatch
    {
        [ReplaceMethod(typeof(PieMenu), "SetupPieMenuButtons")]
        public void SetupPieMenuButtons(MenuItem menu, Vector2 origin, bool returnButtonVisible)
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

            for (num2 = 0U; num2 < childCount; num2 += 1U)
            {
                uint num6 = instance.mButtonIndices[(int)((UIntPtr)num2)];
                instance.mItemButtons[(int)((UIntPtr)num6)].Visible = true;
            }

            if (instance.mCurrent == instance.mTree.mRoot)
            {
                if (instance.mHeadObjectGuid.IsValid && instance.mHeadSceneWindow != null)
                {
                    instance.mHeadSceneWindow.Visible = true;
                }
            }
            else
            {
                if (instance.mHeadSceneWindow != null)
                {
                    instance.mHeadSceneWindow.Visible = false;
                }
            }
        }
        [ReplaceMethod(typeof(PieMenu), "Begin")]
        public void Begin(MenuTree tree, Vector2 location)
        {
            var instance = (PieMenu)(this as object);
            instance.mTriggerHandle = instance.mContainer.AddTriggerHook("piemenu", TriggerActivationMode.kPermanent, 1);
            instance.mContainer.TriggerDown += instance.OnTriggerDown;
            instance.mTree = tree;
            instance.mCurrent = instance.ValidateMenuStructure(tree.mRoot);
            instance.SetupPieMenuButtons(instance.mCurrent, location, false);
            instance.mCurrentButtonSelected = -1;
            instance.mContainer.Visible = true;
            Button searchButton = (instance.GetChildByID(185745581U, true) as Button);
            searchButton.Click += OnClickShowSearchDialog;
            Audio.StartSound("ui_piemenu_primary");
            UIManager.PushModal(instance);
        }
        [ReplaceMethod(typeof(PieMenu), "End")]
        public void End()
        {
            var instance = (PieMenu)(this as object);
            instance.mContainer.RemoveTriggerHook(instance.mTriggerHandle);
            instance.mContainer.TriggerDown -= instance.OnTriggerDown;
            instance.mTree = null;
            instance.mCurrent = null;
            Button searchButton = (instance.GetChildByID(185745581U, true) as Button);
            searchButton.Click -= OnClickShowSearchDialog;
            instance.mContainer.Visible = false;
            instance.mPositionStackPtr = -1;
            instance.mReturnButtonStackPtr = -1;
            instance.mCurrentButtonSelected = -1;
            instance.ResetSimHead();
            UIManager.PopModal(instance);
        }

        private void OnClickShowSearchDialog(WindowBase sender, UIButtonClickEventArgs args)
        {
            try
            {
                var pieMenu = (PieMenu)(this as object);
                Simulator.AddObject(new OneShotFunctionTask(() => { ShowSearchDialog(pieMenu); },
                    StopWatch.TickStyles.Seconds, 0.1f));
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "OnClickShowSearchDialog");
            }
        }


        private void ShowSearchDialog(PieMenu pieMenu)
        {
            try
            {
                string result = StringInputDialog.Show("Search Interactions", "Enter search term:", "", false);
                if (!string.IsNullOrEmpty(result))
                {
                    FilterMenuItems(pieMenu, result);
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "ShowSearchDialog");
            }
        }
        

        private void FilterMenuItems(PieMenu pieMenu, string query)
        {
            try
            {
                MenuItem originalRoot = pieMenu.mTree.mRoot;
                List<MenuItem> allItems = new List<MenuItem>();
                FlattenMenuItems(originalRoot, allItems);

                // Create new root and assign the mTree from the pieMenu's tree
                MenuItem newRoot = new MenuItem();
                newRoot.mTree = pieMenu.mTree; // Fix: Set mTree before adding children

                foreach (MenuItem item in allItems)
                {
                    if (item.mName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        MenuItem newItem = new MenuItem
                        {
                            mName = item.mName,
                            mTag = item.mTag,
                            mStyle = item.mStyle,
                            mTree = pieMenu.mTree // Ensure each new item's mTree is set
                        };
                        newRoot.AddChild(newItem);
                    }
                }

                pieMenu.mCurrent = newRoot;
                pieMenu.SetupPieMenuButtons(newRoot, pieMenu.mPositionStack[pieMenu.mPositionStackPtr], false);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "FilterMenuItems");
            }
        }

        private void FlattenMenuItems(MenuItem root, List<MenuItem> items)
        {
            try
            {
                foreach (MenuItem child in root.mChildren)
                {
                    items.Add(child);
                    FlattenMenuItems(child, items); // Recursive call
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "FilterMenuItems");
            }
        }
    }
}