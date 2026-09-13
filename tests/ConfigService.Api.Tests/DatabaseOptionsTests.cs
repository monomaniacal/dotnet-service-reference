using System.ComponentModel.DataAnnotations;
using ConfigService.Api.Options;

namespace ConfigService.Api.Tests;

public sealed class DatabaseOptionsTests
{
    [Fact]
    public void DefaultConnectionString_IsEmpty()
    {
        var options = new DatabaseOptions();

        Assert.Equal(string.Empty, options.ConnectionString);
    }

    [Fact]
    public void Validate_WithEmptyConnectionString_FailsRequired()
    {
        var options = new DatabaseOptions();
        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(options, context, results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(DatabaseOptions.ConnectionString)));
    }

    [Fact]
    public void Validate_WithConnectionString_Succeeds()
    {
        var options = new DatabaseOptions { ConnectionString = "Host=localhost;Database=x;Username=x;Password=x" };
        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(options, context, results, validateAllProperties: true);

        Assert.True(isValid);
        Assert.Empty(results);
    }
}
