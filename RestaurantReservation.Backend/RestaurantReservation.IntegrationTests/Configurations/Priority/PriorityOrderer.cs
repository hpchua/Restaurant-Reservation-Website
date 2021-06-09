using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace RestaurantReservation.IntegrationTests.Configurations.Priority
{
    public class PriorityOrderer : ITestCaseOrderer
    {
        public const string Name = "Xunit.Priority.PriorityOrderer";
        public const string Assembly = "Xunit.Priority";

        private static string _priorityAttributeName = typeof(PriorityAttribute).AssemblyQualifiedName;
        private static string _defaultPriorityAttributeName = typeof(DefaultPriorityAttribute).AssemblyQualifiedName;
        private static string _priorityArgumentName = nameof(PriorityAttribute.Priority);

        private static ConcurrentDictionary<string, int> _defaultPriorities = new ConcurrentDictionary<string, int>();

        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
        {
            var groupedTestCases = new Dictionary<int, List<ITestCase>>();
            var defaultPriorities = new Dictionary<Type, int>();

            foreach (var testCase in testCases)
            {
                var defaultPriority = DefaultPriorityForClass(testCase);
                var priority = PriorityForTest(testCase, defaultPriority);

                if (!groupedTestCases.ContainsKey(priority))
                    groupedTestCases[priority] = new List<ITestCase>();

                groupedTestCases[priority].Add(testCase);
            }

            var orderedKeys = groupedTestCases.Keys.OrderBy(k => k);
            foreach (var list in orderedKeys.Select(priority => groupedTestCases[priority]))
            {
                list.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.TestMethod.Method.Name, y.TestMethod.Method.Name));
                foreach (TTestCase testCase in list)
                    yield return testCase;
            }
        }

        private int PriorityForTest(ITestCase testCase, int defaultPriority)
        {
            var priorityAttribute = testCase.TestMethod.Method.GetCustomAttributes(_priorityAttributeName).SingleOrDefault();
            return priorityAttribute?.GetNamedArgument<int>(_priorityArgumentName) ?? defaultPriority;
        }

        private int DefaultPriorityForClass(ITestCase testCase)
        {
            var testClass = testCase.TestMethod.TestClass.Class;
            if (!_defaultPriorities.TryGetValue(testClass.Name, out var result))
            {
                var defaultAttribute = testClass.GetCustomAttributes(_defaultPriorityAttributeName).SingleOrDefault();
                result = defaultAttribute?.GetNamedArgument<int>(_priorityArgumentName) ?? int.MaxValue;
                _defaultPriorities[testClass.Name] = result;
            }

            return result;
        }
    }
}
