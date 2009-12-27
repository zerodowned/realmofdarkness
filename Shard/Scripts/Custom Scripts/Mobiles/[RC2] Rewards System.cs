//Author: krazeykow
//Version: 1.0.1

using System;
using Server;
using Server.Items;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Server.Gumps;
using Server.Network;
using Server.Targeting;
using Server.Mobiles;
using Server.ContextMenus;


public class Currency2 : Item
{

    private Payment m_PaymentType;
    private PropertyInfo m_PropertyInfo;
    private Type m_Type;
    private int m_ItemID;
    private string m_Name;
    private int m_Hue;

    public Type PaymentType { get { return m_Type; } }
    public string PayName { get { return m_Name; } }
    public int PayID { get { return m_ItemID; } }
    public int CurrHue { get { return m_Hue; } } 

    public enum Payment
    {
        ByGold,
        ByItem,
        ByProperty
    }
    //default
    public Currency2()
    {
        m_PaymentType = Payment.ByGold;
        m_Type = typeof(Gold);
        m_PropertyInfo = null;
        m_ItemID = 3823;
        m_Name = "Gold";
    }
    //consume by item
    public Currency2(Item i)
    {
        if ( i.GetType().Equals(typeof(Gold)) )
            m_PaymentType = Payment.ByGold;
        else
            m_PaymentType = Payment.ByItem;

        m_Type = i.GetType();
        m_PropertyInfo = null;
        m_ItemID = CoinID(i.ItemID);
        m_Name = GetName(i);
        m_Hue = i.Hue;
    }
    //consume property on player
    public Currency2(Mobile m, PropertyInfo pi)
    {
        m_PaymentType = Payment.ByProperty;
        m_Type = m.GetType();
        m_PropertyInfo = pi;
        m_ItemID = 8461;
        m_Name = "Player " + pi.Name;
    }
    //consume by item's number based property
    public Currency2(Item i, PropertyInfo pi)
    {
        m_PaymentType = Payment.ByProperty;
        m_Type = i.GetType();
        m_PropertyInfo = pi;
        m_ItemID = CoinID(i.ItemID);
        m_Name = GetName(i) + "'s " + pi.Name;
        m_Hue = i.Hue;
    }
    public void Modify(Type t, string append)
    {
        if ( (t is Mobile) || append == null )
            return;

        Item i;
        
        i = (Item)Activator.CreateInstance(t, null);
        m_Name = GetName(i) + append;
        m_Hue = i.Hue;
        m_ItemID = CoinID(m_ItemID);
        
    }
    public int CoinID(int itemID)
    {
        if (itemID == 3821)
            return 3823;
        else if (itemID == 3824)
            return 3826;
        else
            return itemID;
    }
    public void VOneUpdate()
    {
        switch (m_PaymentType)
        {
            case Payment.ByGold:
            case Payment.ByItem:
                {
                    Modify(m_Type, "");
                    break;
                }
            case Payment.ByProperty:
                {
                    if (!(m_Type is Mobile))
                        Modify(m_Type, "'s " + m_PropertyInfo.Name);
                    break;
                }
        }

     }  
                
    public string GetName(Item i)
    {
        if (String.IsNullOrEmpty(i.Name))
            return i.GetType().Name;
        else
            return i.Name;
    }
    public bool Purchase(Mobile m, int cost)
    {
        int remain = Value(m) - cost;

        if (remain < 0 || m.Backpack == null)
            return false;
        
        Charge(m, cost, remain);

        return true;
    }
    public void Charge(Container pack, Container bank, Type t, int cost)
    {
        if ( pack == null || bank == null )
            return;

        cost = cost - pack.ConsumeUpTo(t, cost);

        if ( cost > 0 )
            bank.ConsumeUpTo(t, cost);
    }
   
    public void Charge(Mobile m, Type t, PropertyInfo pi, int cost, int remain)
    {
        try
        {
            if (t.BaseType.Equals(typeof(Mobile)) || t.Equals(typeof(PlayerMobile)))
            {
                pi.SetValue(m, remain, null);
            }
            else
            {
                if (m.Backpack == null || m.FindBankNoCreate() == null)
                    return;

                Item[] packItems = m.Backpack.FindItemsByType(t);
                Item[] bankItems = m.FindBankNoCreate().FindItemsByType(t);

                Item[] items = new Item[packItems.Length + bankItems.Length];
                packItems.CopyTo(items, 0);
                bankItems.CopyTo(items, packItems.Length);

                //none found in backpack
                if (items == null)
                    return;

                int i = 0, value = 0, dif = 0;

                //account for multiple objects
                while (cost > 0 && i < items.Length)
                {
                    value = (int)pi.GetValue(items[i], null);
                    dif = value - cost;

                    //one item has enough
                    if (dif >= 0)
                    {
                        pi.SetValue(items[i], dif, null);
                    }
                    //take what amount item has (the amount has already been confirmed by Value(m))
                    else // assert (dif < 0)
                    {
                        cost -= (int)pi.GetValue(items[i], null);
                        pi.SetValue(items[i], 0, null);
                    }
                    i++;
                }
            }
        }
        catch { }
    }
    public void Charge(Mobile m, Container pack, Container bank, int cost)
    {
        if ( pack == null || bank == null )
            return;

        cost = cost - pack.ConsumeUpTo(typeof(Gold), cost);

        if (cost > 0)
            Banker.Withdraw(m, cost);
    }
    public void Charge(Mobile m, int cost, int remain)
    {
        switch (m_PaymentType)
        {
            case Payment.ByGold:
                Charge(m, m.Backpack, m.FindBankNoCreate(), cost);
                break;
            case Payment.ByItem:
                Charge(m.Backpack, m.FindBankNoCreate(), m_Type, cost);
                break;

            case Payment.ByProperty:
                Charge(m, m_Type, m_PropertyInfo, cost, remain);
                break;
        }
    }
    public int Value(Mobile m)
    {
        switch (m_PaymentType)
        {
            case Payment.ByGold:
                return ProcessByGold(m);
            case Payment.ByItem:
                return ProcessByItem(m, m_Type);

            case Payment.ByProperty:
                return ProcessByProperty(m, m_Type, m_PropertyInfo);
        }
        return 0;
    }
    public int ProcessByGold(Mobile m)
    {
        if (m.Backpack == null)
            return 0;

        int totalGold = 0;

        totalGold += m.Backpack.GetAmount( typeof( Gold ) );
        totalGold += Banker.GetBalance( m );

        return totalGold;
    }
    public int ProcessByItem(Mobile m, Type t)
    {
        Container pack = m.Backpack;
        Container bank = m.FindBankNoCreate();

        if (pack == null || bank == null)
            return 0;

        int amount = (pack.GetAmount(t) + bank.GetAmount(t));

        return amount;
    }
    public int ProcessByProperty(Container pack, Container bank, Type t, PropertyInfo pi)
    {
        if (pack == null || bank == null)
            return 0;


        Item[] packItems = pack.FindItemsByType(t);
        Item[] bankItems = bank.FindItemsByType(t);

        Item[] items = new Item[packItems.Length + bankItems.Length];
        packItems.CopyTo(items, 0);
        bankItems.CopyTo(items, packItems.Length);
        
        int sum = 0;

        foreach (object o in items)
        {
            sum += ((int)pi.GetValue(o, null));
        }

        return sum;
    }
    public int ProcessByProperty(Mobile m, Type t, PropertyInfo pi)
    {
        try
        {
            if (t.BaseType.Equals(typeof(Mobile)) || t.Equals(typeof(Mobile)) )
            {
                return (int)pi.GetValue(m, null);
            }
            else
            {
                return ProcessByProperty(m.Backpack, m.FindBankNoCreate(), t, pi);
            }
        }
        catch {}
        
        return 0;
        
    }
   
    public Currency2( Serial serial ) : base( serial )
	{
	}
	public override void Serialize( GenericWriter writer )
	{
		base.Serialize( writer );
		writer.Write( (int) 1 );

        //version 1
        writer.Write((int)m_Hue);
        
        //explicit casts are given for clarification        
        writer.Write((int)m_PaymentType);
        writer.Write((string)m_Type.Name);
        writer.Write((string)(m_PropertyInfo == null ? "" : m_PropertyInfo.Name));
        writer.Write((int)m_ItemID);
        writer.Write((string)m_Name);
        
	}
	
	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize( reader );
		int version = reader.ReadInt();

        

        switch (version)
        {
            case 1:
                {
                    m_Hue = reader.ReadInt();
                    goto case 0;
                }
            case 0:
                {
                    m_PaymentType = (Payment)reader.ReadInt();

                    try { string name = reader.ReadString(); m_Type = ScriptCompiler.FindTypeByName(name); }
                    catch { }

                    try { string propName = reader.ReadString(); if (propName.Equals("")) { m_PropertyInfo = null; } else { m_PropertyInfo = m_Type.GetProperty(propName); } }
                    catch { }

                    m_ItemID = reader.ReadInt();
                    m_Name = reader.ReadString();

                    break;
                }
        }
        if (version == 0)
        {
            try { VOneUpdate(); }
            catch {  }
        }
     }
}

public class Reward2 : Item, ICloneable
{      
    private Item m_Reward2;
    private int m_Cost;
    private string m_Title;
    private string m_Description;

    private ObjectPropertyList m_Display;

    private RestockInfo m_RestockInfo;
    private int m_BuyCount;

    public int BuyCount { get { return m_BuyCount; } set { m_BuyCount = value; } }
    public RestockInfo Restock { get { return m_RestockInfo; } set { m_RestockInfo = value; } }

    public ObjectPropertyList Display
    {
        get
        {
            if (m_Display == null)
            {
                m_Display = new ObjectPropertyList(this);

                m_Reward2.GetProperties(m_Display);
                m_Reward2.AppendChildProperties(m_Display);

                m_Display.Terminate();
                m_Display.SetStatic();
            }

            return m_Display;
        }
    }

    public void Void_RestockInfo()
    {
        m_RestockInfo = null;
    }
    //returns ((-1) -> no restockInfo) or current count
    public int Try_Restock()
    {
        if (m_RestockInfo == null)
            return -1;        

        TimeSpan dif = DateTime.Now - m_RestockInfo.LastRestock;
        int cycles = 0;

        if (dif > (m_RestockInfo.RestockRate) && m_RestockInfo.RestockRate.TotalMinutes > 0)
        {
            cycles = (int)(dif.TotalMinutes / m_RestockInfo.RestockRate.TotalMinutes);

            for (int i = 0; i < cycles; ++i)
            {
                if (m_RestockInfo.Restock())
                    continue;
            }
        }

        return m_RestockInfo.Count;
    }
    public bool InStock(int amount)
    {
        return (m_RestockInfo == null ? true : (m_RestockInfo.Count - amount >= 0));       
    }
    public void RegisterBuy(int amount)
    {      
        if (m_RestockInfo != null && InStock(amount))
            m_RestockInfo.Count -= amount;

        this.m_BuyCount += amount;
    }

    //for retrieving information only
    public Item Reward2Info
    {
        get { return m_Reward2; }
    }
    //use only when creating Reward2 for player
    public Item Reward2Copy
    {
        get { return GetReward2(); }
    }
    public int Cost
    {
        get { return m_Cost; }
        set { m_Cost = value; }
    }
    public string Title
    {
        get { return m_Title; }
        set { m_Title = value; }
    }
    public string Description
    {
        get { return m_Description; }
        set { m_Description = value; }
    }
    object ICloneable.Clone()
    {
        return new Reward2(this);
    }
    public Reward2()
    {
        m_Reward2 = new Item();
        m_Cost = 0;
        m_Title = "";
        m_Description = "";
        m_RestockInfo = null;
    }
    public Reward2(Reward2 r)
    {
        m_Reward2 = r.Reward2Copy;
        m_Cost = r.Cost;
        m_Title = r.Title;
        m_Description = r.Description;
        m_RestockInfo = null;
    }
    public Reward2(Item i)
    {
        m_Reward2 = i;
        m_Cost = 0;
        m_Title = (i.Name == null ? i.GetType().Name : i.Name);
        m_Description = "None.";
        m_RestockInfo = null;

    }
    public Reward2(Item i, int cost)
    {
        m_Reward2 = i;
        m_Cost = cost;
        m_Title = (i.Name == null ? i.GetType().Name : i.Name);
        m_Description = "None.";
        m_RestockInfo = null;
    }
    public Reward2(Item i, int cost, string title)
    {
        m_Reward2 = i;
        m_Cost = cost;
        m_Title = title;
        m_Description = "None.";
        m_RestockInfo = null;
    }
    public Reward2(Item i, int cost, string title, string desc)
    {
        m_Reward2 = i;
        m_Cost = cost;
        m_Title = title;
        m_Description = desc;
        m_RestockInfo = null;
    }

    public class RestockInfo 
    {
        private TimeSpan m_RestockRate;
        private DateTime m_LastRestock;

        private int m_RestockAmnt;
        private int m_Count;
        private int? m_Maximum;
        
        public TimeSpan RestockRate { get { return m_RestockRate; } }
        public DateTime LastRestock { get { return m_LastRestock; } }

        public int RestockAmnt { get { return m_RestockAmnt; } }
        public int Count { get { return m_Count; } set { m_Count = value; } }
        public int Maximum { get { return m_Maximum.GetValueOrDefault(-1); } }

        public int Hours { get { return (int)m_RestockRate.Hours; } }
        public int Minutes { get { return (int)m_RestockRate.Minutes; } }

        public RestockInfo()
        {
            m_RestockRate = new TimeSpan();
            m_RestockAmnt = 0;
            m_Count = 0;
            m_Maximum = null;
            m_LastRestock = DateTime.Now;

        }
        public RestockInfo(int count) : this()
        {
            m_Count = count;
        }
        public RestockInfo(int hour, int minute, int count, int RestockNum)
        {
            m_RestockRate = new TimeSpan(hour, minute, 0);
            m_Count = count;
            m_RestockAmnt = RestockNum;
            m_Maximum = null;
            m_LastRestock = DateTime.Now;

        }
        public RestockInfo(int hour, int minute, int count, int RestockNum, int maxNum) : this (hour, minute, count, RestockNum)
        {
            m_Maximum = maxNum;
        }
        public bool Equals(RestockInfo info)
        {
            return (this.m_RestockRate.Equals(info.RestockRate) && (this.m_RestockAmnt == info.RestockAmnt) && (this.m_Count == info.Count)
                && (this.m_Maximum == info.Maximum));
        }
        public bool Restock()
        {
            m_LastRestock = DateTime.Now;

            if (m_Maximum != null) //Restock to a limit
            {
                if ((m_Count + m_RestockAmnt) > m_Maximum)
                {
                    m_Count = m_Maximum.Value;
                    return false;
                }

                m_Count += m_RestockAmnt;
                return true;
            }
            
            m_Count += m_RestockAmnt;            
            return true;
        }

        public void Serialize( GenericWriter writer )
        {
            writer.Write((TimeSpan)m_RestockRate);           
            writer.Write((int)m_RestockAmnt);
            writer.Write((int)m_Count);
            writer.Write((int)m_Maximum.GetValueOrDefault(-1));
        }

        public RestockInfo( GenericReader reader )
        {
            m_RestockRate = reader.ReadTimeSpan();
            m_LastRestock = DateTime.Now;
            m_RestockAmnt = reader.ReadInt();
            m_Count = reader.ReadInt();
            
            int? max = reader.ReadInt();
         
            m_Maximum = ((max < 0) ? null : max);
        }
    }

    public void Edit(int cost, string title, string desc)
    {
        if (cost < 0 || title == null || desc == null)
            return;

        m_Cost = cost;
        m_Title = title;
        m_Description = desc;
    }
    public Item GetReward2()
    {
        return ItemClone2.Clone(m_Reward2);
    }

    public Reward2( Serial serial ) : base( serial )
	{
	}
	public override void Serialize( GenericWriter writer )
	{
		base.Serialize( writer );

		writer.Write( (int) 1 );

        writer.Write((int)m_BuyCount);

        if (m_RestockInfo == null)
        {
            writer.Write(false);
        }
        else
        {
            writer.Write(true);
            m_RestockInfo.Serialize(writer);
        }

        writer.WriteItem((Item)m_Reward2);
        writer.Write((int)m_Cost);
        writer.Write((string)m_Title);
        writer.Write((string)m_Description);          
    }
	
	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize( reader );
		int version = reader.ReadInt();

        switch (version)
        {
            case 1:
                {
                    m_BuyCount = reader.ReadInt();

                    if (reader.ReadBool())
                        m_RestockInfo = new RestockInfo(reader);

                    goto case 0;
                }
            case 0:
                {
                    m_Reward2 = (Item)reader.ReadItem();
                    m_Cost = reader.ReadInt();
                    m_Title = reader.ReadString();
                    m_Description = reader.ReadString();                  
                    
                    break;
                }
        }
    
	}

}
public class Reward2Collection : List<Reward2>, ICloneable
{
    

    object ICloneable.Clone()
    {
        Reward2Collection clone = new Reward2Collection();
        
        foreach (Reward2 r in this)
        {
            clone.Add(((Reward2)(r as ICloneable).Clone()));
        }
        return clone;
    }
}

public static class WorldReward2Vendors
{
    private static List<IReward2Vendor> m_Vendors = new List<IReward2Vendor>();

    public static List<IReward2Vendor> Vendors { get { return m_Vendors; } }
   
    public static void RegisterVendor(IReward2Vendor vendor)
    {
        m_Vendors.Add(vendor);
    }
    public static void RemoveVendor(IReward2Vendor vendor)
    {
        m_Vendors.Remove(vendor);
    }
}

public class EditItemGump2 : Gump
{
    private IReward2Vendor m_Vendor;
    private Reward2 m_Reward2;

    private Reward2.RestockInfo m_Restock;
    private bool m_RestockOpen;

    private bool m_IsAdd;

    private Storage2 m_Entries;

    public EditItemGump2(IReward2Vendor vendor, Reward2 r, Storage2 entries, bool openRestockInfo, bool addingItem) : base(0, 0)
    {
        m_Vendor = vendor;
        m_Reward2 = r;

        m_Restock = r.Restock;
        m_RestockOpen = openRestockInfo;
        m_IsAdd = addingItem;

        m_Entries = entries; 

        this.Closable = true;
        this.Disposable = true;
        this.Dragable = true;
        this.Resizable = false;

        AddPage(0);

        AddImageTiled(227, 46, 329, 518, 2702);
        AddImageTiled(252, 291, 280, 217, 5104);
        AddLabel(253, 266, 2124, @"Description:");
        AddImageTiled(334, 147, 192, 23, 5104);
        AddLabel(258, 150, 2124, @"Cost:");
        AddImageTiled(336, 107, 192, 23, 5104);
        AddLabel(258, 108, 2124, @"Name:");
        AddImage(354, 199, 2328);
        AddItem(373, 208, r.Reward2Info.ItemID, r.Reward2Info.Hue);
        AddImageTiled(330, 54, 138, 16, 5214);
        AddLabel(373, 51, 1324, @"Edit Item");
        AddButton(465, 222, 5601, 5605, 2, GumpButtonType.Reply, 0); //restock
        AddLabel(447, 198, 2120, @"Restock ");
        AddLabel(447, 246, 2120, @"(Optional)");
        AddButton(324, 523, 4023, 4024, 1, GumpButtonType.Reply, 0);//ok
        AddButton(421, 523, 4017, 4018, 0, GumpButtonType.Reply, 0); //cancel

        if (entries == null)
            InitialText();
        else
            PacketText();

        if (openRestockInfo || r.Restock != null)
        {
            AddRestockMenu();
            m_RestockOpen = true;
        }
    }
    public void InitialText()
    {
        AddTextEntry(252, 292, 277, 215, 1879, 0, m_Reward2.Description);
        AddTextEntry(334, 148, 188, 20, 1879, 1, m_Reward2.Cost.ToString());
        AddTextEntry(336, 108, 188, 20, 1879, 2, m_Reward2.Title);    
    }
    public void PacketText()
    {
        AddTextEntry(252, 292, 277, 215, 1879, 0, m_Entries[0].ToString()); //desc
        AddTextEntry(334, 148, 188, 20, 1879, 1, m_Entries[1].ToString());  //cost 
        AddTextEntry(336, 108, 188, 20, 1879, 2, m_Entries[2].ToString());  //title
    }
    public void AddRestockMenu()
    {

        AddPage(1);

        AddImageTiled(601, 47, 181, 469, 2624);
        AddLabel(648, 60, 2129, @"Restock Info");
        AddLabel(637, 98, 1418, @"Available");
        AddImageTiled(633, 123, 120, 16, 5214);
        AddTextEntry(637, 121, 107, 20, 547, 3, (m_Restock == null ? "Enter." : m_Restock.Count.ToString())); //3 - beginning amount
        AddLabel(660, 161, 1418, @"Maximum");
        AddImageTiled(631, 186, 120, 16, 5214);
        AddTextEntry(635, 184, 107, 20, 547, 4, ((m_Restock == null || m_Restock.Maximum < 0) ? "None." : m_Restock.Maximum.ToString())); //4 - Maxinum
        AddLabel(648, 227, 1418, @"Restock Rate");
        AddImageTiled(635, 252, 72, 16, 5214);
        AddTextEntry(638, 250, 64, 20, 547, 5, (m_Restock == null ? "0" : m_Restock.Hours.ToString())); //5 - hour
        AddLabel(714, 250, 602, @"hours");
        AddImageTiled(635, 292, 72, 16, 5214);
        AddLabel(716, 290, 602, @"mins");
        AddLabel(632, 323, 602, @"amount to restock");
        AddImageTiled(636, 355, 72, 16, 5214);
        AddTextEntry(638, 290, 64, 20, 547, 6, (m_Restock == null ? "0" : m_Restock.Minutes.ToString())); //6 - minute
        AddTextEntry(639, 353, 64, 20, 547, 7, (m_Restock == null ? "0" : m_Restock.RestockAmnt.ToString())); //7 - Restock Amount      
        AddImage(655, 377, 109);
        AddButton(636, 474, 4022, 4021, 3, GumpButtonType.Reply, 0);
        AddLabel(679, 475, 132, @"Void Restock");
        
    }
    public Reward2.RestockInfo GetRestockInfo(RelayInfo info)
    {
        if (m_RestockOpen)
        {
            int numMinutes = 0, numHours = 0;
            int numBegin, numMax, numRestockAmnt;

            TextRelay entry3 = info.GetTextEntry(3);
            string beginAmount = (entry3.Text.Trim());

            TextRelay entry4 = info.GetTextEntry(4);
            string max = (entry4.Text.Trim());

            TextRelay entry5 = info.GetTextEntry(5);
            string hour = (entry5.Text.Trim());

            TextRelay entry6 = info.GetTextEntry(6);
            string minute = (entry6.Text.Trim());

            TextRelay entry7 = info.GetTextEntry(7);
            string restockAmount = (entry7.Text.Trim());

            Int32.TryParse(hour, out numHours);
            Int32.TryParse(minute, out numMinutes);
            Int32.TryParse(beginAmount, out numBegin);
            Int32.TryParse(max, out numMax);
            Int32.TryParse(restockAmount, out numRestockAmnt);

            if (numBegin > 0)
            {
                if ((numMinutes + numHours) > 0 && numRestockAmnt > 0)
                {
                    if (numMax > 0)
                    {
                        return new Reward2.RestockInfo(numHours, numMinutes, numBegin, numRestockAmnt, numMax);
                    }
                    else
                    {
                        return new Reward2.RestockInfo(numHours, numMinutes, numBegin, numRestockAmnt);
                    }
                }

                return new Reward2.RestockInfo(numBegin);
            }                   
        }
        return m_Restock; //default
    }
    public Storage2 GetEntries(RelayInfo info)
    {
        //Description
        TextRelay entry0 = info.GetTextEntry(0);
        string desc = (entry0 == null ? "None." : entry0.Text.Trim());

        //Cost
        TextRelay entry1 = info.GetTextEntry(1);
        string costtext = (entry1 == null ? "0" : entry1.Text.Trim());

        //Name
        TextRelay entry2 = info.GetTextEntry(2);
        string name = (entry2 == null ? "" : entry2.Text.Trim());

        return new Storage2(desc, costtext, name);
    }
    public override void OnResponse(NetState sender, RelayInfo info)
    {
        Mobile m = sender.Mobile;

        switch (info.ButtonID)
        {
            case 0:
                {
                    //cancel
                    break;
                }
            case 1: //ok
                {
                    try
                    {
                        //Description
                        TextRelay entry0 = info.GetTextEntry(0);
                        string desc = (entry0 == null ? "None." : entry0.Text.Trim());

                        //Cost
                        TextRelay entry1 = info.GetTextEntry(1);
                        string costtext = (entry1 == null ? "%Error" : entry1.Text.Trim());
                        int cost = Int32.Parse(costtext);

                        //Name
                        TextRelay entry2 = info.GetTextEntry(2);
                        string name = (entry2 == null ? "" : entry2.Text.Trim());

                        if ( m_IsAdd )
                            m_Vendor.AddReward2(m_Reward2);
                        
                        m_Reward2.Edit(cost, name, desc);

                        if (m_RestockOpen)
                        {
                            Reward2.RestockInfo input = GetRestockInfo(info);

                            if (input != null)
                            {
                                if (m_Restock != null && !(m_Restock.Equals(input))) //do not reassign same object
                                {
                                    m_Reward2.Restock = input;
                                }
                                else
                                {
                                    m_Reward2.Restock = input;
                                }
                            }
                        }

                        m.SendMessage("{0} has been modified in {1}'s collection", name, m_Vendor.GetName());

                    }
                    catch { m.SendMessage("Please make sure all fields are filled correctly, and the title has not been previously used."); }

                    break;
                }
            case 2: //open restock options
                {                                  
                    m.SendGump(new EditItemGump2(m_Vendor, m_Reward2, GetEntries(info), true, m_IsAdd));
                    
                    return; //override display 
                }
            case 3: //delete restock options
                {                   
                    m_Reward2.Void_RestockInfo();
                                    
                    m.SendGump(new EditItemGump2(m_Vendor, m_Reward2, GetEntries(info), false, m_IsAdd));
                    
                    return; //override display 
                }
        }
        MenuUploader2.Display(m_Vendor.Menu, m, m_Vendor, true); //send back display
    }
}

public class Exhibit2 : Item
{
    private IReward2Vendor m_Vendor;
    private Reward2 m_Reward2;
    private ObjectPropertyList m_PropertyList;

    public Exhibit2(IReward2Vendor vendor, Reward2 r)
        : base()
    {
        m_Vendor = vendor;
        m_Reward2 = r;

        SetOPL(r);

        this.ItemID = r.Reward2Info.ItemID;
        this.Hue = r.Reward2Info.Hue;
    }
    public override void SendPropertiesTo(Mobile from)
    {
        if (m_PropertyList == null)
        {
            m_PropertyList = new ObjectPropertyList(this);
            m_PropertyList.Add("Display [Broken]");
        }

        from.Send(m_PropertyList);
    }
    public void SetOPL(Reward2 r)
    {
        Item i = r.Reward2Info;

        m_PropertyList = new ObjectPropertyList(this);

        m_PropertyList.Add("Cost: " + m_Reward2.Cost.ToString() + " " + m_Vendor.Payment.PayName + " [Click for more]");

        i.GetProperties(m_PropertyList);
        i.AppendChildProperties(m_PropertyList);

        m_PropertyList.Terminate();
        m_PropertyList.SetStatic();

    }

    public override void OnDoubleClick(Mobile m)
    {
        if (m_Vendor == null || m_Vendor.IsRemoved() || m_Reward2 == null || !m_Vendor.Display.Contains(m_Reward2))
        {
            m_Vendor = null; // free up resource
            m_Reward2 = null;
            m.SendMessage("This item is no longer available to purchase.");
        }
        else
            m.SendGump(new ViewItemGump2(m, m_Vendor, m_Reward2, false));
    }

    public Exhibit2(Serial serial)
        : base(serial)
    {
    }
    public override void Deserialize(GenericReader reader)
    {
        base.Deserialize(reader);

        int version = reader.ReadInt();

        if (reader.ReadBool())
        {
            m_Reward2 = (Reward2)reader.ReadItem();

            m_Vendor = (IReward2Vendor)reader.ReadMobile();

            try { SetOPL(m_Reward2); }
            catch { }
        }
    }

    public override void Serialize(GenericWriter writer)
    {
        base.Serialize(writer);

        writer.Write((int)0); // version  

        if (m_Vendor == null || m_Vendor.IsRemoved() || m_Reward2 == null || m_Reward2.Deleted)
        {
            m_Vendor = null;
            m_Reward2 = null;

            writer.Write((bool)false); //do not deserialize
        }
        else
        {
            writer.Write((bool)true);

            writer.WriteItem((Reward2)m_Reward2);

            writer.Write((Mobile)m_Vendor.GetMobile());
        }
    }
}
public class ViewItemGump2 : Gump
{
    IReward2Vendor m_Vendor;
    Reward2 m_Reward2;

    public ViewItemGump2(Mobile m, IReward2Vendor vendor, Reward2 r, bool viewItem)
        : base(0, 0)
    {
        m_Vendor = vendor;
        m_Reward2 = r;

        this.Closable = true;
        this.Disposable = true;
        this.Dragable = true;
        this.Resizable = false;

        AddPage(0);
        AddImageTiled(149, 207, 548, 277, 2624);
        AddImage(184, 300, 2328);
        AddImage(101, 222, 10400);
        
        AddHtml(428, 290, 243, 140, r.Description, (bool)true, (bool)true);
        AddItem(201, 311, r.Reward2Info.ItemID, r.Reward2Info.Hue);
                
        AddLabel(430, 263, 2101, @"Description");
        AddButton(192, 442, 4029, 4030, 2, GumpButtonType.Reply, 0); //buy
        AddLabel(231, 444, 2125, "Buy (" + r.Cost +")");
        AddButton(366, 442, 4020, 4021, 0, GumpButtonType.Reply, 0); //cancel
        AddLabel(407, 444, 2116, @"Cancel");
        AddImageTiled(184, 239, 510, 8, 9201);

        AddItem(182, 376, m_Vendor.Payment.PayID, m_Vendor.Payment.CurrHue);
        AddLabel(248, 369, 83, @"Pay By: " + m_Vendor.Payment.PayName);
        AddLabel(184, 268, 43, @"Vendor: " + m_Vendor.GetName());
        AddLabel(363, 216, 2123, r.Title);

        if (m.AccessLevel > AccessLevel.GameMaster) //GM capabilities
        {
            if (!viewItem)
                AddLabel(363, 216, 2123, r.Title);
            else
            {
                AddLabel(336, 296, 2101, @"Edit Item");
                AddButton(294, 293, 4011, 4012, 4, GumpButtonType.Reply, 0); //edit item

                if (m_Vendor.GetMobile() != null)
                {
                    AddLabel(336, 348, 2101, @"Get Display");
                    AddButton(294, 345, 4011, 4012, 3, GumpButtonType.Reply, 0); //get display   
                }
            }
        }
        else
        {
            OpenDisplay(m); //prevent build up of box display during editing
        }

        //Edit gump for display object
        if (viewItem) //not display
        {
            AddLabel(336, 322, 2101, @"View Item");
            AddButton(294, 319, 4011, 4012, 1, GumpButtonType.Reply, 0); //view item
                    
        }
        else //display
        {
            AddLabel(279, 320, 38, @"Cost: " + m_Reward2.Cost.ToString());
            AddLabel(249, 398, 69, @"You have: " + m_Vendor.Payment.Value(m));

            if (m_Reward2.Restock != null)
            {
                m_Reward2.Try_Restock();
                AddLabel(279, 340, 4, "In Stock: " + m_Reward2.Restock.Count.ToString());
            }

            if (m_Reward2.Reward2Info is Container)
                OpenDisplay(m); 
        }
    }
    public void OpenDisplay(Mobile m)
    {
        DisplayBox2 dbox = m_Vendor.Display;

        if (dbox != null)
            dbox.DisplayTo(m, m_Reward2);
    }
    public override void OnResponse(NetState sender, RelayInfo info)
    {
        Mobile m = sender.Mobile;

        if (m_Vendor.IsRemoved())
            return;

        switch (info.ButtonID)
        {
            case 0:
                {
                    break;
                }
            case 1:
                {
                    try
                    {
                        OpenDisplay(m);
                        m.SendGump(this);
                    }
                    catch
                    {
                        m.SendMessage("This Reward can no longer be found.");
                    }
                    break;
                }
            case 2:
                {
                    Container bank = m.BankBox;
                    Container pack = m.Backpack;

                    if ( bank == null || pack == null )
                        break;

                    Currency2 curr = m_Vendor.Payment;

                    if (m_Reward2.InStock(1))
                    {
                        if (curr.Purchase(m, m_Reward2.Cost))
                        {
                            m_Reward2.RegisterBuy(1);

                            Item i = m_Reward2.Reward2Copy;

                            if (!m.PlaceInBackpack(i))
                            {
                                bank.DropItem(i);
                                m.SendMessage("You are overweight, the Reward was added to your bank");
                            }

                            m.SendMessage("You bought {0} for {1} {2}.", m_Reward2.Title, m_Reward2.Cost, curr.PayName);

                        }
                        else
                            m.SendMessage("You cannot afford {0}", m_Reward2.Title);
                    }                           
                    else
                        m.SendMessage("{0} is no longer in stock.", m_Reward2.Title);  

                    break;
                }
            case 3:
                {
                    if (m.Backpack != null)
                        m.AddToBackpack(new Exhibit2(m_Vendor, m_Reward2));
                    break;
                }
            case 4:
                {
                    m.SendGump(new EditItemGump2(m_Vendor, m_Reward2, null, false, false));
                    break;
                }
        }
    }
}

//Created for the usage of Warning Gump delegate 
public class Storage2
{
    object[] m_Objs;

    public object this[int index]
    {
        get { return m_Objs[index]; }
    }

    public Storage2(params object[] input)
    {
        m_Objs = input;
    }
    
}


public class WorldVendorsGump2 : Gump
{
    IReward2Vendor m_Vendor; //used in case of copy

    public WorldVendorsGump2(IReward2Vendor vendor) : base( 0, 0 )
    {
        m_Vendor = vendor;

        this.Closable=true;
		this.Disposable=true;
		this.Dragable=true;
		this.Resizable=false;

		AddPage(0);

        //background
        AddImageTiled(50, 3, 430, 556, 2702);
		AddImageTiled(93, 78, 387, 9, 10101);
		AddImage(33, 58, 10420);
		AddImageTiled(47, 2, 3, 556, 10004);
		AddImageTiled(0, 58, 82, 414, 10440);
		AddImageTiled(47, 558, 436, 3, 10001);
		AddImageTiled(47, 0, 435, 3, 10001);
		AddImageTiled(480, 2, 3, 556, 10004);

        AddPage(1);

        CreateVendors(); //fill out entries
    }
    
    public static void CopyVendor_Callback(Mobile from, bool okay, object state)
    {
        if (from.AccessLevel < AccessLevel.GameMaster)
            return;
                
        if (okay)
        {
            //indexes -> (vendor (0), vendor's copy target(1))
            Storage2 store = (Storage2)state;

            IReward2Vendor source = (IReward2Vendor)store[0];
            IReward2Vendor target = (IReward2Vendor)store[1];

            try { source.CopyVendor(target); }
            catch { from.SendMessage("Error occured while copying, please insure backpack is on vendors."); return; }

            from.SendMessage("{0} has copied {1}'s Reward collection", source.GetName(), target.GetName());
        }
    }
    public override void OnResponse(NetState sender, RelayInfo info)
    {
        Mobile m = sender.Mobile;

        if (info.ButtonID == 0)
        {
            m.CloseGump(typeof(ControlPanelGump2));
            return;
        }

        MenuUploader2.Display(m_Vendor.Menu, m, m_Vendor, false);

        try
        {
            if (info.ButtonID % 2 == 0 ) //even ID: Copy Vendor
            {
                IReward2Vendor selected = WorldReward2Vendors.Vendors[(info.ButtonID / 2) - 1];

                if ( m_Vendor.GetName().Equals(selected.GetName()))
                {
                    m.SendMessage("A vendor cannot copy itself.");
                }
                else
                {
                    //params -> (vendor, vendor's copy target)
                    Storage2 store = new Storage2(m_Vendor, selected);

                    m.SendGump(new WarningGump(1060635, 30720, "Warning: You will lose all saved items on " + m_Vendor.GetName(), 0xFFC000, 420, 400, new WarningGumpCallback(CopyVendor_Callback), store));
                }
            }
            else //assert odd ID: Goto 
            {
                IReward2Vendor v = WorldReward2Vendors.Vendors[((info.ButtonID + 1) / 2) - 1];

                m.Location = v.GetLocation();
                m.Map = v.GetMap();
            }
        }
        catch
        {
            m.SendMessage("Vendor cannot be found.");
        }
    }
    public void CreateVendors()
    {
        int yPos = 101, pageNum = 1, buttonNum = 0;
      
        for (int i = 0; i < WorldReward2Vendors.Vendors.Count; ++i)
        {            
            AddButton(175, yPos + 38, 4005, 4006, ++buttonNum, GumpButtonType.Reply, pageNum); //goto button - odd ID
            AddButton(279, yPos + 38, 4011, 4012, ++buttonNum, GumpButtonType.Reply, pageNum); //copy vendor - even ID
            AddLabel(86, yPos, 1849, WorldReward2Vendors.Vendors[i].GetName());
            AddImageTiled(86, yPos + 93, 359, 2, 96);

            if (WorldReward2Vendors.Vendors[i].GetMobile() != null)
                AddItem(110, yPos + 27, 8461); // person
            else

                AddItem(110, yPos + 27, WorldReward2Vendors.Vendors[i].GetItem().ItemID, WorldReward2Vendors.Vendors[i].GetItem().Hue); //stone ID

            AddLabel(213, yPos + 43, 2209, @"Go To");
            AddLabel(318, yPos + 43, 2204, @"Copy Vendor");
            
            yPos += 99;

            //add new page every four entries
            if ((i + 1) % 4 == 0)
            {
                AddButton(417, 531, 9903, 248, -1, GumpButtonType.Page, (pageNum + 1));

                AddPage(++pageNum);

                AddButton(92, 532, 9909, 248, -1, GumpButtonType.Page, (pageNum - 1));

                //reset to top of page
                yPos = 101;
            }
        }
    }
}
public class InfoGump2 : Gump
{
    public InfoGump2()
        : base(0, 0)
    {
        this.Closable = true;
        this.Disposable = true;
        this.Dragable = true;
        this.Resizable = false;

        AddPage(0);
        
        AddImageTiled(0, 0, 452, 516, 2702);
        AddHtml(21, 78, 407, 105, @"Consume Item:  Vendor counts the number of the chosen item present in player's backpack.  

        Property(Item/Player):  Vendor takes the total of the given number based property (example: PlayerPoints, Charges)
        (Items): Adds up from every item present.
        (Player): Simply takes from what is present.
        ", (bool)true, (bool)true);

        AddLabel(23, 52, 2000, @"Choosing How Player Pays:");
        AddHtml(21, 242, 407, 105, @"You can create your own rewards in game, all properties will be copied over.  ", (bool)true, (bool)true);
        AddLabel(24, 216, 2000, @"Adding Items");
        AddLabel(239, 479, 37, @"Author: krazeykow1102");
        AddItem(391, 467, 8451);
        AddLabel(24, 368, 2000, @"How Displays Work");
        AddHtml(24, 394, 410, 78, @"Displays allow you to create your own interactive Reward room.  The display is a simple mirror to the item on the vendor.  Payment and cost will reflect what is on the vendor.  Vendor must stay in tact for display to function", (bool)true, (bool)true);
    }
}


public class ControlPanelGump2 : Gump
{
    private IReward2Vendor m_Vendor;
   
    public ControlPanelGump2(Mobile m, IReward2Vendor vendor)
        : base(0, 0)
    {

        m_Vendor = vendor;
                
        this.Closable = true;
        this.Disposable = true;
        this.Dragable = true;
        this.Resizable = false;

        AddPage(0);

        //background
        AddImageTiled(4, 3, 215, 571, 2702);
        AddImageTiled(3, 0, 217, 3, 10001);
        AddImageTiled(4, 572, 216, 3, 10001);
        AddImageTiled(3, 1, 3, 571, 10004);
        AddImageTiled(217, 1, 3, 574, 10004);

        AddImage(40, 8, 5214);
        AddLabel(41, 7, 47, @"Control Panel");

        AddLabel(109, 62, 55, @"Manage Rewards");
        AddButton(72, 60, 4008, 4009, 1, GumpButtonType.Reply, 0); //PlayerView
        AddItem(31, 63, 5366); //scope
        AddImageTiled(19, 160, 132, 2, 96);

        AddLabel(79, 123, 1324, @"Add Item or Container");
        AddButton(52, 126, 2117, 2118, 2, GumpButtonType.Reply, 0); //AddItem
        AddItem(0, 113, 8192); //box  
        
        AddLabel(11, 232, 55, @"Target Payment Source");
        AddButton(73, 259, 2117, 2118, 3, GumpButtonType.Reply, 0); //Payment
        AddLabel(12, 173, 55, @"Pay By: " + m_Vendor.Payment.PayName );
        AddItem(61, 197, m_Vendor.Payment.PayID, m_Vendor.Payment.CurrHue);

        AddLabel(12, 32, 55, @"Vendor: " + m_Vendor.GetName());
        AddButton(77, 533, 4026, 4027, 4, GumpButtonType.Reply, 0);
        AddLabel(78, 553, 104, @"Help");

        if (m_Vendor.GetMobile() != null)
        {
            AddButton(72, 91, 4011, 4012, 5, GumpButtonType.Reply, 0); //Displays
            AddLabel(109, 93, 55, @"Get All Displays");
        }
        else
            AddLabel(12, 93, 55, @"Use Mobile Vendor for displays.");

        AddLabel(51, 303, 43, @"Consume Item");
        AddRadio(15, 300, 9720, 9724, false, 6);
        AddLabel(50, 349, 43, @"[props of (Item/Player)");
        AddRadio(15, 342, 9720, 9724, false, 7);
        AddImageTiled(12, 382, 156, 22, 5058);
        AddTextEntry(12, 382, 149, 20, 41, 0, @"[prop Name Here");

        AddPage(1);

        CreateGumps(); //fill out entries
               
    }
    public override void OnResponse(NetState sender, RelayInfo info)
    {
        Mobile m = sender.Mobile;

        if (m_Vendor.IsRemoved())
        {
            m.SendMessage("This vendor has been deleted.");
            return;
        }

        switch (info.ButtonID)
        {
            case 0:
                {
                    m.CloseGump(typeof(WorldVendorsGump2));

                    //close
                    break;
                }
            case 1: //PlayerView
                {
                    m.CloseGump(typeof(WorldVendorsGump2));

                    MenuUploader2.Display(m_Vendor.Menu, m, m_Vendor, true);
                    break;
                }
            case 2: //AddItem
                {
                    m.CloseGump(typeof(WorldVendorsGump2));

                    m.Target = new AddItemTarget2(m_Vendor);
                    break;
                }
            case 3: //Payment
                { 
                    
                        try
                        {
                            if ( info.IsSwitched(6)) // Consume by item amount
                            {
                                m.SendMessage("Warning: payment will be taken as the unmodified created item.");
                                m.Target = new PaymentTarget2(m_Vendor);
                            }
                            else if (info.IsSwitched(7)) //Consume by item's property
                            {
                                m.SendMessage("Target yourself or an item.");
                                TextRelay entry0 = info.GetTextEntry(0);
                                string propName = (entry0 == null ? "" : entry0.Text.Trim());

                                m.Target = new PaymentTarget2(m_Vendor, propName);
                            }
                            else
                            {                                
                                MenuUploader2.Display(m_Vendor.Menu, m, m_Vendor, false);
                                m.SendMessage("Please select consume or property.");
                            }
                        }

                            catch { }

                            break;      
                  }
            case 4:
                {
                    MenuUploader2.Display(m_Vendor.Menu, m, m_Vendor, false);
                    m.SendGump(new InfoGump2());
                    break;
                }
            case 5: //Displays
                {
                    if (m.Backpack == null)
                        return;

                    MetalBox c = new MetalBox();
                    c.Name = m_Vendor.GetName() + "'s Reward Displays";

                    foreach (Reward2 r in m_Vendor.Reward2s)
                    {
                        c.DropItem(new Exhibit2(m_Vendor, r));
                    }

                    m.AddToBackpack(c);

                    break;
                }
                default: //Set Menu - IDs are >= 6
                {
                    try
                    {
                        Type menuType = MenuUploader2.Menus[info.ButtonID - 6];

                        if (m_Vendor.GetItem() != null && menuType.Equals(typeof(ClassicVendorGump2)))
                            m.SendMessage("Only Mobile Reward Vendors can have classic menus.");
                        else
                        {
                            m_Vendor.Menu = menuType;
                            m.SendMessage("Display changed to " + menuType.Name);
                        }
                    }
                    catch { }

                    MenuUploader2.Display(m_Vendor.Menu, m, m_Vendor, false);

                    break;
                                      
                }
         }   
    }
    public void CreateGumps()
    {
        int yPos = 433, pageNum = 1;

        for (int i = 0; i < MenuUploader2.Menus.Count; ++i )
        {

            AddLabel(50, yPos, 100, MenuUploader2.Menus[i].Name);
            AddButton(12, yPos, 4005, 4006, (i + 6), GumpButtonType.Reply, pageNum);
            

            yPos += 26;

            //add new page every four entries
            if ((i + 1) % 4 == 0)
            {
                AddButton(144, 542, 9702, 9703, -1, GumpButtonType.Page, (pageNum + 1));

                AddPage(++pageNum);

                AddButton(10, 542, 9706, 9707, -1, GumpButtonType.Page, (pageNum - 1));

                //reset to top of page
                yPos = 433;
            }
        } 
    }
}
public static class MenuUploader2 
{
    private static List<Type> m_Menus = new List<Type>(); 
    
    public static List<Type> Menus { get { return m_Menus; } }

    public static void RegisterMenu<G>(G menu) where G : Gump, IReward2VendorGump
    {
        m_Menus.Add(menu.GetType());
    }
    
    public static void Display(IReward2VendorGump menu, Mobile m, IReward2Vendor vendor)
    {
        m.CloseGump(typeof(IReward2VendorGump));

        //create 'canvas'
        menu.CreateBackground();

        //create Reward displays
        foreach (Reward2 r in vendor.Reward2s)
        {
            //refresh restock  
            if (r.Try_Restock() == 0 && !(menu is ManageItemsGump2))
                continue;
            
            menu.AddEntry(r);
        }

        //send finished product
        menu.Send(m);
    }
    public static void Display(Type menu, Mobile m, IReward2Vendor vendor, bool playerView)  //Note: Playerview is only relevant for Staff
    {
        try
        {
            if (m.AccessLevel > AccessLevel.GameMaster) 
            {
                if (playerView)
                    Display(new ManageItemsGump2(vendor, m), m, vendor);
                else //control panel
                {
                    m.CloseGump(typeof(WorldVendorsGump2));
                    m.CloseGump(typeof(ControlPanelGump2));

                    m.SendGump(new WorldVendorsGump2(vendor)); 
                    m.SendGump(new ControlPanelGump2(m, vendor));
                }
            }
            else //player
            {
                Display((IReward2VendorGump)Activator.CreateInstance(menu, new object[] { vendor, m }), m, vendor); //create fresh instance
            }
        }
        catch
        {
            if ( m.AccessLevel > AccessLevel.GameMaster )
                m.SendMessage("The current display is invalid, try setting gump back to default, JewlRewardGump.");
            else
                m.SendMessage("The current display is invalid, please notify the staff.");
        }
    }
}
public class ManageItemsGump2 : JewlReward2Gump
{
    private int m_ButtonNum;

    public ManageItemsGump2(IReward2Vendor vendor, Mobile m) : base(vendor, m)
    {
        m_ButtonNum = 0;
    }
    public override void AddEntryControl(Reward2 r)
    {
        ImageTileButtonInfo b = new ItemTileButtonInfo(r.Reward2Info);

        //begin time entries
        if (r.Restock != null)
        {
            if (r.Restock.RestockRate.TotalMinutes == 0)
                AddLabel(324, (PosY + 22), 5, "Limited");
            else
                AddLabel(324, (PosY + 22), 5, ((int)r.Restock.RestockRate.Hours).ToString() + " hours " + ((int)r.Restock.RestockRate.Minutes).ToString() + " min");

            if (r.Restock.Maximum > 0)
                AddLabel(324, (PosY + 68), 4, "In Stock: " + r.Restock.Count.ToString() + " / " + r.Restock.Maximum.ToString() + ", Purchased: " + r.BuyCount.ToString());
            else
                AddLabel(324, (PosY + 68), 4, "In Stock: " + r.Restock.Count.ToString() + ", Purchased: " + r.BuyCount.ToString());
        }
        else
        {
            AddLabel(324, (PosY + 68), 4, "Purchased: " + r.BuyCount.ToString());
        }
        //end 

        //create entry
        AddLabel(227, PosY, 2123, r.Title);
        AddLabel(324, (PosY + 45), 2115, "Cost: " + r.Cost);
        //odd numbers
        AddImageTiledButton(227, (PosY + 26), 2328, 2329, ++m_ButtonNum, GumpButtonType.Reply, PageNum, b.ItemID, b.Hue, 15, 10, r.Display.Header);
        AddImageTiled(215, (PosY + 95), 359, 2, 96);
        //odd numbers (same as above)
        AddButton(470, (PosY + 45), 4011, 4012, m_ButtonNum, GumpButtonType.Reply, PageNum);
        AddLabel(508, (PosY + 47), 1882, @"Options");
        //even numbers
        AddButton(470, (PosY + 19), 4020, 4021, ++m_ButtonNum, GumpButtonType.Reply, PageNum);
        AddLabel(509, (PosY + 21), 1882, @"Delete");
        PosY += 102;

        //add new page every four entries
        if (EntryNum % 4 == 0)
        {
            AddButton(546, 562, 9903, 9904, -1, GumpButtonType.Page, PageNum + 1);

            AddPage(++PageNum);

            AddButton(221, 563, 9909, 9910, -1, GumpButtonType.Page, PageNum - 1);

            //reset to top of page
            PosY = 130;
        }

        EntryNum++;
    }
    public static void DeleteReward2_Callback(Mobile from, bool okay, object state)
    {
        if (from.AccessLevel < AccessLevel.GameMaster)
            return;

        if (okay)
        {
            //indexes -> (Vendor(0), Reward2(1))
            Storage2 store = (Storage2)state;

            IReward2Vendor vendor = (IReward2Vendor)store[0];
            Reward2 Reward2 = (Reward2)store[1];

            try { vendor.RemoveReward2(Reward2); }
            catch { from.SendMessage("An error ocurred in the removal of this item."); }
            
            MenuUploader2.Display(vendor.Menu, from, vendor, true);
        }
    }
    public override void OnResponse(NetState sender, RelayInfo info)
    {
        Mobile m = sender.Mobile;

        if (info.ButtonID == 0)
        {
            MenuUploader2.Display(Vendor.Menu, m, Vendor, false);
            return;
        }
            
        if (info.ButtonID % 2 == 0) //even
        {
                try
                {
                    Reward2 r = Vendor.Reward2s[(info.ButtonID / 2) - 1];

                    //params -> (vendor, Reward2)
                    Storage2 store = new Storage2(Vendor, r);

                    m.SendGump(new WarningGump(1060635, 30720, "Warning: Are you sure you want to remove " + r.Title, 0xFFC000, 420, 400, new WarningGumpCallback(DeleteReward2_Callback), store));

                }
                catch { m.SendMessage("This item could not be found."); }
            
        }
        else //assert - odd
        {
            try
            {
                MenuUploader2.Display(Vendor.Menu, m, Vendor, true);
                m.SendGump(new ViewItemGump2(m, Vendor, Vendor.Reward2s[((info.ButtonID + 1) / 2) - 1], true));
            }
            catch { m.SendMessage("Vendor could not be found"); }
        }
        
    }
    
}
public class JewlReward2Gump : Gump, IReward2VendorGump
{
    private int m_PageNum;
    private int m_EntryNum;
    private int m_PosY;
    private IReward2Vendor m_Vendor;
    private Mobile m_Mobile;
    private int m_Currency2Amnt;
   
    public static void Initialize()
    {
        MenuUploader2.RegisterMenu(new JewlReward2Gump());
    }

    public JewlReward2Gump()
        : base(0, 0)
    {
        m_PageNum = 1;
        m_EntryNum = 1;
        m_PosY = 130;
        m_Vendor = null;
        m_Mobile = null;
        m_Currency2Amnt = 0;
    }
    public JewlReward2Gump(IReward2Vendor vendor, Mobile m) : this()
    {
        m_Vendor = vendor;
        m_Mobile = m;
        m_Currency2Amnt = vendor.Payment.Value(m);
    }

    //Properties
    public int PageNum { get { return m_PageNum; } set { m_PageNum = value; } }
    public int EntryNum { get { return m_EntryNum; } set { m_EntryNum = value; } }
    public int PosY { get { return m_PosY; } set { m_PosY = value; } }
    public IReward2Vendor Vendor { get { return m_Vendor; } set { m_Vendor = value; } }
    public Mobile Mobile { get { return m_Mobile; } set { m_Mobile = value; } }
    public int Currency2Amnt { get { return m_Currency2Amnt; } set { m_Currency2Amnt = value; } }

    //Interface Explicit Implementation 
    void IReward2VendorGump.Send(Mobile m) { SendControl(m); }
    void IReward2VendorGump.CreateBackground() { CreateBackgroundControl(); } 
    void IReward2VendorGump.AddEntry(Reward2 r) { AddEntryControl(r); }

    public virtual void SendControl(Mobile m)
    {
        m.SendGump(this);
    }
    public virtual void CreateBackgroundControl()
    {
        this.Closable = true;
        this.Disposable = true;
        this.Dragable = true;
        this.Resizable = false;

        AddPage(0);

        AddImageTiled(179, 34, 430, 556, 2702);
        AddImageTiled(222, 109, 387, 9, 10101);
        AddImage(162, 89, 10420);
        AddImageTiled(176, 33, 3, 556, 10004);
        AddImageTiled(129, 89, 82, 414, 10440);
        AddImageTiled(176, 589, 436, 3, 10001);
        AddImageTiled(176, 31, 435, 3, 10001);
        AddImageTiled(609, 33, 3, 556, 10004);
        AddLabel(348, 52, 2124, m_Vendor.GetName());
        AddImage(210, 52, 9818);
        AddImage(534, 52, 9818);
        AddLabel(404, 119, 2125, @"You have:");
        AddItem(348, 126, m_Vendor.Payment.PayID, m_Vendor.Payment.CurrHue);
        AddLabel(404, 137, 2125, (m_Currency2Amnt.ToString() + " " + m_Vendor.Payment.PayName) );
        

        AddPage(1);
    }
      
    public virtual void AddEntryControl(Reward2 r)
    {
        ImageTileButtonInfo b = new ItemTileButtonInfo(r.Reward2Info);

        //create entry

        //begin time entries
        if (r.Restock != null)
        {
            if (r.Restock.RestockRate.TotalMinutes == 0)
                AddLabel(324, (PosY + 22), 5, "Limited");

            if (r.Restock.Maximum > 0)
                AddLabel(324, (PosY + 68), 4, "In Stock: " + r.Restock.Count.ToString());
            else
                AddLabel(324, (PosY + 68), 4, "In Stock: " + r.Restock.Count.ToString());
        }
        //end 

        AddLabel(227, m_PosY, 2123, r.Title);
        AddLabel(324, (m_PosY + 45), 2115, "Cost: " + r.Cost);
        AddImageTiledButton(227, (m_PosY + 26), 2328, 2329, (m_EntryNum), GumpButtonType.Reply, m_PageNum, b.ItemID, b.Hue, 15, 10, b.LocalizedTooltip);
        AddImageTiled(215, (m_PosY + 95), 359, 2, 96);
        AddButton(470, (PosY + 45), 4011, 4012, (m_EntryNum), GumpButtonType.Reply, m_PageNum);
        AddLabel(508, (PosY + 47), 745, @"View Item");
        m_PosY += 102;
        
        //add new page every four entries
        if (m_EntryNum % 4 == 0)
        {
            AddButton(546, 562, 9903, 9904, -1, GumpButtonType.Page, m_PageNum + 1);

            AddPage(++m_PageNum);

            AddButton(221, 563, 9909, 9910, -1, GumpButtonType.Page, m_PageNum - 1);

            //reset to top of page
            m_PosY = 130;
        }

        m_EntryNum++;
    }
    public override void OnResponse(NetState sender, RelayInfo info)
    {
        Mobile m = sender.Mobile;

        if (info.ButtonID == 0)
            return;

        MenuUploader2.Display(m_Vendor.Menu, m, m_Vendor, true);

        try
        {
            m.SendGump(new ViewItemGump2(m, m_Vendor, m_Vendor.Reward2s[info.ButtonID - 1], true));
        }
        catch { m.SendMessage("Vendor could not be found"); }
    }
}

public class DisplayBox2 : LargeCrate, IEnumerable
{
    private Dictionary<Reward2, MetalBox> m_Boxes;

    public override bool IsPublicContainer { get { return true; } }

    public MetalBox this[Reward2 r]
    {
        get
        {
            MetalBox box = null; //not found

            m_Boxes.TryGetValue(r, out box);

            return box;
        }
    }

    [Constructable]
    public DisplayBox2()
        : base()
    {
        this.Name = "Display Box [DO NOT REMOVE]";
        this.LiftOverride = true;
        m_Boxes = new Dictionary<Reward2, MetalBox>();
    }
    public DisplayBox2(Reward2Collection rc)
        : this()
    {
        CreateEntries(rc);
    }
    public bool Contains(Reward2 r)
    {
        return m_Boxes.ContainsKey(r);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return m_Boxes.GetEnumerator();
    }
    public override bool CheckLift(Mobile from, Item item, ref LRReason reject)
    {
        reject = 0;
        return false;
    }
    public void DisplayTo(Mobile m, Reward2 r)
    {
        string invalid = "This item can no longer be displayed.";
        bool staff = m.AccessLevel >= AccessLevel.GameMaster;

        try
        {
            Container[] containers = new Container[] { (Container)this.Parent, this }; //cast exception if parent isn't container

            foreach (Container c in containers)
            {
                if (m.NetState != null && m.NetState.IsPost6017)
                    m.Send(new ContainerContent6017(m, c));
                else
                    m.Send(new ContainerContent(m, c));
            }

            m_Boxes[r].DisplayTo(m); //throws null ref if not found

            m.SendMessage("A container has been opened for you to view the item.");
        }
        catch (NullReferenceException)
        {
            if (staff)
                m.SendMessage("Display error - please insure Reward is still on this vendor.");
            else
                m.SendMessage(invalid);
        }
        catch (InvalidCastException)
        {
            if (staff)
                m.SendMessage("Display error - the current vendor does not have a DisplayBox in backpack.");
            else
                m.SendMessage(invalid);
        }
        catch (Exception)
        {
            if (staff)
                m.SendMessage("Unknown error occured with display.");
            else
                m.SendMessage(invalid);
        }
            
    }
    //Warning: must catch exception in methods
    public void CreateEntries(Reward2Collection rc)
    {
        foreach (Reward2 r in rc)
        {
            this.AddDisplay(r, r.Reward2Info);
        }
    }
    public void AddDisplay(Reward2 key, Item i)
    {
        if (m_Boxes.ContainsKey(key))
            throw new Exception("Key already in use.");

        MetalBox container = new MetalBox();
        
        container.DropItem(i);

        this.DropItem(container);

        m_Boxes.Add(key, container);    
    }
    public void AddDisplay(Reward2 r)
    {
        AddDisplay(r, r.Reward2Info);
    }
    public void RemoveDisplay(Reward2 r)
    {
        ((Item)m_Boxes[r]).Delete(); //remove from displaybox
        m_Boxes.Remove(r);   //remove from hashtable      
    }
    //End Warning

    public override bool OnDragDrop( Mobile from, Item dropped )
    {
        return false;//do nothing
    }

    public DisplayBox2( Serial serial ) : base( serial )
	{
	}
    
	public override void Serialize( GenericWriter writer )
    {
		base.Serialize( writer );

		writer.Write( (int) 0 ); // version
        
        int count = 0;
        try
        {
            count = m_Boxes.Count;
        }
        catch
        {
            m_Boxes = new Dictionary<Reward2, MetalBox>();
            m_Boxes.Add(new Reward2(), new MetalBox());
            count = m_Boxes.Count;
        }
        writer.Write((int)count);

        foreach (KeyValuePair<Reward2, MetalBox> kvp in m_Boxes)
        {
            writer.Write((Reward2)kvp.Key);
            writer.WriteItem((MetalBox)kvp.Value);
        }
        
    }

	public override void Deserialize( GenericReader reader )
    {
		base.Deserialize( reader );
                
		int version = reader.ReadInt();

        
        int count = reader.ReadInt();
        
        m_Boxes = new Dictionary<Reward2, MetalBox>();     
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                Reward2 r = (Reward2)reader.ReadItem();
                MetalBox box = (MetalBox)reader.ReadItem();

                m_Boxes.Add(r, box);
            }
        }      
    }
}

public class LoadGumpEntry2 : ContextMenuEntry
{
    private IReward2Vendor m_Vendor;
    private Mobile m_Mobile;

    public LoadGumpEntry2(Mobile from, IReward2Vendor vendor)
        : base(6103, 8)
    {
        m_Vendor = vendor;
        m_Mobile = from;
        Enabled = true;
    }

    public override void OnClick()
    {
        try
        {
            MenuUploader2.Display((IReward2VendorGump)Activator.CreateInstance(m_Vendor.Menu, new object[] { m_Vendor, m_Mobile }), m_Mobile, m_Vendor); //create fresh instance
        }
        catch { m_Mobile.SendMessage("The current display is invalid.");  }
    }
}
public class ClassicVendorGump2 : Gump, IReward2VendorGump
{
    private IReward2Vendor m_Vendor;
    private BaseVendor m_BVendor;

    private Mobile m_Mobile;   

    private List<ObjectPropertyList> m_Opls;
    private ArrayList m_List;

    public List<ObjectPropertyList> ObjPropLists { get { return m_Opls; } }
    public ArrayList Reward2List { get { return m_List; } }

    public static void Initialize()
    {
        MenuUploader2.RegisterMenu(new ClassicVendorGump2());
    }

    public ClassicVendorGump2()
        : base(0, 0)
    {
        m_Vendor = null;
        m_Mobile = null;
        m_BVendor = null;
        m_Opls = null;
        m_List = null;
    }
    public ClassicVendorGump2(IReward2Vendor vendor, Mobile m) : this()
    {
        m_Vendor = vendor;
        m_Mobile = m;
        m_BVendor = Parent(vendor);
        m_Opls = new List<ObjectPropertyList>();
        m_List = new ArrayList();
    }
    private BaseVendor Parent(IReward2Vendor vendor)
    {
        BaseVendor validVendor = null;

        if (vendor != null) //only available on mobiles inherited from BaseVendor
            validVendor = vendor as BaseVendor;

        return validVendor;
    }
    void IReward2VendorGump.Send(Mobile m)
    {
        if ( !m_Vendor.Payment.PaymentType.Equals(typeof(Gold)) ) //need custom payment display
            m.SendGump(this);    

        if (m_BVendor != null)
        {

            if (m_List.Count > 0)
            {
                m_List.Sort(new BuyItemStateComparer());

                m_BVendor.SendPacksTo(m);

                if (m.NetState == null)
                    return;

                if (m.NetState.IsPost6017)
                    m.Send(new VendorBuyContent6017(m_List));
                else
                    m.Send(new VendorBuyContent(m_List));
                m.Send(new VendorBuyList(m_BVendor, m_List));
                m.Send(new DisplayBuyList(m_BVendor));

                m.Send(new MobileStatusExtended(m));//make sure their gold amount is sent

                if (m_Opls != null)
                {
                    for (int i = 0; i < m_Opls.Count; ++i)
                    {
                        m.Send(m_Opls[i]);
                    }
                }

                m_BVendor.SayTo(m, 500186); // Greetings.  Have a look around.
            }
        }               
    }   
    void IReward2VendorGump.AddEntry(Reward2 r)
    {
        m_List.Add(new BuyItemState(r.Title, m_BVendor.BuyPack.Serial, r.Serial, r.Cost, (r.Restock == null ? 20 : r.Restock.Count), r.Reward2Info.ItemID, r.Reward2Info.Hue));

        m_Opls.Add(r.Display);      
    }
    void IReward2VendorGump.CreateBackground()
    {
        this.Closable = true;
        this.Disposable = true;
        this.Dragable = true;
        this.Resizable = false;

        AddPage(0);
        AddImage(36, 105, 2162);
        AddItem(111, 217, m_Vendor.Payment.PayID, m_Vendor.Payment.CurrHue);
        AddLabel(110, 172, 2122, @"Pay By:  " + m_Vendor.Payment.PayName);
        AddLabel(110, 299, 2120, @"You have: " + m_Vendor.Payment.Value(m_Mobile));
        AddImage(71, 302, 57);
        AddImage(71, 174, 57);
    }
}
public class MobileReward2Vendor : Banker, IReward2Vendor 
{
    private Currency2 m_Currency2;
    private Reward2Collection m_Reward2s;
    private Type m_Menu;
    private DisplayBox2 m_Box;

    private bool m_IsBanker;

    [CommandProperty(AccessLevel.GameMaster)]
    public bool IsBanker
    {
        get { return m_IsBanker; }
        set { m_IsBanker = value; }     
    }
    Currency2 IReward2Vendor.Payment
    {
        get { return m_Currency2; }
        set { m_Currency2 = value; }
    }
    Reward2Collection IReward2Vendor.Reward2s
    {
        get { return m_Reward2s; }
        set { m_Reward2s = value; }
    }
    Type IReward2Vendor.Menu
    {
        get { return m_Menu; }
        set { m_Menu = value;  }
    }
    DisplayBox2 IReward2Vendor.Display
    {
        get { return m_Box; }
    }
    [Constructable]
    public MobileReward2Vendor() : base()
    {
        m_IsBanker = true;

        //default is Gold
        m_Currency2 = new Currency2();
        m_Reward2s = new Reward2Collection();
        m_Menu = typeof(ClassicVendorGump2);
        m_Box = new DisplayBox2();

        //add to world collection
        WorldReward2Vendors.RegisterVendor(this);
        //must be brought to world for client to view
        this.AddToBackpack(m_Box);

        this.Title = "";
    }
    public override void AddCustomContextEntries(Mobile from, List<ContextMenuEntry> list)
    {
        if (from.Alive)
        {
            list.Add(new LoadGumpEntry2(from, this));

            if ( m_IsBanker)
                list.Add(new OpenBankEntry(from, this));
        }
    }
    Mobile IReward2Vendor.GetMobile()
    {
        return this;
    }
    Item IReward2Vendor.GetItem()
    {
        return null;
    }
   
    bool IReward2Vendor.IsRemoved()
    {
        return this.Deleted;
    }
    Container IReward2Vendor.GetContainer()
    {
        return this.Backpack;
    }
    string IReward2Vendor.GetName()
    {
        return this.Name;
    }
    Map IReward2Vendor.GetMap()
    {
        return this.Map;
    }
    Point3D IReward2Vendor.GetLocation()
    {
        return this.Location;
    }
    //begin bank override
    public override bool HandlesOnSpeech(Mobile from)
    {
        if (!m_IsBanker)
            return false;

        base.HandlesOnSpeech(from);

        return true;
    }
    public override void OnSpeech(SpeechEventArgs e)
    {
        if (m_IsBanker)
            base.OnSpeech(e);
    }    
    //end

    //Warning: Following methods will throw Exceptions
    void IReward2Vendor.AddReward2(Reward2 r)
    {
        m_Box.AddDisplay(r);
        m_Reward2s.Add(r);
    }
    void IReward2Vendor.RemoveReward2(Reward2 r)
    {
        m_Reward2s.Remove(r);
        m_Box.RemoveDisplay(r);
    }
    
    void IReward2Vendor.CopyVendor(IReward2Vendor vendor)
    {
        Item currBox = this.Backpack.FindItemByType(typeof(DisplayBox2));
        currBox.Delete();

        m_Currency2 = vendor.Payment;
        m_Menu = vendor.Menu;

        m_Reward2s = (Reward2Collection)((vendor.Reward2s as ICloneable).Clone());
        m_Box = new DisplayBox2(m_Reward2s);

        this.AddToBackpack(m_Box);
    }
    //End Warning
    public override void OnDelete()
    {
        WorldReward2Vendors.RemoveVendor(this);
        base.OnDelete();
    }
    //response to ClassicVendorGump2 - (must be inside BaseVendor)
    public override bool OnBuyItems(Mobile buyer, ArrayList list)
    {
        int totalCost = 0;
      
        Dictionary<Reward2, int> purchases = new Dictionary<Reward2, int>();

        Container bank = buyer.BankBox;
        Container pack = buyer.Backpack;

        if (pack == null || bank == null)
            return false;

        foreach (BuyItemResponse buy in list)
        {
            Serial ser = buy.Serial;
            int amount = buy.Amount;

            if (ser.IsItem)
            {
                Item item = World.FindItem(ser);

                if (item == null)
                    continue;

                Reward2 r = item as Reward2;

                if (r == null || !r.InStock(amount))
                    continue;

                totalCost += (r.Cost * amount);

                purchases.Add(r, amount);
            }
        }

        if (purchases.Count == 0)
        {
            SayTo(buyer, 500190); // Thou hast bought nothing! 
            return false;
        }                    
            if (!m_Currency2.Purchase(buyer, totalCost)) //cannot afford
            {
                SayTo(buyer, 500192);//Begging thy pardon, but thou casnt afford that.
                return false;
            }

            foreach (KeyValuePair<Reward2, int> kvp in purchases)
            {
                kvp.Key.RegisterBuy(kvp.Value);

                for (int index = 0; index < kvp.Value; index++)
                {                    
                    Item i = kvp.Key.Reward2Copy;

                    if (!buyer.PlaceInBackpack(i))
                    {
                        bank.DropItem(i);
                        buyer.SendMessage("You are overweight, the Reward was added to your bank");
                    }
                }

                buyer.SendMessage("You bought {0} {1}", kvp.Value, kvp.Key.Title);
            }

            buyer.PlaySound(0x32);

            return true;  
             
    }
    //respone to 'vendor buy'
    public override void VendorBuy(Mobile from)
    {       
        MenuUploader2.Display((IReward2VendorGump)Activator.CreateInstance(m_Menu, new object[] { (IReward2Vendor)this, from }), from, (IReward2Vendor)this ); //create fresh instance
    }
    //end edit
    public override void OnDoubleClick(Mobile m)
    {
        if (InRange(m, 4) && InLOS(m))
            MenuUploader2.Display(m_Menu, m, this, false);
    }
    public MobileReward2Vendor( Serial serial ) : base( serial )
	{
	}
    
	public override void Serialize( GenericWriter writer )
    {
		base.Serialize( writer );   

		writer.Write( (int) 1 ); // version

        writer.Write((bool)m_IsBanker);

        int count = m_Reward2s.Count;
        writer.Write((int)count);

        if ( count > 0 )
        {
            for (int i = 0; i < count; i++)
            {
                writer.WriteItem((Reward2)m_Reward2s[i]);
            }
        }
        
        writer.WriteItem((Currency2)m_Currency2);
        writer.Write((string)m_Menu.Name);
        writer.WriteItem((DisplayBox2)m_Box);       
    }
    
	public override void Deserialize( GenericReader reader )
    {
		base.Deserialize( reader );

        WorldReward2Vendors.RegisterVendor(this);

		int version = reader.ReadInt();
        
        m_Reward2s = new Reward2Collection();

        m_IsBanker = true; //for conversion
               
        switch (version)
        {
            case 1:
                {
                    m_IsBanker = reader.ReadBool();

                    goto case 0;
                }
            case 0:
               {                   
                    int count = reader.ReadInt();
                    if (count > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            Reward2 r = (Reward2)reader.ReadItem();
                            m_Reward2s.Add(r);
                        }
                    }

                    m_Currency2 = (Currency2)reader.ReadItem();

                    try { string name = reader.ReadString(); m_Menu = ScriptCompiler.FindTypeByName(name); }
                    catch { m_Menu = typeof(JewlReward2Gump); }

                    m_Box = (DisplayBox2)reader.ReadItem();

                    break;
                }
        }
	}
}
public class StoneReward2Holder : MetalBox
{
    private StoneReward2Vendor m_S_Vendor;

    public StoneReward2Holder(StoneReward2Vendor sv)
        : base()
    {
        m_S_Vendor = sv;
        this.Movable = false;
        this.Name = sv.Name + "'s Display Container [DO NOT DELETE]";
    }
    public override void OnItemLifted(Mobile m, Item item)
    {
        m.SendMessage("This item should not be moved away from the vendor.");
    }

    public void UpdateName()
    {
        if (m_S_Vendor == null)
            return;

        this.Name = m_S_Vendor.Name + "'s Display Container [DO NOT DELETE]";
    }
    public StoneReward2Holder(Serial serial)
        : base(serial)
	{
	}

	public override void Serialize( GenericWriter writer )
	{
		base.Serialize( writer );

		writer.Write( (int) 0 ); // version
	}

	public override void Deserialize( GenericReader reader )
	{
		base.Deserialize( reader );

		int version = reader.ReadInt();
    }
}
public class StoneReward2Vendor : Item, IReward2Vendor
{
    private Currency2 m_Currency2;
    private Reward2Collection m_Reward2s;
    private Type m_Menu;
    private StoneReward2Holder m_Box_Holder;
    private DisplayBox2 m_Box;

    Currency2 IReward2Vendor.Payment
    {
        get { return m_Currency2; }
        set { m_Currency2 = value; }
    }
    Reward2Collection IReward2Vendor.Reward2s
    {
        get { return m_Reward2s; }
        set { m_Reward2s = value; }
    }
    Type IReward2Vendor.Menu
    {
        get { return m_Menu; }
        set { m_Menu = value; }
    }
    DisplayBox2 IReward2Vendor.Display
    {
        get { return m_Box; }
    }

    [Constructable]
    public StoneReward2Vendor()
        : base()
    {
        m_Currency2 = new Currency2();
        m_Reward2s = new Reward2Collection();
        m_Menu = typeof(JewlReward2Gump);
        m_Box = new DisplayBox2();

        ItemID = 0xEDC;
        Name = "Stone Vendor";

        WorldReward2Vendors.RegisterVendor(this);

        m_Box_Holder = new StoneReward2Holder(this);
        m_Box_Holder.DropItem(m_Box);      

        this.Movable = false;
    }
    public override void OnLocationChange( Point3D oldLocation )
    {
        base.OnLocationChange(oldLocation);

        if (m_Box_Holder != null)
        {
            m_Box_Holder.Location = this.Location;
            m_Box_Holder.Z = this.Z - 10;
        }
    }
    public override void OnMapChange()
    {
        base.OnMapChange();

        if ( m_Box_Holder != null )
            m_Box_Holder.Map = this.Map;
    }
    Mobile IReward2Vendor.GetMobile()
    {
        return null;
    }
    Item IReward2Vendor.GetItem()
    {
        return this;
    }

    bool IReward2Vendor.IsRemoved()
    {
        return this.Deleted;
    }
    Container IReward2Vendor.GetContainer()
    {
        return m_Box_Holder;
    }
    string IReward2Vendor.GetName()
    {
        return this.Name;
    }
    Map IReward2Vendor.GetMap()
    {
        return this.Map;
    }
    Point3D IReward2Vendor.GetLocation()
    {
        return this.Location;
    }
    void IReward2Vendor.AddReward2(Reward2 r)
    {
        m_Box.AddDisplay(r);
        m_Reward2s.Add(r);
    }
    void IReward2Vendor.RemoveReward2(Reward2 r)
    {
        m_Reward2s.Remove(r);
        m_Box.RemoveDisplay(r);
    }

    void IReward2Vendor.CopyVendor(IReward2Vendor vendor)
    {
        Item currBox = m_Box_Holder.FindItemByType(typeof(DisplayBox2));
        currBox.Delete();

        m_Currency2 = vendor.Payment;
        m_Menu = vendor.Menu;

        m_Reward2s = (Reward2Collection)((vendor.Reward2s as ICloneable).Clone());
        m_Box = new DisplayBox2(m_Reward2s);

        m_Box_Holder.DropItem(m_Box);
    }
    //End Warning
    public override void OnDelete()
    {
        if ( m_Box_Holder != null)
            m_Box_Holder.Delete();

        WorldReward2Vendors.RemoveVendor(this);
        base.OnDelete();
    }
    
    public override void OnDoubleClick(Mobile m)
    {
        if (m_Box_Holder != null)
        {
            m_Box_Holder.MoveToWorld(this.Location, this.Map);
            m_Box_Holder.Z = this.Z - 10;
            m_Box_Holder.UpdateName();
        }
        MenuUploader2.Display(m_Menu, m, this, false);
    }
     
    public StoneReward2Vendor(Serial serial)
        : base(serial)
    {
    }

    public override void Serialize(GenericWriter writer)
    {
        base.Serialize(writer);

        writer.Write((int)0); //Version 0

        int count = m_Reward2s.Count;
        writer.Write((int)count);

        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                writer.WriteItem((Reward2)m_Reward2s[i]);
            }
        }

        writer.WriteItem((Currency2)m_Currency2);
        writer.Write((string)m_Menu.Name);
        writer.WriteItem((DisplayBox2)m_Box);
        writer.WriteItem((StoneReward2Holder)m_Box_Holder);
    }

    public override void Deserialize(GenericReader reader)
    {
        base.Deserialize(reader);

        WorldReward2Vendors.RegisterVendor(this);

        int version = reader.ReadInt();

        m_Reward2s = new Reward2Collection();

        int count = reader.ReadInt();
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                Reward2 r = (Reward2)reader.ReadItem();
                m_Reward2s.Add(r);
            }
        }

        m_Currency2 = (Currency2)reader.ReadItem();

        try { string name = reader.ReadString(); m_Menu = ScriptCompiler.FindTypeByName(name); }
        catch { m_Menu = typeof(JewlReward2Gump); }

        m_Box = (DisplayBox2)reader.ReadItem();
        m_Box_Holder = (StoneReward2Holder)reader.ReadItem();
    }
}
public interface IReward2Vendor
{
    //used to determine how player pays
    Currency2 Payment { get; set; }
    //to keep added items
    Reward2Collection Reward2s { get; set; }
    //to change gump
    Type Menu { get; set; }
    //to show player items
    DisplayBox2 Display { get; }
    //to return what holds 'DisplayBox' object
    Container GetContainer();
    //to return removal status
    bool IsRemoved();
   
    //serial /deserial
    Mobile GetMobile();
    Item GetItem();

    //used for 'world collection'
    string GetName();
    Point3D GetLocation();
    Map GetMap();

    //to manage RewardCollection
    void AddReward2(Reward2 r);
    void RemoveReward2(Reward2 r);
    
    //to create vendor clones
    void CopyVendor(IReward2Vendor vendor);
}
public interface IReward2VendorGump
{
    void Send(Mobile m);
    void AddEntry(Reward2 r);
    void CreateBackground();
    
    //add entry || background || 
}


public class PaymentTarget2 : Target
{
    private string m_PropertyName;
    private IReward2Vendor m_Vendor;

    public PaymentTarget2()
        : base(2, false, TargetFlags.None)
    {
    }
    //by item
    public PaymentTarget2(IReward2Vendor vendor) : this ()
	{
        m_Vendor = vendor;
        m_PropertyName = null;
    }
    //by item property
    public PaymentTarget2(IReward2Vendor vendor, string propName) : this()
    {
        m_Vendor = vendor;
        m_PropertyName = propName;
    }
    protected override void OnTarget(Mobile m, object targeted)
    {
        if ( targeted is Item )
        {
            //consume by item
            if (m_PropertyName == null)
            {
                m_Vendor.Payment = new Currency2((Item)targeted);
                m.SendMessage("{0} will now use {1} for payment.", m_Vendor.GetName(), targeted.GetType().Name);
            }
            //consume by item property
            else
            {
                SetByProperty(m, targeted);
            }
        }
        else if (targeted is PlayerMobile)
        {
            if (m_PropertyName == null)
            {
                m.SendMessage("You can only use Property option for player.");

            }
            else
            {
                SetByProperty(m, null);
            }
        }
        else
        {
            m.SendMessage("Invalid, target must be Item or Player.");
        }

        MenuUploader2.Display(m_Vendor.Menu, m, m_Vendor, false);
    }
    private void SetByProperty(Mobile m, object o)
    {
        string msg = "Please make sure you entered the Property's name correctly, "
                        + "it IS case sensitive.";
        try
        {
            if (o != null)//Item
            {
                PropertyInfo pi = o.GetType().GetProperty(m_PropertyName);

                if (pi != null && pi.PropertyType.Equals(typeof(Int32)))
                {
                    m_Vendor.Payment = new Currency2((Item)o, pi);
                    m.SendMessage(m_Vendor.GetName() + "will now use {1}.{2} for payment.", m_Vendor.GetName(), o.GetType().Name, pi.Name);
                }
                else
                    m.SendMessage(msg);
            }
            else//Mobile
            {
                PropertyInfo pi = m.GetType().GetProperty(m_PropertyName);

                if (pi != null && pi.PropertyType.Equals(typeof(Int32)))
                {
                    m_Vendor.Payment = new Currency2(m, pi);
                    m.SendMessage("{0} will now use {1}.{2} for payment.", m_Vendor.GetName(), m.GetType().Name, pi.Name);
                }
                else
                    m.SendMessage(msg);
            }
        }
        catch { m.SendMessage(msg); }
    }
}
public class AddItemTarget2 : Target
{
    IReward2Vendor m_Vendor;

    public AddItemTarget2() : base( 2, false, TargetFlags.None )
	{
	}
    public AddItemTarget2(IReward2Vendor vendor)
        : this()
    {
        m_Vendor = vendor;
    }
    protected override void OnTarget(Mobile from, object targeted)
    {
        Item i = targeted as Item;

        if (m_Vendor == null || i == null)
            return;

        Item clone = ItemClone2.Clone(i);

        if (clone != null) //copy did not fail
            from.SendGump(new EditItemGump2(m_Vendor, new Reward2(clone), null, false, true));
        else
            from.SendMessage("This item cannot be copied.");

    }
}

public class ItemClone2
{
    public static Item Clone(Item item) 
    {
        try
        {
            if ( item is Container )
                return CloneBag(((Container)item).FindItemsByType(typeof(Item), false), (Container)item, (Container)Activator.CreateInstance(item.GetType(), null));
            else
                return Clone(item.GetType(), item, (Item)Activator.CreateInstance(item.GetType(), null));
        }
        catch {}
       
        return null;
    }
    public static Container CloneBag(Item[] contents, Container old, Container copy)
    {        
        //emtpy new container
        foreach (Item i in copy.FindItemsByType(typeof(Item)))
        {
            i.Delete();
        }

        //'old' contents into 'new'
        foreach (Item i in contents)
        {
            copy.DropItem(Clone(i));           
        }

        copy.Name = old.Name; //requires manual set

        return copy;
    }
    public static Item Clone(Type type, Item old, Item copy)
    {
        foreach (PropertyInfo pi in type.GetProperties())
        {
            if (pi.CanRead && pi.CanWrite && ValidProperty(pi.Name)) 
            {
                try
                {
                    if (pi.PropertyType.IsClass)
                        CloneInnerClass(pi.PropertyType, pi.GetValue(old, null), pi.GetValue(copy, null)); //found object property
                    else
                        pi.SetValue(copy, pi.GetValue(old, null), null); //found value property
                }
                catch {}
            }
        }

        copy.Name = old.Name; //requires manual set

        return copy;
    }
    public static void CloneInnerClass(Type type, object old, object copy)
    {
        if (old == null) //not defined
            return;
        
        foreach ( PropertyInfo pi in type.GetProperties() )
        {
            if ( pi.CanRead && pi.CanWrite ) 
            {
                try
                {
                    pi.SetValue(copy, pi.GetValue(old, null), null);
                }
                catch {}
            }
        }
    }
    public static bool ValidProperty(string str) 
    {
        return !( str.Equals("Parent") || str.Equals("TotalWeight") || str.Equals("TotalItems") || str.Equals("TotalGold") );
    }
   
}
