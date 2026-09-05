using ECommerceApi.Controllers;
using ECommerceApi.DTOs;
using ECommerceApi.Models;
using ECommerceApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Tests;

public class AuthControllerTests
{
    private static AuthController CreateController(TestDb db)
    {
        var manager=TestHelpers.CreateUserManager(db.Context);
        var signIn=TestHelpers.CreateSignInManager(manager);
        var jwt=new JwtService(TestHelpers.CreateConfiguration(),manager);
        return new AuthController(jwt,manager,signIn);
    }

    [Fact] public async Task Register_ValidUser_ReturnsCreated() { // Arrange
        using var db=new TestDb(); var controller=CreateController(db); var dto=new RegisterDto{Email="user@example.com",Username="user",Password="Password123!"};
        // Act
        var result=await controller.Register(dto);
        // Assert
        Assert.IsType<ObjectResult>(result); Assert.Single(await db.Context.Users.ToListAsync()); }

    [Fact] public async Task Register_NormalizesEmailToLowercase() { // Arrange
        using var db=new TestDb(); var controller=CreateController(db); var dto=new RegisterDto{Email=" USER@EXAMPLE.COM ",Username="user",Password="Password123!"};
        // Act
        await controller.Register(dto);
        // Assert
        Assert.Equal("user@example.com",(await db.Context.Users.SingleAsync()).Email); }

    [Fact] public async Task Register_InvalidModelState_ReturnsBadRequest() { // Arrange
        using var db=new TestDb(); var controller=CreateController(db); controller.ModelState.AddModelError("Email","Invalid"); var dto=new RegisterDto{Email="bad",Username="user",Password="Password123!"};
        // Act
        var result=await controller.Register(dto);
        // Assert
        Assert.IsType<BadRequestObjectResult>(result); Assert.Empty(await db.Context.Users.ToListAsync()); }

    [Fact] public async Task Login_UnknownUser_ReturnsUnauthorized() { // Arrange
        using var db=new TestDb(); var controller=CreateController(db); var dto=new LoginDto{Username="missing",Password="Password123!"};
        // Act
        var result=await controller.Login(dto);
        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result); }
    
    [Fact] public async Task Login_ValidCredentials_ReturnsToken() { // Arrange
        using var db=new TestDb(); var controller=CreateController(db); await controller.Register(new RegisterDto{Email="user@example.com",Username="user",Password="Password123!"});
        // Act
        var result=await controller.Login(new LoginDto{Username="user",Password="Password123!"});
        // Assert
        var ok=Assert.IsType<OkObjectResult>(result); var token=ok.Value!.GetType().GetProperty("token")!.GetValue(ok.Value) as string; Assert.False(string.IsNullOrWhiteSpace(token)); }
}
