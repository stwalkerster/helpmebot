// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LegacyCommandParserTests.cs" company="Helpmebot Development Team">
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
//   The legacy command parser tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Tests.Legacy
{
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;

    using Helpmebot.Legacy;
    using Helpmebot.Services;
    using Helpmebot.Services.Interfaces;
    using Helpmebot.Startup;
    using Helpmebot.Tests.TestData;
    using Helpmebot.TypedFactories;

    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// The legacy command parser tests.
    /// </summary>
    [TestFixture]
    public class LegacyCommandParserTests : TestBase
    {
        #region Public Methods and Operators

        /// <summary>
        /// The test find redirection.
        /// </summary>
        /// <param name="inputdata">
        /// The input data.
        /// </param>
        /// <param name="expecteddata">
        /// The expected data.
        /// </param>
        /// <param name="expectedRedir">
        /// The expected redirection.
        /// </param>
        [TestCaseSource(typeof(CommandParserRedirectionDataSource))]
        public void TestFindRedirection(string inputdata, string expecteddata, string expectedRedir)
        {
            // arrange
            string[] input = inputdata.Split();

            // act
            string redir = LegacyCommandParser.FindRedirection(ref input);

            // assert
            Assert.That(redir, Is.EqualTo(expectedRedir));
        }

        /// <summary>
        /// The test redirection result.
        /// </summary>
        /// <param name="inputdata">
        /// The input data.
        /// </param>
        /// <param name="expecteddata">
        /// The expected data.
        /// </param>
        /// <param name="expectedRedir">
        /// The expected redirection.
        /// </param>
        [TestCaseSource(typeof(CommandParserRedirectionDataSource))]
        public void TestPostRedirectionMessage(string inputdata, string expecteddata, string expectedRedir)
        {
            // arrange
            string[] input = inputdata.Split();

            string[] expected = expecteddata.Split();
            if (expecteddata == string.Empty)
            {
                expected = new string[0];
            }

            // act
            LegacyCommandParser.FindRedirection(ref input);

            // assert
            Assert.That(input, Is.EqualTo(expected));
        }

        /// <summary>
        /// The local setup.
        /// </summary>
        public override void LocalSetup()
        {
            var windsorContainer = new WindsorContainer();
            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(windsorContainer));

            ICommandTypedFactory commandFactory = new Mock<ICommandTypedFactory>().Object;
            IKeywordService keywordService = new Mock<IKeywordService>().Object;
            IKeywordCommandFactory keywordFactory = new Mock<IKeywordCommandFactory>().Object;

            var commandParser = new CommandParser(
                "!",
                commandFactory,
                keywordService,
                keywordFactory,
                this.Logger.Object);
            windsorContainer.Register(Component.For<ICommandParser>().Instance(commandParser));
        }

        #endregion
    }
}