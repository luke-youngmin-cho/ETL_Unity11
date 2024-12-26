namespace SynchronizationPractices
{
    internal class LazySingleton<T>
        where T : LazySingleton<T>
    {
        internal static T Instance => s_instance.Value;

        static readonly Lazy<T> s_instance = new Lazy<T>(() => (T)Activator.CreateInstance(typeof(T)));
    }
}
