using System;

namespace SC
{
    public class SessionStartedEventArgs : EventArgs
    {
        #region Private Fields

        private bool isSessionStarted = false;

        #endregion

        #region Public Constructor

        public SessionStartedEventArgs(bool success)
        {
            isSessionStarted = success;
        }

        #endregion

        #region Public Properties

        public bool IsSessionStarted
        {
            get { return isSessionStarted; }
            set { isSessionStarted = value; }
        }

        #endregion
    }
}
