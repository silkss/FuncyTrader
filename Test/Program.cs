using IBApi;
using System;
using Test;

var wrapper = new IbWrapper();

wrapper.Connect();

var contract = new Contract() {
    Symbol = "GC",
    SecType = "FOP",
    LastTradeDateOrContractMonth = "20240125",
    Exchange = "COMEX"
};

wrapper.ReqContract(contract);

var working = true;
while (working) {

    switch (Console.ReadLine()?.Trim().ToLower()) {
        case "exit":
            working = false;
            break;
        case "clear":
            Console.Clear();
            break;
        case "print":
            wrapper.Print();
            break;
        default:
            break;
    }
}