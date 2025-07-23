using AI.Workshop.ConsoleChat.Tools;

Console.WriteLine("Welcome to the AI Workshop Console Chat Tools examples!\r\n");

var tools = new BasicToolsExamples();

// 1) call a tool method directly
//await tools.ItemPriceMethod();

// 2) call a tool method using the shopping cart methods to simulate state
await tools.ShoppingCartMethods();


