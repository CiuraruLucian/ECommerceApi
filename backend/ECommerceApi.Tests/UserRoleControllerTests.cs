using ECommerceApi.Controllers;
using ECommerceApi.DTOs;
using ECommerceApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Tests;

public class UserRoleControllerTests
{
    [Fact] public async Task GetAll_ReturnsAllUsers() { // Arrange
        using var db=new TestDb(); var manager=TestHelpers.CreateUserManager(db.Context); await manager.CreateAsync(new User{UserName="one",Email="one@example.com"},"Password123!"); await manager.CreateAsync(new User{UserName="two",Email="two@example.com"},"Password123!"); var controller=new UserController(manager);
        // Act
        var result=await controller.GetAll();
        // Assert
        var ok=Assert.IsType<OkObjectResult>(result); Assert.Equal(2,((IEnumerable<object>)ok.Value!).Count()); }

    [Fact] public async Task GetById_MissingUser_ReturnsNotFound() { // Arrange
        using var db=new TestDb(); var manager=TestHelpers.CreateUserManager(db.Context); var controller=new UserController(manager);
        // Act
        var result=await controller.GetById("missing");
        // Assert
        Assert.IsType<NotFoundResult>(result); }

    [Fact] public async Task Put_UpdatesEmailAndUsername() { // Arrange
        using var db=new TestDb(); var manager=TestHelpers.CreateUserManager(db.Context); var user=new User{UserName="old",Email="old@example.com"}; await manager.CreateAsync(user,"Password123!"); var controller=new UserController(manager);
        // Act
        var result=await controller.Put(user.Id,new UpdateUserDto{Email="new@example.com",UserName="new"});
        // Assert
        Assert.IsType<OkObjectResult>(result); var updated=await manager.FindByIdAsync(user.Id); Assert.Equal("new@example.com",updated!.Email); Assert.Equal("new",updated.UserName); }

    [Fact] public async Task Delete_MissingUser_ReturnsNotFound() { // Arrange
        using var db=new TestDb(); var manager=TestHelpers.CreateUserManager(db.Context); var controller=new UserController(manager);
        // Act
        var result=await controller.Delete("missing");
        // Assert
        Assert.IsType<NotFoundResult>(result); }

    [Fact] public async Task AssignRole_ExistingUserAndRole_ReturnsOk() { // Arrange
        using var db=new TestDb(); var manager=TestHelpers.CreateUserManager(db.Context); var user=new User{UserName="user",Email="user@example.com"}; await manager.CreateAsync(user,"Password123!"); db.Context.Roles.Add(new IdentityRole
        {
            Name = "Admin",
            NormalizedName = "ADMIN"
        });
        await db.Context.SaveChangesAsync(); var controller=new RoleController(manager);
        // Act
        var result=await controller.AssignRole(new AssignRoleDto{UserId=user.Id,RoleName="Admin"});
        // Assert
        Assert.IsType<OkObjectResult>(result);

    }
}
