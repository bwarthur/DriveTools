using System;

namespace DriveTools.Events
{
    public delegate void GetAllFilesStatusEventHandler(object source, GetAllFilesStatusEventHandlerArgs args);
    
    public static class EventHandlers
    {
        public static event GetAllFilesStatusEventHandler GetAllFilesStatusEvent;

        public static void GetAllFilesStatusEventTrigger(object source, GetAllFilesStatusEventHandlerArgs args)
        {
            if (GetAllFilesStatusEvent != null) GetAllFilesStatusEvent(source, args);
        }
    }

    public class GetAllFilesStatusEventHandlerArgs : EventArgs
    {
        public int CurrentFileCount { get; set; }
    }
}
