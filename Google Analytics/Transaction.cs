using System;
using System.Collections.Generic;
using System.Text;

namespace SierraLib.Analytics.GoogleAnalytics
{
    public class Transaction
    {
        public Transaction(string orderID, double totalCost, string storeName = "", 
            double totalTax = 0, double shippingCost = 0)
        {
            if (orderID == null || orderID.Trim().Length == 0)
                throw new ArgumentException("OrderID must not be null or empty", "orderID");

            OrderID = orderID;
            TotalCost = totalCost;
            StoreName = storeName;
            TotalTax = totalTax;
            ShippingCost = shippingCost;
        }


        public string OrderID
        { get; private set; }

        public string StoreName
        { get; private set; }

        public double TotalCost
        { get; private set; }

        public double TotalTax
        { get; private set; }

        public double ShippingCost
        { get; private set; }
    }
}
