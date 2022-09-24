using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC
{
    public class FinishInitializeEventArgs : EventArgs
    {
        #region Private Fields

        private bool isInitializeSuccessful = false;

        #endregion

        #region Public Constructor

        public FinishInitializeEventArgs(bool initializeSuccessful)
        {
            isInitializeSuccessful = initializeSuccessful;
        }

        #endregion

        #region Public Properties

        public bool IsInitializeSuccessful
        {
            get { return isInitializeSuccessful; }
            set { isInitializeSuccessful = value; }
        }

        #endregion
    }
}
