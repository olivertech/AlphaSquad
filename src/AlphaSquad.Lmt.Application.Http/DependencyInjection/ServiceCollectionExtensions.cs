using System;
using AlphaSquad.Lmt.Application.ApiClient;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Lmt.Application.Http.Facades;
using AlphaSquad.Lmt.Application.Http.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using AlphaSquad.Lmt.Application.Http.Authentication;

namespace AlphaSquad.Lmt.Application.Http.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGeneratedApi(
        this IServiceCollection services,
        string baseUrl)
    {
        if (services is null)
            throw new ArgumentNullException(nameof(services));

        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("Base URL cannot be empty.", nameof(baseUrl));

        services.AddHttpClient("GeneratedApi", client =>
        {
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddScoped<IAuthenticationProvider, AccessTokenAuthenticationProvider>();

        services.AddScoped<IRequestAdapter>(serviceProvider =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("GeneratedApi");
            var authenticationProvider = serviceProvider.GetRequiredService<IAuthenticationProvider>();

            return new HttpClientRequestAdapter(authenticationProvider, httpClient: httpClient)
            {
                BaseUrl = baseUrl
            };
        });

        services.AddScoped<AlphaSquadLmtApplicationApiClient>(serviceProvider =>
        {
            var requestAdapter = serviceProvider.GetRequiredService<IRequestAdapter>();
            return new AlphaSquadLmtApplicationApiClient(requestAdapter);
        });

        services.AddScoped<IApiFacade, ApiFacade>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICheckinsService, CheckinsService>();
        services.AddScoped<IClassesService, ClassesService>();
        services.AddScoped<IEventsService, EventsService>();
        services.AddScoped<IExercisesService, ExercisesService>();
        services.AddScoped<IGamificationService, GamificationService>();
        services.AddScoped<ILegalService, LegalService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<IPlansService, PlansService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<ISocialService, SocialService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<ITenantsService, TenantsService>();
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<IWorkoutsService, WorkoutsService>();

        return services;
    }
}