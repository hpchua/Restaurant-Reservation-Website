using System;

namespace RestaurantReservation.IntegrationTests.Configurations.Priority
{
    /// <summary>
    /// <para>Indicates relative priority of tests for execution. Tests with the same
    /// priority are run in alphabetical order. </para>
    /// 
    /// <para>Tests with no priority attribute
    /// are assigned a priority of <see cref="int.MaxValue"/>,
    /// unless the class has a <see cref="DefaultPriorityAttribute"/>.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class PriorityAttribute : Attribute
    {
        public PriorityAttribute(int priority)
        {
            Priority = priority;
        }

        public int Priority { get; private set; }
    }
}
