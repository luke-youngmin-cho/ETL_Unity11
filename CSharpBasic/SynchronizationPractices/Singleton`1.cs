namespace SynchronizationPractices
{
    internal class Singleton<T>
        where T : Singleton<T>
    {
        internal static T Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_initLock)
                    {
                        if (s_initLock == null)
                        {
                            s_instance = (T)Activator.CreateInstance(typeof(T));
                        }
                    }
                }

                return s_instance;
            }
        }

        static T s_instance;
        static object s_initLock = new object();
    }
}
