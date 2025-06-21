
using EveTrading.EveApi;

public class TestSignal : IOrderSequence {

    public List<Order> Orders => new[]{
            new Order() { Date = DateTime.Now, BuyPrice = 1000m, SellPrice = 1000m, Signal = Signal.Buy },
            new Order() { Date = DateTime.Now, BuyPrice = 2000m, SellPrice = 2000m, Signal = Signal.Sell },
            new Order() { Date = DateTime.Now, BuyPrice = 2000m, SellPrice = 2000m, Signal = Signal.Sell }
        }.ToList();
    public decimal FinalSalePrice => 1m;

    public TestSignal() { }

}