using Autofac;
using Autofac.Integration.Mvc;
using System.Web.Mvc;
using WebApp_Storage_DotNet.Interfaces;
using WebApp_Storage_DotNet.Services;

namespace WebApp_Storage_DotNet
{
    public class IoCConfig
    {

        /// <summary>
        /// For more info see 
        /// :https://code.google.com/p/autofac/wiki/MvcIntegration (mvc4 instructions)
        /// </summary>
        public static void RegisterDependencies()
        {
            var builder = new ContainerBuilder();

            // placed here before RegisterControllers as last one wins
            //builder.RegisterAssemblyTypes()
            //       .Where(t => t.Name.EndsWith("Repository"))
            //       .AsImplementedInterfaces()
            //       .InstancePerRequest();
            //builder.RegisterAssemblyTypes()
            //       .Where(t => t.Name.EndsWith("Service"))
            //       .AsImplementedInterfaces()
            //       .InstancePerRequest();
            builder.RegisterType<AzureStorageFileService>().As<IFileService>();

            // Note that ASP.NET MVC requests controllers by their concrete types, 
            // so registering them As<IController>() is incorrect. 
            // Also, if you register controllers manually and choose to specify 
            // lifetimes, you must register them as InstancePerDependency() or 
            // InstancePerRequest() - ASP.NET MVC will throw an exception if 
            // you try to reuse a controller instance for multiple requests. 
            builder.RegisterControllers(typeof(MvcApplication).Assembly)
                   .InstancePerRequest();

            builder.RegisterAssemblyModules(typeof(MvcApplication).Assembly);

            //builder.RegisterModelBinders(Assembly.GetExecutingAssembly());
            //builder.RegisterModelBinderProvider();

            /*
             The MVC Integration includes an Autofac module that will add HTTP request 
             lifetime scoped registrations for the HTTP abstraction classes. The 
             following abstract classes are included: 
            -- HttpContextBase 
            -- HttpRequestBase 
            -- HttpResponseBase 
            -- HttpServerUtilityBase 
            -- HttpSessionStateBase 
            -- HttpApplicationStateBase 
            -- HttpBrowserCapabilitiesBase 
            -- HttpCachePolicyBase 
            -- VirtualPathProvider 

            To use these abstractions add the AutofacWebTypesModule to the container 
            using the standard RegisterModule method. 
            */
            builder.RegisterModule<AutofacWebTypesModule>();


            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

    }
}
