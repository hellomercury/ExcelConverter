using LitJson;

namespace ExcelConverter.Json.DataTemplate
{
    public class Item
    {
        public int ID;
        public string Name;
        public string Icon;
        public int PileUpperLimit;
        public int ItemBuyInfo;
        public int UnlockInfo;

        public Item()
        {

        }

        public Item(JsonData InSingleData)
        {
            ID = (int)InSingleData["ID"];
            Name = (string)InSingleData["Name"];
            Icon = (string)InSingleData["Icon"];
            PileUpperLimit = (int)InSingleData["PileUpperLimit"];
            ItemBuyInfo = (int)InSingleData["ItemBuyInfo"];
            UnlockInfo = (int)InSingleData["UnlockInfo"];
        }

        public override string ToString()
        {
            return "DataTemplate    Item"
                + "\nID = " + ID
                + "\nName = " + Name
                + "\nIcon = " + Icon
                + "\nPileUpperLimit = " + PileUpperLimit
                + "\nItemBuyInfo = " + ItemBuyInfo
                + "\nUnlockInfo = " + UnlockInfo;
        }
    }
}

