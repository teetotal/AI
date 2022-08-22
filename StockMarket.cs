using ENGINE;
using System.Collections.Generic;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            //매도 주문서.
            public class StockSellOrder {       
                public int customer;         
                public string resourceId = string.Empty;                
                public float bid;
            }
            public class StockSellOrderPool : Singleton<StockSellOrderPool> {
                private ObjectPool<StockSellOrder> mPool = new ObjectPool<StockSellOrder>();
                public StockSellOrderPool() { }
                public ObjectPool<StockSellOrder> GetPool() {
                    return mPool;
                }
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
                    float expectedPrice = 0;
                    var purchasableOrder = StockMarketHandler.Instance.GetPurchasableOrder(resourceId);
                    if( (purchasableOrder != null && marketPrice * 0.9f > purchasableOrder.bid) ) {
                        expectedPrice = purchasableOrder.bid;                      
                    }                   
                    //예측에 따른 매수                    
                    else if(gradient > 0) { //가격이 오를 것으로 예상 되거나 저점이라고 판단되면 
                        if(IsMinOrMax(prices, true)) { //고점
                            expectedPrice = marketPrice * 0.98f;
                        } else {
                            expectedPrice = marketPrice * 1.0f;
                        }                        
                    } else {
                        if(IsMinOrMax(prices, false)) { //저점
                            expectedPrice = marketPrice * 1.05f;
                        } else if(gradient * -1 < mPurchaseIntention) { //하락세 여도 mPurchaseIntention 이하로 떨어 지면 구매
                            expectedPrice = marketPrice * 0.95f;
                        } else {
                            return;
                        }
                    }
                    Buy(resourceId, expectedPrice);      
                }
                private void Sell(string resourceId, float marketPrice, float rate) {
                    if(mPurchaseQuantities[resourceId] > 0) {
                        mPurchaseQuantities[resourceId] --;

                        StockSellOrder order = StockSellOrderPool.Instance.GetPool().Alloc();

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
                public bool OnBuy(string resourceId, float bid) {                    
                    if(mMoney - bid < 0 )
                        return false;
                    mMoney -= bid;
                    mPurchasePrices[resourceId] = bid;
                    mPurchaseQuantities[resourceId]++;
                    return true;
                }
                public void ReturnBackOrder(StockSellOrder order) {
                    mPurchaseQuantities[order.resourceId]++;      
                    StockSellOrderPool.Instance.GetPool().Release(order);              
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
        }
    }
}