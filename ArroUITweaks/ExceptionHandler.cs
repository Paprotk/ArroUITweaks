using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;

namespace Arro.UITweaks
{
    public class ExceptionHandler
    {
        private readonly string _functionErrorName;
        private readonly Exception _exception;
        
        public ExceptionHandler(string functionName, Exception ex)
        {
            _functionErrorName = functionName;
            _exception = ex;
        }
        
        public static void WriteErrorXMLFile(string fileName, Exception errorToPrint)
        {
            uint num = 0u;
            // ReSharper disable once UnusedVariable
            string s = Simulator.CreateExportFile(ref num, fileName);

            if (num != 0)
            {
                CustomXmlWriter customXmlWriter = new CustomXmlWriter(num);
                customXmlWriter.WriteToBuffer(errorToPrint.ToString());
                customXmlWriter.WriteEndDocument();
            }
        }

        // This method is called when an exception is thrown.
        public static void HandleException(Exception ex, string functionName)
        {
            // Create a new ExceptionHandler instance with the specific error details.
            var handler = new ExceptionHandler(functionName, ex);
            handler.ShowButtonNotification();
        }
        
        public void ShowButtonNotification()
        {
            string titleText = "Error occurred while executing " + _functionErrorName + ". Click the button below to save exception info to The Sims 3 folder.";
            StyledNotification.Format format = new StyledNotification.Format(
                titleText,
                "=^..^=",
                ButtonCallback,
                StyledNotification.NotificationStyle.kSystemMessage
            )
            {
                mCloseOnCallback = true
            };
            StyledNotification.Show(format, "arro_error_icon");
        }

        // The callback method for the button. Saves the error to an XML file.
        private void ButtonCallback()
        {
            // Use the instance-specific error data to save it.
            WriteErrorXMLFile(_functionErrorName + "_error", _exception);
        }
    }
}
