using System.Net.Http.Json;
using FluentAssertions;
using FluentAssertions.Primitives;
using Microsoft.AspNetCore.Mvc;

namespace Tests.Shared.Extensions;

public static class HttpResponseMessageExtensions
{
    /// <summary>
    /// Check for exact expected problem detail and title in the response
    /// </summary>
    /// <param name="assertions"></param>
    /// <param name="expectedProblem"></param>
    /// <returns></returns>
    public static AndConstraint<FluentAssertions.Web.HttpResponseMessageAssertions> HasProblemDetail(
        this HttpResponseMessageAssertions assertions,
        object expectedProblem
    )
    {
        var responseProblemDetails = assertions
            .Subject.Content.ReadFromJsonAsync<ProblemDetails>()
            .GetAwaiter()
            .GetResult();
        responseProblemDetails.Should().BeEquivalentTo(expectedProblem);
        var responseMessageAssertions = new FluentAssertions.Web.HttpResponseMessageAssertions(assertions.Subject);

        return new AndConstraint<FluentAssertions.Web.HttpResponseMessageAssertions>(responseMessageAssertions);
    }

    /// <summary>
    /// Check for containing expected problem detail and title in the response
    /// </summary>
    /// <param name="assertions"></param>
    /// <param name="detail"></param>
    /// <param name="title"></param>
    /// <returns></returns>
    public static AndConstraint<FluentAssertions.Web.HttpResponseMessageAssertions> ContainsProblemDetail(
        this HttpResponseMessageAssertions assertions,
        string detail,
        string? title = null
    )
    {
        var responseProblemDetails = assertions
            .Subject.Content.ReadFromJsonAsync<ProblemDetails>()
            .GetAwaiter()
            .GetResult();
        responseProblemDetails.Should().NotBeNull();
        responseProblemDetails!.Detail.Should().Contain(detail);

        if (!string.IsNullOrWhiteSpace(title))
            responseProblemDetails.Title.Should().Be(title);

        var responseMessageAssertions = new FluentAssertions.Web.HttpResponseMessageAssertions(assertions.Subject);

        return new AndConstraint<FluentAssertions.Web.HttpResponseMessageAssertions>(responseMessageAssertions);
    }

    /// <summary>
    /// Check for containing expected problem detail in the response
    /// </summary>
    /// <param name="assertions"></param>
    /// <param name="expectedProblemDetails"></param>
    /// <returns></returns>
    public static AndConstraint<FluentAssertions.Web.HttpResponseMessageAssertions> ContainsProblemDetail(
        this HttpResponseMessageAssertions assertions,
        ProblemDetails expectedProblemDetails
    )
    {
        var responseProblemDetails = assertions
            .Subject.Content.ReadFromJsonAsync<ProblemDetails>()
            .GetAwaiter()
            .GetResult();
        responseProblemDetails.Should().NotBeNull();

        if (!string.IsNullOrWhiteSpace(expectedProblemDetails.Title))
            responseProblemDetails!.Title.Should().Be(expectedProblemDetails.Title);

        if (!string.IsNullOrWhiteSpace(expectedProblemDetails.Detail))
            responseProblemDetails!.Detail.Should().Contain(expectedProblemDetails.Detail);

        if (!string.IsNullOrWhiteSpace(expectedProblemDetails.Type))
            responseProblemDetails!.Type.Should().Be(expectedProblemDetails.Type);

        if (expectedProblemDetails.Status is not null)
            responseProblemDetails!.Status.Should().Be(expectedProblemDetails.Status);

        var responseMessageAssertions = new FluentAssertions.Web.HttpResponseMessageAssertions(assertions.Subject);

        return new AndConstraint<FluentAssertions.Web.HttpResponseMessageAssertions>(responseMessageAssertions);
    }

    public static AndConstraint<FluentAssertions.Web.HttpResponseMessageAssertions> HasResponse<TResponse>(
        this HttpResponseMessageAssertions assertions,
        object? expectedObject = null,
        Action<TResponse?>? responseAction = null
    )
    {
        assertions.BeSuccessful();

        var responseObject = assertions.Subject.Content.ReadFromJsonAsync<TResponse>().GetAwaiter().GetResult();

        responseObject.Should().NotBeNull();

        if (expectedObject is not null)
        {
            //https://fluentassertions.com/objectgraphs/
            responseObject.Should().BeEquivalentTo(expectedObject, options => options.ExcludingMissingMembers());
        }

        responseAction?.Invoke(responseObject);

        var responseMessageAssertions = new FluentAssertions.Web.HttpResponseMessageAssertions(assertions.Subject);
        return new AndConstraint<FluentAssertions.Web.HttpResponseMessageAssertions>(responseMessageAssertions);
    }
}
