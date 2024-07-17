using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionDeletedFaultConsumer : IConsumer<Fault<AuctionDeleted>>
{
    public async Task Consume(ConsumeContext<Fault<AuctionDeleted>> context)
    {
        Console.WriteLine("--> Consuming faulty deleting");

        var exception = context.Message.Exceptions.First();

        if (exception.ExceptionType == "System.ArgumentException")
        {
            // Handle exception
            await context.Publish(context.Message.Message);
        }
        else
        {
            Console.WriteLine("Not an argument exception - update error dashboard somewhere");
        }
    }
}