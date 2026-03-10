namespace EventManagerService.Presentation
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen();

            return services;

        }
    }
}
