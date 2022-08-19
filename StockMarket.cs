using System;
using System.Collections.Generic;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            //매도 주문서
            public class StockSellOrder {       
                public int customer;         
                public string resourceId = string.Empty;                
                public float bid;
            }
            //참가자
            public class StockCustomer {
                //구매가
                private Dictionary<string, float> mPurchasePrices = new Dictionary<string, float>();
                //구매 수량
                private Dictionary<string, int> mPurchaseQuantities = new Dictionary<string, int>();
                private int mId;
                private float mMoney;                
                private float mPurchaseIntention;                
                public StockCustomer(int id, float money, float purchaseIntention) {
                    mId = id;
                    Init(money, purchaseIntention);
                }
                public void Init(float money, float purchaseIntention) {
                    mMoney = money;
                    mPurchaseIntention = purchaseIntention;
                }
                public void SetDefault(string resourceId, float price, int quantity) {
                    mPurchasePrices[resourceId] = price;
                    mPurchaseQuantities[resourceId] =  quantity;
                }
                public float GetMoney() {
                    return mMoney;
                }
                public Dictionary<string, int> GetPurchaseQuantities() {
                    return mPurchaseQuantities;
                }
                public void DecisionSell(string resourceId, List<float> prices) {
                    float marketPrice = prices[prices.Count-1];
                    float gradient = GetGradient(prices);                    

                    // 예측에 따른 매도
                    // 가격이 떨어질 것으로 예상 하거나, 고점이라고 판단했을때 
                    float rate = 1;
                    if(mMoney <= StockMarketHandler.Instance.MIN_MONEY) {//보유 금액이 부족하면 급매를 한다.
                        rate = 0.8f;
                    } else if(StockMarketHandler.Instance.GetDefaultPrice(resourceId) * 2f <= marketPrice) { //기준 가격대비 2배 이상이면 판다. 고평가 판단
                        rate = 0.9f;
                    } else if(gradient <= 0) { 
                        if(IsMinOrMax(prices, false)) { //저점
                            rate = 1.02f;
                        } else {
                            rate = 0.98f;
                        }
                    } else if(IsMinOrMax(prices, true)) { //고점 판단
                        rate = 0.98f;
                    } else {                        
                        return;
                    }
                    Sell(resourceId, marketPrice, rate);
                }
                public void DecisionBuy(string resourceId, List<float> prices) {
                    float marketPrice = prices[prices.Count-1];
                    float gradient = GetGradient(prices);

                    //시장 가격에 따른 매수. 
                    var purchasableOrder = StockMarketHandler.Instance.GetPurchasableOrder(resourceId);
                    if( (purchasableOrder != null && marketPrice * 0.9f > purchasableOrder.bid) ) {
                        Buy(resourceId, purchasableOrder.bid);                        
                    }                   
                    //예측에 따른 매수                    
                    else if(gradient > 0) { //가격이 오를 것으로 예상 되거나 저점이라고 판단되면 
                        if(IsMinOrMax(prices, true)) { //고점
                            Buy(resourceId, marketPrice * 0.98f);
                        } else {
                            Buy(resourceId, marketPrice * 1.0f);
                        }                        
                    } else {
                        if(IsMinOrMax(prices, false)) { //저점
                            Buy(resourceId, marketPrice * 1.05f);
                            //Buy(resourceId, marketPrice * 1.05f);
                        } else if(gradient * -1 < mPurchaseIntention) { //하락세 여도 mPurchaseIntention 이하로 떨어 지면 구매
                            Buy(resourceId, marketPrice * 0.95f);
                        }
                    }
                }
                private void Sell(string resourceId, float marketPrice, float rate) {
                    if(mPurchaseQuantities[resourceId] > 0) {
                        mPurchaseQuantities[resourceId] --;

                        StockSellOrder order = new StockSellOrder();
                        order.customer = mId;
                        order.resourceId = resourceId;
                        order.bid = marketPrice * rate;
                        if(!StockMarketHandler.Instance.Sell(order)) {                            
                            order.bid *= 0.99f;
                            //한번 정도만 더 낮게 주문.
                            if(!StockMarketHandler.Instance.Sell(order)) {
                                ReturnBackOrder(order);
                            }
                        }
                    }
                }
                private void Buy(string resourceId, float expectedPrice) {
                    StockMarketHandler.Instance.Buy(mId, resourceId, expectedPrice);
                }
                public void OnSell(StockSellOrder order) {                    
                    //판매 대금 받는다
                    mMoney += order.bid;
                }
                public bool OnBuy(StockSellOrder order) {                    
                    if(mMoney - order.bid < 0 )
                        return false;
                    mMoney -= order.bid;
                    mPurchasePrices[order.resourceId] = order.bid;
                    mPurchaseQuantities[order.resourceId]++;
                    return true;
                }
                public void ReturnBackOrder(StockSellOrder order) {
                    mPurchaseQuantities[order.resourceId]++;                    
                }
                private float GetGradient(List<float> prices) {
                    float gradient = 0;
                    for(int i = 0; i < prices.Count - 1; i++) {
                        float diff = (prices[i+1] - prices[i]);
                        gradient += diff;
                    }
                    return gradient;
                }
                //고점, 저점 판단
                private bool IsMinOrMax(List<float> prices, bool isIncrease) {
                    for(int i = 0; i < prices.Count - 1; i++) {
                        float diff = (prices[i+1] - prices[i]);
                        if(isIncrease && diff < 0)
                            return false;
                        if(!isIncrease && diff > 0)
                            return false;
                    }
                    return true;
                }
            }
            //주식시장
            public class StockMarketHandler {
                private const int CAPACITY = 3;
                private const int CUSTOMERS = 30;                
                public int MIN_MONEY { get; } = 100;
                public int MAX_MONEY { get; } = 1000;
                private const int DEFAULT_QUANTITY = 100;
                private Dictionary<string, float> mDefaultPrice = new Dictionary<string, float>();
                private Dictionary<string, List<float>> mMarketPrice = new Dictionary<string, List<float>>();
                private List<float> mTemp = new List<float>();
                private List<StockCustomer> mCustomers = new List<StockCustomer>();           
                private Dictionary<string, SortedList<float, StockSellOrder>> mSellOrders = new Dictionary<string, SortedList<float, StockSellOrder>>();
                private Random mRandom = new Random();
                private int cntSell, cntBuy;
                private static readonly Lazy<StockMarketHandler> instance =
                        new Lazy<StockMarketHandler>(() => new StockMarketHandler());
                public static StockMarketHandler Instance {
                    get {
                        return instance.Value;
                    }
                }
                private StockMarketHandler() { }
                public void Init() {
                    var satisfactions = SatisfactionDefine.Instance.GetAll();
                    float defaultPrice = 8;
                    foreach(var p in satisfactions) {
                        if(p.Value.resource) {
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
                public float GetDefaultPrice(string resourceId) {
                    return mDefaultPrice[resourceId];
                }
                public void Update() {
                    Console.WriteLine("========================================= Sell: {0} Buy: {1} =========================================", cntSell, cntBuy);
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
                        
                        //살때
                        for(int i = 0; i < mCustomers.Count; i++) {  
                            int idx = mRandom.Next(mCustomers.Count);                          
                            mCustomers[idx].DecisionBuy(resourceId, mTemp);
                        }   

                        Print(resourceId);
                    }
                    /*
                    for(int i = 0; i < mCustomers.Count; i++) {                                                        
                        Console.WriteLine("{0}, {1}", i, mCustomers[i].GetMoney());
                    } 
                    */
                }
                private void Print(string resourceId) {
                    Console.WriteLine("{0} Orders: {1} Market Price {2} > {3} > {4}", resourceId, mSellOrders[resourceId].Count, 
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
                public bool Sell(StockSellOrder order) {
                    if(mSellOrders[order.resourceId].ContainsKey(order.bid))
                        return false;

                    mSellOrders[order.resourceId].Add(order.bid, order);
                    cntSell++;
                    return true;
                }
                public void Buy(int customer, string resourceId, float maxPrice) {                    
                    if(mSellOrders[resourceId].Count > 0) {                        
                        StockSellOrder order = mSellOrders[resourceId].Values[0];
                        if(order.bid <= maxPrice) {
                            //매수자 update
                            if(mCustomers[customer].OnBuy(order)) {
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