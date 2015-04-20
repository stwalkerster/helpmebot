namespace Helpmebot.Tests.TestData
{
    using System.Collections;
    using System.Collections.Generic;

    using NUnit.Framework;

    internal class TestDataSourceBase : IEnumerable<TestCaseData>
    {
        /// <summary>
        /// The test case data.
        /// </summary>
        protected List<TestCaseData> testCaseData;

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<TestCaseData> GetEnumerator()
        {
            return this.testCaseData.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}