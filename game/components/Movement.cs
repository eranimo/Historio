using System.Collections.Generic;

public class Movement {
	public Hex currentTarget = null; 
	public Queue<Hex> moveQueue = new Queue<Hex>();
}