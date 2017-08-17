
/// <summary>
/// 非mono单例
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonTemplate<T> where T : class, new()
{
    protected static T mSingleton = null;

    
    public static T Singleton
    {
        get
        {
            if (mSingleton == null)
            {
                mSingleton = new T();
            }
            return mSingleton;
        }
    }
}
