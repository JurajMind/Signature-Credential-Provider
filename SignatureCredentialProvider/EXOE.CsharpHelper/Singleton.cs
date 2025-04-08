using System;
using System.Reflection;

namespace EXOE.CsharpHelper
{
    //http://www.codeproject.com/Articles/14026/Generic-Singleton-Pattern-using-Reflection-in-C
    /// <summary>
    ///     MultiThread supported
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Singleton<T> where T : class
    {
        private static volatile T _instance;
            // Le mot clé volatile indique qu'un champ peut être modifié par plusieurs threads qui s'exécutent simultanément. Les champs qui sont déclarés volatile ne sont pas soumis aux optimisations du compilateur qui supposent l'accès par un seul thread. Cela garantit que la valeur la plus à jour est présente dans le champ à tout moment.

        // Le modificateur volatile est généralement utilisé pour un champ auquel accèdent plusieurs threads sans utiliser l'instruction lock, instruction (Référence C#) pour sérialiser l'accès.
        private static object _lock = new object();

        static Singleton()
        {
        }

        public static T Instance
        {
            get { return (_instance != null) ? _instance : ResetAndGetInstance(); }
        }

        private static T ResetAndGetInstance()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    GetInstance();
                }
            }
            return _instance;
        }

        private static void GetInstance()
        {
            ConstructorInfo constructor = null;
            try
            {
                constructor = typeof (T).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null,
                    new Type[0], null); // Binding flags exclude public constructors.
            }
            catch (Exception excep)
            {
                throw new SingletonException(excep);
            }
            if (constructor == null || constructor.IsAssembly)
            {
                throw new SingletonException(string.Format("A private or protected constructor is missing for '{0}'.",
                    typeof (T).Name));
            }
            _instance = (T) constructor.Invoke(null);
        }
    }
}