using ECommerceApi.DTOs;
using System.ComponentModel.DataAnnotations;

namespace ECommerceApi.Tests;

public class DtoValidationTests
{
    private static IList<ValidationResult> Validate(object model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, true);
        return results;
    }

    [Fact] public void RegisterDto_ValidData_IsValid() { // Arrange
        var dto = new RegisterDto { Email="user@example.com", Username="user", Password="password123" };
        // Act
        var results=Validate(dto);
        // Assert
        Assert.Empty(results); }

    [Fact] public void RegisterDto_EmptyEmail_IsInvalid() { // Arrange
        var dto=new RegisterDto { Email="", Username="user", Password="password123" };
        // Act
        var results=Validate(dto);
        // Assert
        Assert.Contains(results,r=>r.MemberNames.Contains(nameof(RegisterDto.Email))); }

    [Fact] public void RegisterDto_InvalidEmail_IsInvalid() { // Arrange
        var dto=new RegisterDto { Email="not-an-email", Username="user", Password="password123" };
        // Act
        var results=Validate(dto);
        // Assert
        Assert.Contains(results,r=>r.MemberNames.Contains(nameof(RegisterDto.Email))); }

    [Fact] public void RegisterDto_UsernameLongerThan50_IsInvalid() { // Arrange
        var dto=new RegisterDto { Email="user@example.com", Username=new string('a',51), Password="password123" };
        // Act
        var results=Validate(dto);
        // Assert
        Assert.Contains(results,r=>r.MemberNames.Contains(nameof(RegisterDto.Username))); }

    [Fact] public void RegisterDto_PasswordShorterThan8_IsInvalid() { // Arrange
        var dto=new RegisterDto { Email="user@example.com", Username="user", Password="1234567" };
        // Act
        var results=Validate(dto);
        // Assert
        Assert.Contains(results,r=>r.MemberNames.Contains(nameof(RegisterDto.Password))); }

    [Fact] public void RegisterDto_PasswordExactly8_IsValid() { // Arrange
        var dto=new RegisterDto { Email="user@example.com", Username="user", Password="12345678" };
        // Act
        var results=Validate(dto);
        // Assert
        Assert.DoesNotContain(results,r=>r.MemberNames.Contains(nameof(RegisterDto.Password))); }

    [Fact] public void RegisterDto_UsernameExactly50_IsValid() { // Arrange
        var dto=new RegisterDto { Email="user@example.com", Username=new string('a',50), Password="password123" };
        // Act
        var results=Validate(dto);
        // Assert
        Assert.DoesNotContain(results,r=>r.MemberNames.Contains(nameof(RegisterDto.Username))); }

    [Fact] public void LoginDto_ValidData_IsValid() { // Arrange
        var dto=new LoginDto { Username="user", Password="password123" };
        // Act
        var results=Validate(dto);
        // Assert
        Assert.Empty(results); }

    [Fact] public void LoginDto_EmptyUsername_IsInvalid() { // Arrange
        var dto=new LoginDto { Username="", Password="password123" };
        // Act
        var results=Validate(dto);
        // Assert
        Assert.Contains(results,r=>r.MemberNames.Contains(nameof(LoginDto.Username))); }

    [Fact] public void LoginDto_EmptyPassword_IsInvalid() { // Arrange
        var dto=new LoginDto { Username="user", Password="" };
        // Act
        var results=Validate(dto);
        // Assert
        Assert.Contains(results,r=>r.MemberNames.Contains(nameof(LoginDto.Password))); }

    [Fact] public void AddCartItemDto_ValidQuantity_IsValid() { // Arrange
        var dto=new AddCartItemDto { ProductId=1, Quantity=2 };
        // Act
        var results=Validate(dto);
        // Assert
        Assert.Empty(results); }

    [Fact] public void AddCartItemDto_ZeroQuantity_IsInvalid() { // Arrange
        var dto=new AddCartItemDto { ProductId=1, Quantity=0 };
        // Act
        var results=Validate(dto);
        // Assert
        Assert.Contains(results,r=>r.MemberNames.Contains(nameof(AddCartItemDto.Quantity))); }

    [Fact] public void AddCartItemDto_NegativeQuantity_IsInvalid() { // Arrange
        var dto=new AddCartItemDto { ProductId=1, Quantity=-1 };
        // Act
        var results=Validate(dto);
        // Assert
        Assert.Contains(results,r=>r.MemberNames.Contains(nameof(AddCartItemDto.Quantity))); }

    [Fact] public void AddCartItemDto_MaxQuantity_IsValid() { // Arrange
        var dto=new AddCartItemDto { ProductId=1, Quantity=int.MaxValue };
        // Act
        var results=Validate(dto);
        // Assert
        Assert.Empty(results); }

    [Fact] public void ProductDto_AllValuesProvided_IsValid() { // Arrange
        var dto=new ProductDto { Name="Phone", Description="Good phone", Price=100m, Stock=5 };
        // Act
        var results=Validate(dto);
        // Assert
        Assert.Empty(results); }

    [Fact] public void ProductDto_EmptyName_IsInvalid() { // Arrange
        var dto=new ProductDto { Name="", Description="Good", Price=1m, Stock=1 };
        // Act
        var results=Validate(dto);
        // Assert
        Assert.Contains(results,r=>r.MemberNames.Contains(nameof(ProductDto.Name))); }

    [Fact] public void ProductDto_EmptyDescription_IsInvalid() { // Arrange
        var dto=new ProductDto { Name="Phone", Description="", Price=1m, Stock=1 };
        // Act
        var results=Validate(dto);
        // Assert
        Assert.Contains(results,r=>r.MemberNames.Contains(nameof(ProductDto.Description))); }

    [Fact] public void ProductDto_DefaultPrice_DoesNotViolateRequiredBecauseItIsValueType() { // Arrange
        var dto=new ProductDto { Name="Phone", Description="Good" };
        // Act
        var results=Validate(dto);
        // Assert
        Assert.DoesNotContain(results,r=>r.MemberNames.Contains(nameof(ProductDto.Price))); }

    [Fact] public void ProductDto_DefaultStock_DoesNotViolateRequiredBecauseItIsValueType() { // Arrange
        var dto=new ProductDto { Name="Phone", Description="Good" };
        // Act
        var results=Validate(dto);
        // Assert
        Assert.DoesNotContain(results,r=>r.MemberNames.Contains(nameof(ProductDto.Stock))); }

    [Fact] public void UpdateUserDto_AllowsNullValues() { // Arrange
        var dto=new UpdateUserDto();
        // Act
        var results=Validate(dto);
        // Assert
        Assert.Empty(results); }

    [Fact] public void AssignRoleDto_DefaultRoleName_IsEmptyString() { // Arrange
        var dto=new AssignRoleDto();
        // Act
        var role=dto.RoleName;
        // Assert
        Assert.Equal(string.Empty,role); }
}
