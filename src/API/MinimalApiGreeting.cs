﻿using Asp.Versioning.Builder;
using Asp.Versioning.Conventions;
using FeatureManagementFilters.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;


namespace FeatureManagementFilters.API.V2
{
	public static class FeatureMinimalsApi
	{
		public static RouteGroupBuilder MapGreetingApiV2(this IEndpointRouteBuilder app)
		{
			// Create a version set
			var apiVersionSet = app.NewApiVersionSet()
								   .HasApiVersion(2.0)  
								   .ReportApiVersions()
								   .Build();

			var api = app.MapGroup("api/v{version:apiVersion}")
		     .WithApiVersionSet(apiVersionSet)
			 .MapToApiVersion(2.0);
			api.MapPost("/minimal-custom-greeting", GetCustomGreeting);


			return api;
		}
		public static async Task<Results<Ok<string>, BadRequest<ValidationProblemDetails>, NotFound<string>>> GetCustomGreeting
			([AsParameters] Greeting greeting, GreetingValidator validator, IFeatureManager featureManager)
		{
			// Use the validator to validate the incoming model
			var validationResult = await validator.ValidatWithResultAsync(greeting);

			if (!validationResult.IsValid)
			{
				validator.Logger.LogWarning("validation error on {GreetingType}: {Errors}",
					nameof(Greeting), validationResult.ProblemDetails!.Errors);

				// Return problem details if validation fails
				return TypedResults.BadRequest(validationResult.ProblemDetails);

			}

			if (await featureManager.IsEnabledAsync("CustomGreeting"))
			{
				return TypedResults.Ok($"Hello VIP user {greeting.Fullname}, this is your custom greeting V2!");
			}

			return TypedResults.Ok("Hello Anonymous user!");
		}
	}
}




