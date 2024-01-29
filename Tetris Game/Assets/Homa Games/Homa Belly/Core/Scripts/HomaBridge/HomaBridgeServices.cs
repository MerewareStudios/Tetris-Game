using System;
using System.Collections.Generic;
using System.Reflection;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// This class collects all the services that will be initialized by the HomaBridge.
    /// Services can inject themselves to allow more control over the initialization process or be collected
    /// on HomaBridge initialization time and automatically injected.
    /// </summary>
    public static class HomaBridgeServices
    {
        private static readonly List<MediatorBase> Mediators = new List<MediatorBase>();
        private static readonly List<IMediator> OldMediators = new List<IMediator>();
        private static readonly List<IAttribution> Attributions = new List<IAttribution>();
        private static readonly List<AnalyticsBase> Analytics = new List<AnalyticsBase>();
        private static CustomerSupportImplementation _customerSupport = null;
        private static readonly HashSet<Type> InjectedTypes = new HashSet<Type>();

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitializeOnSubsystemRegistration()
        {
            InjectedTypes.Clear();
            Mediators.Clear();
            OldMediators.Clear();
            Attributions.Clear();
            Analytics.Clear();
            _customerSupport = null;
        }

        /// <summary>
        /// Create Mediation, Attribution and Analytics services.
        /// </summary>
        public static int InstantiateServices()
        {
#if !UNITY_EDITOR
            CollectServiceImplementations();
#endif
            return Mediators.Count + Attributions.Count + Analytics.Count + (_customerSupport != null ? 1 : 0);
        }

        public static bool GetMediators(out List<MediatorBase> mediators)
        {
            mediators = Mediators;
            return mediators != null && mediators.Count > 0;
        }

        public static bool GetOldMediators(out List<IMediator> oldMediators)
        {
            oldMediators = OldMediators;
            return oldMediators != null && oldMediators.Count > 0;
        }

        public static bool GetAttributions(out List<IAttribution> attributions)
        {
            attributions = Attributions;
            return attributions != null && attributions.Count > 0;
        }

        public static bool GetAnalytics(out List<AnalyticsBase> analytics)
        {
            analytics = Analytics;
            return analytics != null && analytics.Count > 0;
        }

        public static bool GetCustomerSupport(out CustomerSupportImplementation customerSupport)
        {
            customerSupport = _customerSupport;
            return customerSupport != null;
        }

        [Preserve]
        private static void CollectServiceImplementations()
        {
            foreach (var assembly in GetHomaAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract || type.IsInterface)
                    {
                        continue;
                    }

                    if (!IsServiceType(type))
                        continue;

                    if (InjectedTypes.Contains(type))
                        continue;

                    try
                    {
                        var instance = Activator.CreateInstance(type);
                        Inject(instance);
                    }
                    catch (Exception e)
                    {
                        HomaGamesLog.Error(
                            $"[HomaBridgeServices]: Error creating instance of '{type.Name}': {e.Message}");
                    }
                }
            }
        }

        private static void Inject(object service)
        {
            bool injected = true;
            switch (service)
            {
                case IAnalytics analytics:
                    Analytics.Add(new AnalyticsInterfaceForwarder(analytics));
                    break;
                case AnalyticsBase analyticsBase:
                    Analytics.Add(analyticsBase);
                    break;
                case IAttribution attribution:
                    Attributions.Add(attribution);
                    break;
                case MediatorBase mediatorBase:
                    Mediators.Add(mediatorBase);
                    break;
                case IMediator mediator:
                    OldMediators.Add(mediator);
                    break;
                case CustomerSupportImplementation customerSupportImplementation:
                    _customerSupport = customerSupportImplementation;
                    break;
                default:
                    injected = false;
                    HomaGamesLog.Warning(
                        $"[HomaBridgeServices]: Service of type {service.GetType().Name} could not be injected!");
                    break;
            }

            if (injected)
            {
                InjectedTypes.Add(service.GetType());
            }
        }

        private static bool IsServiceType(Type type)
        {
            return type != typeof(AnalyticsInterfaceForwarder) &&
                   (typeof(MediatorBase).IsAssignableFrom(type)
                    || typeof(AnalyticsBase).IsAssignableFrom(type)
                    || typeof(IMediator).IsAssignableFrom(type)
                    || typeof(IAttribution).IsAssignableFrom(type)
                    || typeof(IAnalytics).IsAssignableFrom(type)
                    || typeof(CustomerSupportImplementation).IsAssignableFrom(type));
        }

        private static IEnumerable<Assembly> GetHomaAssemblies()
        {
            const string runtimeDefaultAssemblyName = "Assembly-CSharp";
            const string homaAssembliesPrefix = "HomaGames";
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                {
                    continue;
                }

                var assemblyName = assembly.GetName().Name;
                if (!assemblyName.FastEquals(runtimeDefaultAssemblyName) && !assemblyName.FastStartsWith(homaAssembliesPrefix))
                {
                    continue;
                }
                
                yield return assembly;
            }
        }

        private static bool FastEquals(this string source, string target)
        {
            return string.Equals(source, target, StringComparison.Ordinal);
        }
        
        private static bool FastStartsWith(this string source, string target)
        {
            return source.IndexOf(target, StringComparison.Ordinal) == 0;
        }
    }
}