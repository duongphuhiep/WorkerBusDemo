using Shouldly;
using Xunit.Abstractions;

namespace TestProject1;

public class AsyncLocalTest(ITestOutputHelper testOutputHelper)
{
    AsyncLocal<int> asyncLocal = new AsyncLocal<int>();

    [Fact]
    public async Task Case1()
    {
        asyncLocal.Value = 1;
        await Task.Run(() =>
        {
            asyncLocal.Value = 2;
            asyncLocal.Value.ShouldBe(2);
        });
        await Task.Run(() => { asyncLocal.Value.ShouldBe(1); });
    }

    [Fact]
    public async Task Case2()
    {
        asyncLocal.Value = 1;
        await Task.WhenAll(
            Task.Run(async () =>
            {
                await Task.Delay(20);
                testOutputHelper.WriteLine("Modify asyncLocal in Task 1");
                asyncLocal.Value = 2;
                asyncLocal.Value.ShouldBe(2);
            }),
            Task.Run(async () =>
            {
                await Task.Delay(100);
                testOutputHelper.WriteLine("Read asyncLocal in Task 2");
                asyncLocal.Value.ShouldBe(1);
            }));
    }
    
    [Fact]
    public async Task Case3()
    {
        asyncLocal.Value = 1;
        var t1 = Task.WhenAll(
            Task.Run(async () =>
            {
                await Task.Delay(20);
                testOutputHelper.WriteLine("Modify asyncLocal in Task 1");
                asyncLocal.Value = 2;
                asyncLocal.Value.ShouldBe(2);
                await Task.WhenAll(
                    Task.Run(async () =>
                    {
                        await Task.Delay(30);
                        testOutputHelper.WriteLine("Modify asyncLocal in Task 1.1");
                        asyncLocal.Value = 3;
                        asyncLocal.Value.ShouldBe(3);
                    }),
                    Task.Run(async () =>
                    {
                        await Task.Delay(40);
                        testOutputHelper.WriteLine("Read asyncLocal in Task 1.2");
                        asyncLocal.Value.ShouldBe(2);
                    }));
            }),
            Task.Run(async () =>
            {
                await Task.Delay(100);
                testOutputHelper.WriteLine("Read asyncLocal in Task 2");
                asyncLocal.Value.ShouldBe(1);
            }));
        
        asyncLocal.Value = 5;
        var t2 = Task.Run(async () =>
        {
            await Task.Delay(100);
            asyncLocal.Value.ShouldBe(5);
        });
        await Task.WhenAll(t1, t2);
    }
}