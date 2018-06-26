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

        public Item(JsonData InJsonArray)
        {
            ID = (int)InJsonArray[0];
            Name = (string)InJsonArray[1];
            Icon = (string)InJsonArray[2];
            PileUpperLimit = (int)InJsonArray[3];
            ItemBuyInfo = (int)InJsonArray[4];
            UnlockInfo = (int)InJsonArray[5];
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

