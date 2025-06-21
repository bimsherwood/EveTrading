
public class BackTest {

    private readonly IOrderSequence SignalSeries;

    public BackTest(IOrderSequence signal) {
        this.SignalSeries = signal;
    }

    public decimal TradeWith(decimal startingCash, decimal salesTaxRate) {

        var orders = this.SignalSeries.Orders;

        var cash = startingCash;
        var units = 0;
        for (var i = 0; i < orders.Count; i++) {

            var order = orders[i];

            // Buy
            if (order.Signal == Signal.Buy) {
                var targetSpend = cash;
                var unitsBought = (int)(targetSpend / order.BuyPrice);
                var actualSpend = unitsBought * order.BuyPrice;
                units += unitsBought;
                cash -= actualSpend;
            }

            // Sell
            if (order.Signal == Signal.Sell) {
                var unitsSold = units;
                var saleRevenue = unitsSold * order.SellPrice;
                var salesTax = saleRevenue * salesTaxRate;
                cash += saleRevenue - salesTax;
                units -= unitsSold;
            }

        }

        var finalSalesPrice = this.SignalSeries.FinalSalePrice;
        var finalSaleRevenue = units * finalSalesPrice;
        var finalSalesTax = finalSaleRevenue * salesTaxRate;
        cash += finalSaleRevenue - finalSalesTax;
        return cash;

    }

}