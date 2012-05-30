using System;
using System.Collections.Generic;
using System.Text;

namespace SierraLib.Analytics.GoogleAnalytics
{
    public class Item
    {
        public Item(string orderID, string itemSKU, double itemPrice, long itemCount, string itemName = "", string itemCategory = "")
        {
            if (orderID == null || orderID.Trim().Length == 0)
                throw new ArgumentException("OrderID must not be null or empty", "orderID");
            if (itemSKU == null || itemSKU.Trim().Length == 0)
                throw new ArgumentException("ItemSKU must not be null or empty", "itemSKU");

            OrderID = orderID;
            ItemSKU = itemSKU;
            ItemName = itemName;
            ItemCategory = itemCategory;
            ItemPrice = itemPrice;
            ItemCount = itemCount;
        }

        public string OrderID
        { get; private set; }

        public string ItemSKU
        { get; private set; }

        public string ItemName
        { get; private set; }

        public string ItemCategory
        { get; private set; }

        public double ItemPrice
        { get; private set; }

        public long ItemCount
        { get; private set; }
    }
}
