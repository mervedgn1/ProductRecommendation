using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductRecommendationHangfire
{
    public class FakeContext : IDisposable
    {
        #region Fields

        /// <summary>
        /// Defines the globalServiceScope.
        /// </summary>
        private static volatile IServiceProvider globalServiceScope;

        /// <summary>
        /// Defines the serviceProvider.
        /// </summary>
        private IServiceProvider serviceProvider;

        /// <summary>
        /// Defines the serviceScope.
        /// </summary>
        private IServiceScope serviceScope;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeContext"/> class.
        /// </summary>
        public FakeContext()
        {
            serviceScope = globalServiceScope.CreateScope();
            serviceProvider = serviceScope.ServiceProvider;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ServiceProvider.
        /// </summary>
        public IServiceProvider ServiceProvider => serviceProvider;

        #endregion

        /// <summary>
        /// The SetServiceProvider.
        /// </summary>
        /// <param name="serviceScope">The serviceScope<see cref="IServiceProvider"/>.</param>
        public static void SetServiceProvider(IServiceProvider serviceScope)
            => globalServiceScope = serviceScope;

        /// <summary>
        /// The Dispose.
        /// </summary>
        public void Dispose()
            => serviceScope.Dispose();
    }
}
