// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlagGroupParseFlagsDataSource.cs" company="Helpmebot Development Team">
//   Helpmebot is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   Helpmebot is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with Helpmebot.  If not, see http://www.gnu.org/licenses/ .
// </copyright>
// <summary>
//   The flag group parse flags data source.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Tests.TestData
{
    using System.Collections;
    using System.Collections.Generic;

    using Helpmebot.Model;

    using NUnit.Framework;

    /// <summary>
    /// The flag group parse flags data source.
    /// </summary>
    internal class FlagGroupParseFlagsDataSource : IEnumerable<TestCaseData>
    {
        #region Fields

        /// <summary>
        /// The data.
        /// </summary>
        private readonly List<TestCaseData> data;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="FlagGroupParseFlagsDataSource"/> class.
        /// </summary>
        public FlagGroupParseFlagsDataSource()
        {
            this.data = new List<TestCaseData>();

            this.data.Add(
                new TestCaseData(
                    new FlagGroup { Flags = new List<FlagGroupAssoc>() },
                    "B",
                    new FlagGroup { Flags = new List<FlagGroupAssoc> { new FlagGroupAssoc { Flag = "B" } } }));

            this.data.Add(
                new TestCaseData(
                    new FlagGroup { Flags = new List<FlagGroupAssoc> { new FlagGroupAssoc { Flag = "B" } } },
                    "C",
                    new FlagGroup { Flags = new List<FlagGroupAssoc> { new FlagGroupAssoc { Flag = "C" } } }));

            this.data.Add(
                new TestCaseData(
                    new FlagGroup { Flags = new List<FlagGroupAssoc> { new FlagGroupAssoc { Flag = "B" } } },
                    "+C",
                    new FlagGroup
                        {
                            Flags =
                                new List<FlagGroupAssoc>
                                    {
                                        new FlagGroupAssoc { Flag = "B" },
                                        new FlagGroupAssoc { Flag = "C" }
                                    }
                        }));

            this.data.Add(
                new TestCaseData(
                    new FlagGroup
                        {
                            Flags =
                                new List<FlagGroupAssoc>
                                    {
                                        new FlagGroupAssoc { Flag = "B" },
                                        new FlagGroupAssoc { Flag = "C" }
                                    }
                        },
                    "-C",
                    new FlagGroup { Flags = new List<FlagGroupAssoc> { new FlagGroupAssoc { Flag = "B" } } }));

            this.data.Add(
                new TestCaseData(
                    new FlagGroup
                    {
                        Flags =
                            new List<FlagGroupAssoc>
                                    {
                                        new FlagGroupAssoc { Flag = "B" },
                                        new FlagGroupAssoc { Flag = "C" },
                                        new FlagGroupAssoc { Flag = "D" }
                                    }
                    },
                    "-C+EF",
                    new FlagGroup
                    {
                        Flags =
                            new List<FlagGroupAssoc>
                                    {
                                        new FlagGroupAssoc { Flag = "B" },
                                        new FlagGroupAssoc { Flag = "D" },
                                        new FlagGroupAssoc { Flag = "E" },
                                        new FlagGroupAssoc { Flag = "F" }
                                    }
                    }));

            this.data.Add(
                new TestCaseData(
                    new FlagGroup
                    {
                        Flags =
                            new List<FlagGroupAssoc>
                                    {
                                        new FlagGroupAssoc { Flag = "B" },
                                        new FlagGroupAssoc { Flag = "C" },
                                        new FlagGroupAssoc { Flag = "D" }
                                    }
                    },
                    "Z-C+EF",
                    new FlagGroup
                    {
                        Flags =
                            new List<FlagGroupAssoc>
                                    {
                                        new FlagGroupAssoc { Flag = "Z" },
                                        new FlagGroupAssoc { Flag = "E" },
                                        new FlagGroupAssoc { Flag = "F" }
                                    }
                    }));

            this.data.Add(
                new TestCaseData(
                    new FlagGroup(),
                    "+A",
                    new FlagGroup { Flags = new List<FlagGroupAssoc> { new FlagGroupAssoc { Flag = "A" } } }));

            this.data.Add(
                new TestCaseData(
                    new FlagGroup(),
                    "+AAAAAAAA",
                    new FlagGroup { Flags = new List<FlagGroupAssoc> { new FlagGroupAssoc { Flag = "A" } } }));

            this.data.Add(
                new TestCaseData(
                    new FlagGroup(),
                    "+A-A+A-A+A-A+A",
                    new FlagGroup { Flags = new List<FlagGroupAssoc> { new FlagGroupAssoc { Flag = "A" } } }));

            this.data.Add(
                new TestCaseData(
                    new FlagGroup { Flags = new List<FlagGroupAssoc> { new FlagGroupAssoc { Flag = "B" } } },
                    "+A-A+A-A+A-A",
                    new FlagGroup { Flags = new List<FlagGroupAssoc> { new FlagGroupAssoc { Flag = "B" } } }));

            this.data.Add(
                new TestCaseData(
                    new FlagGroup(),
                    "-A",
                    new FlagGroup { Flags = new List<FlagGroupAssoc>() }));
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<TestCaseData> GetEnumerator()
        {
            return this.data.GetEnumerator();
        }

        #endregion

        #region Explicit Interface Methods

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

        #endregion
    }
}