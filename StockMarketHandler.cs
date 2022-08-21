using System;
using System.Collections.Generic;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            //pooling
            public class StockActorSold {
                public string resourceId = string.Empty;
                public float sellingPrice;
            }
            //pooling
            public class StockActorPurchased {
                public string resourceId = string.Empty;
                public float bid; //선 납입 금액
                public float purchasedPrice; //실제 구매 금액
            }
            //pooling
            public class StockActorOrder {
                public bool isSell;
                public Actor? actor;
                public string resourceId = string.Empty;     
                public int quantity;           
                public float bid;
                public void Set(bool isSell, Actor actor, string resourceId, int quantity, float bid) {
                    this.isSell = isSell;
                    this.actor = actor;
                    this.resourceId = resourceId;
                    this.quantity = quantity;
                    this.bid = bid;
                }
            }
            //주식시장
            public class StockMarketHandler {
                public int CAPACITY = 3;
                private const int CUSTOMERS = 30;                
                public int MIN_MONEY { get; } = 100;
                public int MAX_MONEY { get; } = 1000;
                private const int DEFAULT_QUANTITY = 100;
                private Dictionary<string, float> mDefaultPrice = new Dictionary<string, float>();
                private Dictionary<string, List<float>> mMarketPrice = new Dictionary<string, List<float>>();
                private List<float> mTemp = new List<float>();
                private List<StockCustomer> mCustomers = new List<StockCustomer>();           
                private Dictionary<string, SortedList<float, StockSellOrder>> mSellOrders = new Dictionary<string, SortedList<float, StockSellOrder>>();
                //Actor 주문
                //actor id, resource id, orders
                private Dictionary<string, Dictionary<string, List<StockActorOrder>>> mActorSellOrders = new Dictionary<string, Dictionary<string, List<StockActorOrder>>>();
                private Dictionary<string, List<StockActorOrder>> mActorBuyOrders = new Dictionary<string, List<StockActorOrder>>();
                private Dictionary<string, List<StockActorPurchased>> mActorPurchased = new Dictionary<string, List<StockActorPurchased>>();
                private Dictionary<string, List<StockActorSold>> mActorSold = new Dictionary<string, List<StockActorSold>>();
                // --------
                private Random mRandom = new Random();
                private long mLastUpdate = 0;
                private int mUpdateInterval = 0;
                private int cntSell, cntBuy;
                private bool mIsInit = false;
                private static readonly Lazy<StockMarketHandler> instance =
                        new Lazy<StockMarketHandler>(() => new StockMarketHandler());
                public static StockMarketHandler Instance {
                    get {
                        return instance.Value;
                    }
                }
                private StockMarketHandler() { }
                public void Init() {
                    if(mIsInit)
                        return;
                    mIsInit = true;
                    mUpdateInterval = 1;

                    var satisfactions = SatisfactionDefine.Instance.GetAll();
                    float defaultPrice = 8;
                    foreach(var p in satisfactions) {
                        if(p.Value.type == SATISFACTION_TYPE.RESOURCE) {
                            //default price
                            mDefaultPrice[p.Key] = 8;
                            //market price
                            mMarketPrice[p.Key] = new List<float>(CAPACITY);
                            for(int i = 0; i < CAPACITY; i++ ) {
                                float ran = mRandom.Next(10) / 10.0f;
                                if(mRandom.Next(10) < 5)
                                    ran *= -1;
                                EnqueuePrice(p.Key, defaultPrice + ran);
                            }
                            //sell order
                            mSellOrders[p.Key] = new SortedList<float, StockSellOrder>();
                        }
                    }

                    CreateCustomers();
                }
                public Dictionary<string, List<float>> GetMarketPrices() {
                    return mMarketPrice;
                }
                public List<float> GetMarketPrices(string resourceId) {
                    return mMarketPrice[resourceId];
                }
                public float GetMarketPrice(string resourceId) {
                    return mMarketPrice[resourceId][CAPACITY-1];
                }
                public List<StockActorSold>? GetActorSold(string actorId) {
                    if(mActorSold.ContainsKey(actorId))
                        return mActorSold[actorId];
                    return null;
                }
                public List<StockActorPurchased>? GetActorPuchased(string actorId) {
                    if(mActorPurchased.ContainsKey(actorId))
                        return mActorPurchased[actorId];
                    return null;
                }
                public float GetDefaultPrice(string resourceId) {
                    return mDefaultPrice[resourceId];
                }
                public void Update() {
                    long counter = CounterHandler.Instance.GetCount();

                    if(mUpdateInterval > counter - mLastUpdate) 
                        return;
                    
                    mLastUpdate = counter;
                    //Console.WriteLine("========================================= Sell: {0} Buy: {1} =========================================", cntSell, cntBuy);
                    foreach(var p in mMarketPrice) {
                        string resourceId = p.Key;
                        List<float> prices = p.Value;

                        mTemp.Clear();
                        for(int i = 0; i < prices.Count; i++) {
                            mTemp.Add(prices[i]);
                        }
                        //시장 가격 조정
                        if(mSellOrders[resourceId].Count > 0) {
                            //주문 체결 안된건 돌려준다.                        
                            foreach(var order in mSellOrders[resourceId]) {
                                mCustomers[order.Value.customer].ReturnBackOrder(order.Value);
                            }
                            mSellOrders[resourceId].Clear();

                            //시장 가격을 1%내린다.
                            InsertMarketPrice(resourceId, prices[prices.Count - 1] * 0.99f);
                        }
                        //Sell
                        for(int i = 0; i < mCustomers.Count; i++) {       
                            int idx = mRandom.Next(mCustomers.Count);
                            mCustomers[idx].DecisionSell(resourceId, mTemp);
                        }     
                        //아무도 팔지 않으면 시장 가격을 올린다.
                        if(mSellOrders[resourceId].Count == 0) {
                            InsertMarketPrice(resourceId, prices[prices.Count - 1] * 1.01f);
                            mTemp.Clear();
                            for(int i = 0; i < prices.Count; i++) {
                                mTemp.Add(prices[i]);
                            }
                        }
                        //Actor buying
                        ActorBuying();
                        //Buy
                        for(int i = 0; i < mCustomers.Count; i++) {  
                            int idx = mRandom.Next(mCustomers.Count);                          
                            mCustomers[idx].DecisionBuy(resourceId, mTemp);
                        }   
                    }
                }
                private void ActorBuying() {
                    List<float> keys = new List<float>();
                    List<int> actorOrderIdx = new List<int>();
                    foreach(var p in mActorBuyOrders) {
                        actorOrderIdx.Clear();
                        for(int i = 0; i < p.Value.Count; i++) {
                            
                            keys.Clear();

                            StockActorOrder order = p.Value[i];
                            if(order.actor == null)
                                throw new Exception("StockMarket null actor failure");
                            int quantity = order.quantity;
                            foreach(var o in mSellOrders[order.resourceId] ) {
                                if(o.Value.bid <= order.bid) {
                                    keys.Add(o.Key);
                                    //계좌에 저장
                                    AddActorPurchased(order.actor, o.Value, order.bid);

                                    //매도자 callback
                                    mCustomers[o.Value.customer].OnSell(o.Value);
                                    InsertMarketPrice(order.resourceId, o.Value.bid);
                                    quantity--;
                                    if(quantity == 0)
                                        break;
                                } else {
                                    break;
                                }
                            }
                            if(quantity == 0) {
                                actorOrderIdx.Add(i);
                            } else {
                                p.Value[i].quantity = quantity; //수량 조절
                            }
                            
                            if(keys.Count > 0) {
                                for(int j = 0; j < keys.Count; j++)
                                    mSellOrders[order.resourceId].Remove(keys[j]);
                                //callback
                                order.actor.OnStockBuy();
                            }
                        }
                        for(int j = 0; j < actorOrderIdx.Count; j++)
                            p.Value.RemoveAt(actorOrderIdx[j]);
                    }
                }
                private void AddActorPurchased(Actor actor, StockSellOrder order, float bid) {
                    StockActorPurchased p = new StockActorPurchased();
                    p.resourceId = order.resourceId;
                    p.purchasedPrice = order.bid;
                    p.bid = bid;
                    
                    if(!mActorPurchased.ContainsKey(actor.mUniqueId)) {
                        mActorPurchased[actor.mUniqueId] = new List<StockActorPurchased>();
                    }
                    mActorPurchased[actor.mUniqueId].Add(p);
                }
                private void AddActorSold(Actor actor, string resourceId, float price) {
                    StockActorSold p = new StockActorSold();
                    p.resourceId = resourceId;
                    p.sellingPrice = price;

                    if(!mActorSold.ContainsKey(actor.mUniqueId)) {
                        mActorSold[actor.mUniqueId] = new List<StockActorSold>();
                    }
                    mActorSold[actor.mUniqueId].Add(p);
                }
                public string Print(string resourceId) {
                    return string.Format("{0} Orders: {1} Market Price {2} > {3} > {4}", 
                                        SatisfactionDefine.Instance.GetTitle(resourceId), 
                                        mSellOrders[resourceId].Count, 
                                        mMarketPrice[resourceId][0],
                                        mMarketPrice[resourceId][1],
                                        mMarketPrice[resourceId][2]
                                        );
                    /*
                    int i = 0;
                    foreach(var order in mSellOrders[resourceId]) {                        
                        Console.WriteLine(" - Customer: {0} Bid: {1}", order.Value.customer, order.Value.bid);
                        i++;
                        if(i >= 3)
                            return;
                    }
                    */
                }
                //Actor 주문
                public void Order(StockActorOrder order) {
                    if(order.actor == null) {
                        throw new Exception("StockMarket actor must be assigned.");
                    }
                    string actorId = order.actor.mUniqueId;
                    if(order.isSell) {
                        if(!mActorSellOrders.ContainsKey(actorId)) 
                            mActorSellOrders[actorId] = new Dictionary<string, List<StockActorOrder>>();
                        
                        if(!mActorSellOrders[actorId].ContainsKey(order.resourceId))
                            mActorSellOrders[actorId][order.resourceId] = new List<StockActorOrder>();

                        mActorSellOrders[actorId][order.resourceId].Add(order);
                    } else {
                        if(!mActorBuyOrders.ContainsKey(actorId)) {
                            mActorBuyOrders[actorId] = new List<StockActorOrder>();
                        }
                        mActorBuyOrders[actorId].Add(order);
                    }
                }
                public bool Sell(StockSellOrder order) {
                    if(mSellOrders[order.resourceId].ContainsKey(order.bid))
                        return false;

                    mSellOrders[order.resourceId].Add(order.bid, order);
                    cntSell++;
                    return true;
                }
                public void Buy(int customer, string resourceId, float maxPrice) {    
                    //Actor 선처리. Customer는 많아서 Actor엑 순번이 안갈 수 있다.
                    if(BuyInActorSells(customer, resourceId, maxPrice))
                        return;

                    if(mSellOrders[resourceId].Count > 0) {                        
                        StockSellOrder order = mSellOrders[resourceId].Values[0];
                        if(order.bid <= maxPrice) {
                            //매수자 update
                            if(mCustomers[customer].OnBuy(order.resourceId, order.bid)) {
                                //매도자 update
                                mCustomers[order.customer].OnSell(order);

                                InsertMarketPrice(resourceId, order.bid);
                                mSellOrders[resourceId].RemoveAt(0);
                                cntBuy++;
                            }
                        }
                    } else {
                        InsertMarketPrice(resourceId, maxPrice);
                    }
                }
                public bool BuyInActorSells(int customer, string resourceId, float maxPrice) {
                    foreach(var actorSell in mActorSellOrders) {
                        if(!actorSell.Value.ContainsKey(resourceId) || actorSell.Value[resourceId].Count == 0)
                            continue;

                        List<StockActorOrder> sells = actorSell.Value[resourceId];
                        for(int i = 0; i < sells.Count; i++) {
                            StockActorOrder order = sells[i];
                            if(order.actor == null) 
                                throw new Exception("BuyInActorSells StockActorOrder must have actor");

                            if(order.bid <= maxPrice) {
                                //매수자 update
                                if(mCustomers[customer].OnBuy(resourceId, order.bid)) {
                                    //매도자 계좌 입금
                                    AddActorSold(order.actor, resourceId, order.bid);

                                    InsertMarketPrice(resourceId, order.bid);
                                    
                                    //quantity 조정
                                    if(order.quantity -1 > 0)
                                        actorSell.Value[resourceId][i].quantity--;
                                    else {
                                        //삭제
                                        actorSell.Value[resourceId].RemoveAt(i);
                                    }
                                    cntBuy++;
                                    //callback
                                    order.actor.OnStockSell();
                                    return true;
                                }
                            }
                        }
                    }
                    return false;
                }
                private void InsertMarketPrice(string resourceId, float price) {
                    int count = mMarketPrice[resourceId].Count;
                    float marketPrice = mMarketPrice[resourceId][count - 1];
                    if(marketPrice != price) {
                        mMarketPrice[resourceId].RemoveAt(0);
                        mMarketPrice[resourceId].Add(price);
                    }
                }
                public StockSellOrder? GetPurchasableOrder(string resourceId) {
                    if(mSellOrders[resourceId].Count > 0) {
                        return mSellOrders[resourceId].Values[0];
                    }
                    return null;
                }
                private void EnqueuePrice(string id, float price) {
                    if(mMarketPrice[id].Count >= CAPACITY)
                        mMarketPrice[id].RemoveAt(0);

                    mMarketPrice[id].Add(price);
                }
                private void CreateCustomers() {
                    for(int i = 0; i < CUSTOMERS; i++) {
                        int money = mRandom.Next(MIN_MONEY, MAX_MONEY);
                        float purchaseIntention = mRandom.Next(1, 10) / 10.0f;                        

                        StockCustomer customer = new StockCustomer(i, money, purchaseIntention);
                        foreach(var p in mMarketPrice) {
                            customer.SetDefault(p.Key, p.Value[0], DEFAULT_QUANTITY);
                        }
                        mCustomers.Add(customer);
                    }
                }
            }
        }
    }
}