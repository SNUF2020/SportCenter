using System;

namespace SC
{
    public class FinishFindDevicesEventArgs : EventArgs
    {
        #region Private Fields

        private int noOfDevicesFound = 0;

        #endregion

        #region Public Constructor

        public FinishFindDevicesEventArgs(int noOfDevicesFound)
        {
            this.noOfDevicesFound = noOfDevicesFound;
        }

        #endregion

        #region Public Properties

        public int NoOfDevicesFound
        {
            get { return noOfDevicesFound; }
            set { noOfDevicesFound = value; }
        }

        #endregion
    }
}
