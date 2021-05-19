using UnityEngine;
namespace Aryzon.UI {
    public class EnumFlagAttribute : PropertyAttribute
    {
        public int columnCount;
        public EnumFlagAttribute(int rowCount)
        {
            this.columnCount = rowCount;
        }
    }
}