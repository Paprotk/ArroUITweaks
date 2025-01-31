using System;
using System.Collections.Generic;
using MonoPatcherLib;
using Sims3.Gameplay;
using Sims3.SimIFace;
using Sims3.UI;

namespace Arro.RecoverNotification
{
    public class CustomCommandConsoleDialog : ModalDialog
    {
        private static readonly List<string> sCommandHistory = new List<string>();
        private static int sHistoryIndex = -1;
        private const int kMaxHistorySize = 50;

        [ReplaceMethod(typeof(CommandConsoleDialog), "Show")]
        public static string ShowPatch()
        {
            string result = null;
            if (sDialog == null)
            {
                using (sDialog = new CustomCommandConsoleDialog())
                {
                    sDialog.StartModal();
                    result = sDialog.Result;
                    sDialog = null;
                }
            }
            return result;
        }
        public CustomCommandConsoleDialog() : base("CustomCommandConsole", 4097, true, PauseMode.PauseSimulator, "CustomCommandConsoleTriggers")
        {
            if (mModalDialogWindow != null)
            {
                TextEdit textEdit = mModalDialogWindow.GetChildByID(1U, false) as TextEdit;
                if (textEdit != null)
                {
                    textEdit.Caption = "";
                    textEdit.TextChange += OnInputTextChanged;
                }

                TextEdit helpText = mModalDialogWindow.GetChildByID(2U, true) as TextEdit;
                if (helpText != null)
                {
                    helpText.Visible = true;
                    helpText.Caption = CommandSystem.GetCommandList();
                    AdjustHelpTextHeight();
                }

                mModalDialogWindow.Tick += OnTick;
                OkayID = 3U;
                CancelID = 4U;
                SelectedID = 3U;
                mModalDialogWindow.TriggerDown += OnTriggerDown;
            }
            sHistoryIndex = -1;
        }

        public void AdjustHelpTextHeight()
        {
            TextEdit helpText = mModalDialogWindow?.GetChildByID(2U, true) as TextEdit;
            if (helpText == null) return;

            string caption = helpText.Caption;
            string[] lines = caption.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            float lineHeight = 20f;
            float minHeight = 25f;
            float maxHeight = 720f;

            if (lines.Length == 0)
            {
                helpText.Visible = false;
            }
            else
            {
                helpText.Visible = true;
                float newHelpHeight = lines.Length * lineHeight;
                newHelpHeight = Math.Max(newHelpHeight, minHeight);
                newHelpHeight = Math.Min(newHelpHeight, maxHeight);
                Rect helpArea = helpText.Area;
                helpArea.Height = newHelpHeight;
                helpText.Area = helpArea;
            }
        }
        
#pragma warning disable CS0108, CS0114
        public void OnTriggerDown(WindowBase sender, UITriggerEventArgs eventArgs)
#pragma warning restore CS0108, CS0114
        {
            if (eventArgs.TriggerCode == 0x042b0005) // Tab key (NOP)
            {
                HandleTabKey();
                eventArgs.Handled = true;
                return;
            }
            else if (eventArgs.TriggerCode == 0x042b0006) // Up arrow (Previous)
            {
                HandleHistoryNavigation(-1);
                eventArgs.Handled = true;
                return;
            }
            else if (eventArgs.TriggerCode == 0x042b0007) // Down arrow (Next)
            {
                HandleHistoryNavigation(1);
                eventArgs.Handled = true;
                return;
            }
            base.OnTriggerDown(sender, eventArgs);
        }

        private void HandleHistoryNavigation(int direction)
        {
            TextEdit textEdit = mModalDialogWindow.GetChildByID(1U, false) as TextEdit;
            if (textEdit == null || sCommandHistory.Count == 0) return;

            int newIndex;

            if (direction < 0) // Up arrow (older commands)
            {
                newIndex = sHistoryIndex == -1 ? sCommandHistory.Count - 1 : Math.Max(0, sHistoryIndex - 1);
            }
            else // Down arrow (newer commands)
            {
                newIndex = sHistoryIndex == sCommandHistory.Count - 1 ? -1 : Math.Min(sCommandHistory.Count - 1, sHistoryIndex + 1);
            }

            if (newIndex != sHistoryIndex)
            {
                sHistoryIndex = newIndex;
                textEdit.Caption = sHistoryIndex >= 0 ? sCommandHistory[sHistoryIndex] : "";
                textEdit.CursorIndex = (uint)textEdit.Caption.Length;
                textEdit.AnchorIndex = (uint)textEdit.Caption.Length;
            }
        }

        public override void OnTriggerOk()
        {
            TextEdit textEdit = mModalDialogWindow.GetChildByID(1U, false) as TextEdit;
            if (textEdit != null)
            {
                mCommandString = textEdit.Caption;
            }

            if (!string.IsNullOrEmpty(mCommandString))
            {
                string[] array = mCommandString.Split(' ');
                if (array.Length != 0)
                {
                    string text = array[0].ToLower();
                    string text2 = null;

                    if (text == kHelpString)
                    {
                        if (StringTable.GetLocale() != "fr-fr")
                        {
                            if (array.Length > 1)
                            {
                                string commandHelp = CommandSystem.GetCommandHelp(array[1].ToLower());
                                text2 = string.IsNullOrEmpty(commandHelp) ? Responder.Instance.LocalizationModel.LocalizeString("Ui/Caption/Cheat:UnknownCommand", array[1]) : commandHelp;
                            }
                            else
                            {
                                text2 = CommandSystem.GetCommandList();
                            }
                        }
                    }
                    else if (text == kJokeString)
                    {
                        text2 = GetRandomJoke();
                    }
                    else if (string.IsNullOrEmpty(CommandSystem.GetCommandHelp(text)))
                    {
                        text2 = Responder.Instance.LocalizationModel.LocalizeString("Ui/Caption/Cheat:UnknownCommand", text);
                    }

                    TextEdit textEdit2 = mModalDialogWindow.GetChildByID(2U, true) as TextEdit;
                    if (textEdit2 != null && !string.IsNullOrEmpty(text2))
                    {
                        textEdit2.Visible = true;
                        textEdit2.Caption = text2;
                        textEdit2.CursorIndex = 0U;
                        textEdit2.AnchorIndex = 0U;
                        return;
                    }
                }
            }
            EndDialog(3U);
        }

        public override bool OnEnd(uint buttonID)
        {
            if (buttonID == 3U && !string.IsNullOrEmpty(mCommandString))
            {
                CommandSystem.ExecuteCommandString(mCommandString);

                // Add to command history
                if (sCommandHistory.Count == 0 || sCommandHistory[sCommandHistory.Count - 1] != mCommandString)
                {
                    sCommandHistory.Add(mCommandString);
                    if (sCommandHistory.Count > kMaxHistorySize)
                    {
                        sCommandHistory.RemoveAt(0);
                    }
                }
                sHistoryIndex = -1;
            }
            return true;
        }

        private void HandleTabKey()
        {
            TextEdit textEdit = this.mModalDialogWindow.GetChildByID(1U, false) as TextEdit;
            TextEdit helpText = this.mModalDialogWindow.GetChildByID(2U, true) as TextEdit;

            if (textEdit == null || helpText == null)
                return;

            string helpCaption = helpText.Caption;
            if (string.IsNullOrEmpty(helpCaption))
                return;

            string[] lines = helpCaption.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0)
                return;

            string firstLine = lines[0];
            string[] parts = firstLine.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return;

            string command = parts[0].Trim();
            textEdit.Caption = command;
            
            textEdit.CursorIndex = (uint)command.Length;
            textEdit.AnchorIndex = (uint)command.Length;
            
            this.OnInputTextChanged(textEdit, new UITextChangeEventArgs());
        }

        public void OnTick(WindowBase sender, UIEventArgs eventArgs)
		{
			TextEdit textEdit = this.mModalDialogWindow.GetChildByID(1U, false) as TextEdit;
			if (textEdit != null)
			{
				uint length = (uint)textEdit.Caption.Length;
				textEdit.AnchorIndex = length;
				textEdit.CursorIndex = length;
				UIManager.SetFocus(InputContext.kICKeyboard, textEdit);
				textEdit.HideCaret = false;
			}
			this.mModalDialogWindow.Tick -= this.OnTick;
		}

        public string Result => mCommandString;

        public static string GetRandomJoke()
        {
            return StringTable.GetLocalizedString("Gameplay/Core/Cheats:Joke" + (1 + (int)(10.0 * RandomGen.NextDouble())).ToString());
        }

        private void OnInputTextChanged(WindowBase sender, UIEventArgs e)
        {
            string[] inputParts = ((TextEdit)sender).Caption.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string commandFilter = (inputParts.Length != 0) ? inputParts[0].ToLower() : string.Empty;
            
            string[] originalCommands = CommandSystem.GetCommandList().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> allCommands = new List<string>(originalCommands);

            if (!GameStates.IsInMainMenuState)
            {
                allCommands.Add("motherlode - Adds $50000 to active household funds");
                allCommands.Add("kaching - Adds $1000 to active household funds");
            }
            allCommands.Add("testingcheatsenabled - Usage: testingCheatsEnabled [true|false]. Enables/disables testing cheats.");
            
            List<string> filteredLines = new List<string>();
            foreach (string line in allCommands)
            {
                string[] parts = line.Split(new[] { " - " }, StringSplitOptions.None);
                if (parts.Length >= 1 && parts[0].Trim().ToLower().StartsWith(commandFilter))
                {
                    filteredLines.Add(line);
                }
            }
            
            TextEdit helpText = mModalDialogWindow.GetChildByID(2U, true) as TextEdit;
            if (helpText != null)
            {
                helpText.Caption = string.Join("\n", filteredLines.ToArray());
                helpText.Visible = true;
                AdjustHelpTextHeight();
            }
        }
        public static readonly string kHelpString = "help";
        public static readonly string kJokeString = "jokeplease";
        public string mCommandString;
        public static CustomCommandConsoleDialog sDialog;
    }
}