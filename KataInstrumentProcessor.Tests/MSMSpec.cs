 
 
namespace MSMSpec
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using System.Reflection;
	using Machine.Specifications;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	
    [System.Diagnostics.DebuggerStepThrough]
    public static class TestExecutionHelper
    {
        private static IEnumerable<FieldInfo> GetFields(Type type)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            return fields;
        }

        private static IEnumerable<FieldInfo> FilterByType<TFieldType>(IEnumerable<FieldInfo> fields)
        {
            var fieldType = typeof(TFieldType);
            var filtered = fields.Where(x => x.FieldType == fieldType);
            return filtered;
        }

        public static void Process<T>(Dictionary<string, Action> results)
        {
		    if (typeof(T).IsDefined(typeof(Machine.Specifications.IgnoreAttribute), false))
            {
                return;
            }
			
            var instance = Activator.CreateInstance<T>();
            var heirarchy = GetClassHeirarchyTopDown<T>();
            var fieldInfos = heirarchy.SelectMany(x => GetFields(x));
            
            var establishInfos = FilterByType<Establish>(fieldInfos);
            var becauseInfos = FilterByType<Because>(fieldInfos);
            var its = FilterByType<It>(fieldInfos);
            var cleanupInfos = FilterByType<Cleanup>(fieldInfos);

            ExecuteDelegateFieldsOnInstance(establishInfos, instance);
            ExecuteDelegateFieldsOnInstance(becauseInfos, instance);
            
            ProcessIts(its, instance, results);

            ExecuteDelegateFieldsOnInstance(cleanupInfos, instance);
        }

        private static void ProcessIts<T>(IEnumerable<FieldInfo> its, T instance, Dictionary<string, Action> results)
        {
            foreach (var it in its)
            {
			    if (it.IsDefined(typeof(Machine.Specifications.IgnoreAttribute), false))
                {
                    continue;
                }
				
                dynamic method = it.GetValue(instance);
                ExecuteItMethod(results, it, method);
            }
        }

        private static void ExecuteItMethod(Dictionary<string, Action> results, FieldInfo it, dynamic method)
        {
            if (IsTheItImplemented(method))
            {
                ExecuteTheItAndStoreAnyExceptionInResults(it, method, results);
            }
            else
            {
                AddNotImplementedItToResults(it, results);
            }
        }

        private static void ExecuteTheItAndThrowAnyException(FieldInfo it, dynamic method, Dictionary<string, Action> results)
        {
            method();
            results.Add(it.Name, () => { });
        }

        private static void ExecuteTheItAndStoreAnyExceptionInResults(FieldInfo it, dynamic method, Dictionary<string, Action> results)
        {
            try
            {
                method();
                results.Add(it.Name, () => { });
            }
            catch (Exception e)
            {
				var _e = e;
                results.Add(it.Name, () => { throw _e; });
            }
        }

        private static void AddNotImplementedItToResults(FieldInfo it, Dictionary<string, Action> results)
        {
            var name = it.Name;
            results.Add(name, () => Assert.Inconclusive("Not implemented : {0}", name));
        }

        private static dynamic IsTheItImplemented(dynamic method)
        {
            return method != null;
        }

        private static void ExecuteDelegateFieldsOnInstance(IEnumerable<FieldInfo> delegateFieldInfos, object instance)
        {
            foreach (var delegateFieldInfo in delegateFieldInfos)
            {
                dynamic method = delegateFieldInfo.GetValue(instance);
                if(method != null) 
                    method();
            }
        }    

        private static IEnumerable<Type> GetClassHeirarchyTopDown<T>()
        {
            var baseType = typeof(T);

            List<Type> types = new List<Type>();

            while (baseType != typeof(object))
            {
                types.Add(baseType);
                baseType = baseType.BaseType;
            }

            types.Reverse();

            return types;
        }
    }
}
