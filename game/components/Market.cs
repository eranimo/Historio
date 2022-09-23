public struct Order {
	public ResourceType type;
	public int amount;
	public int price;
}

public class Market {
	public List<Order> buyOrders;
	public List<Order> sellOrders;
	public Dictionary<ResourceType, float> prices;
}

// component on entity that can buy and sell on the market
public class Trader {
	public float money;
}