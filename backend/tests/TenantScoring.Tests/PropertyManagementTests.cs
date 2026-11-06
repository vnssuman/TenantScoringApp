using System.Text.Json;
using TenantScoring.Models;
using Xunit;

namespace TenantScoring.Tests;

public sealed class PropertyManagementTests
{
    [Fact]
    public void ManagementConstructorBuildsLegacyLocationAndVacancyStatus()
    {
        var property = new Property(Guid.NewGuid(), "Skyline", "Apartment", "12 Park Road", "Pune", "Maharashtra", 20000, false);

        Assert.Equal("12 Park Road, Pune, Maharashtra", property.Location);
        Assert.Equal("Leased", property.Status);
        Assert.False(property.IsVacant);
    }

    [Fact]
    public void SetStatusKeepsVacancyInSyncForLeaseTransitions()
    {
        var property = new Property(Guid.NewGuid(), "Skyline", "Apartment", "12 Park Road", "Pune", "Maharashtra", 20000, true);

        property.SetStatus("Leased");
        Assert.False(property.IsVacant);

        property.SetStatus("Available");
        Assert.True(property.IsVacant);
    }

    [Fact]
    public void UpdateDetailsUpdatesPropertyManagementAndLegacyFields()
    {
        var property = new Property(Guid.NewGuid(), "Skyline", "Apartment", "12 Park Road", "Pune", "Maharashtra", 20000, true);

        property.UpdateDetails("Garden Villa", "Villa", "9 Lake View", "Nashik", "Maharashtra", 27500, false);

        Assert.Equal("Garden Villa", property.PropertyName);
        Assert.Equal("Villa", property.PropertyType);
        Assert.Equal("9 Lake View, Nashik, Maharashtra", property.Location);
        Assert.Equal(27500, property.RentAmount);
        Assert.Equal("Leased", property.Status);
        Assert.False(property.IsVacant);
    }

    [Fact]
    public void ManagementDtosUseTheFrontendJsonFieldNames()
    {
        const string requestJson = "{\"property_name\":\"Skyline\",\"property_type\":\"Apartment\",\"address\":\"12 Park Road\",\"city\":\"Pune\",\"state\":\"Maharashtra\",\"monthly_rent\":20000,\"is_vacant\":true}";
        var request = JsonSerializer.Deserialize<PropertyManagementRequest>(requestJson);
        var responseJson = JsonSerializer.Serialize(new PropertyManagementResponse(Guid.Empty, "Skyline", "Apartment", "12 Park Road", "Pune", "Maharashtra", 20000, true));

        Assert.NotNull(request);
        Assert.Equal("Skyline", request.PropertyName);
        Assert.Contains("\"id\"", responseJson);
        Assert.Contains("\"property_name\"", responseJson);
        Assert.Contains("\"monthly_rent\"", responseJson);
        Assert.Contains("\"is_vacant\"", responseJson);
    }

    [Fact]
    public void AcceptingRentalRequestPreservesRequestedRentalDates()
    {
        var startDate = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2027, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        var lease = new Lease(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 20000, 0);
        lease.SetRentalTerms(startDate, endDate, null);

        lease.Accept();

        Assert.Equal("Active", lease.Status);
        Assert.Equal(startDate, lease.StartDate);
        Assert.Equal(endDate, lease.EndDate);
    }

    [Fact]
    public void WithdrawingPendingRentalRequestSetsWithdrawnStatus()
    {
        var lease = new Lease(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 20000, 0);

        lease.Withdraw();

        Assert.Equal("Withdrawn", lease.Status);
    }
}