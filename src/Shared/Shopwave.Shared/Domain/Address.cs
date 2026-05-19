using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Shopwave.Shared.Domain;

/// <summary>
/// Value object representing a postal address.
/// Immutable after construction and validates common address fields.
/// </summary>
public sealed class Address : ValueObject 
{
    private static readonly Regex NameRegex = new(@"^[\p{L}\p{N}\s.'-]{2,100}$", RegexOptions.Compiled);
    private static readonly Regex PostalCodeRegex = new(@"^[a-zA-Z0-9\s-]{3,12}$", RegexOptions.Compiled);
    
    public string StreetAddress1 { get; init; } = null!;
    public string? StreetAddress2 { get; init; }
    public string City { get; init; } = null!;
    public string StateProvinceRegion { get; init; } = null!;
    public string Country { get; init; } = null!;
    public string? PostalZipCode { get; init; }
    
    private Address() { } 
    
    public Address(string streetAddress1, string? streetAddress2, string city, string stateProvinceRegion, string country, string? postalZipCode)
    {
        StreetAddress1 = ValidateStreetAddress1(streetAddress1);
        StreetAddress2 = ValidateOptionalStreetAddress2(streetAddress2);
        City = ValidateRegion("City", city);
        StateProvinceRegion = ValidateRegion("State/Province", stateProvinceRegion);
        Country = ValidateRegion("Country", country);
        PostalZipCode = string.IsNullOrWhiteSpace(postalZipCode) ? null : ValidatePostalZipCode(postalZipCode);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return StreetAddress1;
        yield return StreetAddress2 ?? string.Empty; 
        yield return City;
        yield return StateProvinceRegion;
        yield return Country;
        yield return PostalZipCode ?? string.Empty;
    }
    
    private static string ValidateStreetAddress1(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) 
            throw new ArgumentException("Street address is required.");
        
        if (value.Length > 200) 
            throw new ArgumentException("Street address cannot exceed 200 characters.");
        
        return value.Trim();
    }

    private static string? ValidateOptionalStreetAddress2(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) 
            return null;
        
        if (value.Length > 200) 
            throw new ArgumentException("Street address 2 cannot exceed 200 characters.");
        
        return value.Trim();
    }

    private static string ValidateRegion(string fieldName, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) 
            throw new ArgumentException($"{fieldName} is required.");
        
        if (!NameRegex.IsMatch(value.Trim())) 
            throw new ArgumentException($"{fieldName} contains invalid characters.");
        
        return value.Trim();
    }

    private static string ValidatePostalZipCode(string value)
    {
        if (!PostalCodeRegex.IsMatch(value.Trim())) 
            throw new ArgumentException("Postal/ZIP code format is invalid.");
        
        return value.Trim();
    }
}