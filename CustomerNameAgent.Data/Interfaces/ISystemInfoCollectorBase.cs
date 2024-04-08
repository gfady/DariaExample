namespace CustomerNameAgent.Data;

public interface ISystemInfoCollectorBase
{
    Task CollectIfNeededAsync();
}