// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypedFactoryInstaller.cs" company="Helpmebot Development Team">
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
//   The typed factory facility.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Startup.Installers
{
    using Castle.Facilities.TypedFactory;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using Helpmebot.Commands.Interfaces;
    using Helpmebot.TypedFactories;

    using helpmebot6.Commands;

    /// <summary>
    /// The typed factory facility.
    /// </summary>
    [InstallerPriority(InstallerPriorityAttribute.Factory)]
    public class TypedFactoryInstaller : IWindsorInstaller
    {
        /// <summary>
        /// Performs the installation in the <see cref="T:Castle.Windsor.IWindsorContainer"/>.
        /// </summary>
        /// <param name="container">The container.</param><param name="store">The configuration store.</param>
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.AddFacility<TypedFactoryFacility>();

            container.Register(
                Component.For<ICommandTypedFactory>().AsFactory(),
                Component.For<IKeywordCommandFactory>().AsFactory(),
                Classes.FromThisAssembly()
                    .BasedOn<ICommand>()
                    .WithServiceSelf()
                    .WithServices(typeof(ICommand))
                    .LifestyleTransient(),
                Classes.FromThisAssembly()
                    .BasedOn<GenericCommand>()
                    .WithServiceSelf()
                    .WithServices(typeof(ICommand))
                    .LifestyleTransient());
        }
    }
}
