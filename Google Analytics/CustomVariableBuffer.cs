using System;
using System.Collections.Generic;
using System.Text;

namespace SierraLib.Analytics.GoogleAnalytics
{
    public class CustomVariableBuffer
    {
        private CustomVariable[] customVariables = new CustomVariable[5];

        public CustomVariable[] Variables
        { get { return customVariables; } }


        public CustomVariable this[int index]
        {
            get
            {
                checkIndex(index);
                return customVariables[index - 1];
            }

            set
            {
                checkIndex(index);
                customVariables[index - 1] = value;
            }
        }

        public bool IndexAvailable(int index)
        {
            checkIndex(index);
            return customVariables[index - 1] == null;
        }
        
        public bool HasVariables
        {
            get
            {
                foreach (CustomVariable variable in customVariables)
                    if (variable != null)
                        return true;
                return false;
            }
        }

        public void ClearVariableAt(int index)
        {
            checkIndex(index);
            customVariables[index - 1] = null;
        }

        private void checkIndex(int index)
        {
            if (index < 1 || index > 5)
                throw new ArgumentOutOfRangeException("Index should be between 1 and 5, inclusive");
        }
    }
}
