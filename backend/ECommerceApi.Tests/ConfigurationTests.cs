using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ECommerceApi.Tests;

public class ConfigurationTests
{
    [Fact] public void Product_Name_IsRequired() { // Arrange
        using var db=new TestDb(); var property=db.Context.Model.FindEntityType(typeof(Product))!.FindProperty(nameof(Product.Name));
        // Act
        var nullable=property!.IsNullable;
        // Assert
        Assert.False(nullable); }
    [Fact] public void Product_Name_MaxLengthIs100() { // Arrange
        using var db=new TestDb(); var property=db.Context.Model.FindEntityType(typeof(Product))!.FindProperty(nameof(Product.Name));
        // Act
        var length=property!.GetMaxLength();
        // Assert
        Assert.Equal(100,length); }
    [Fact] public void Product_NormalizedName_IsRequired() { // Arrange
        using var db=new TestDb(); var property=db.Context.Model.FindEntityType(typeof(Product))!.FindProperty(nameof(Product.NormalizedName));
        // Act
        var nullable=property!.IsNullable;
        // Assert
        Assert.False(nullable); }
    [Fact] public void Product_NormalizedName_MaxLengthIs100() { // Arrange
        using var db=new TestDb(); var property=db.Context.Model.FindEntityType(typeof(Product))!.FindProperty(nameof(Product.NormalizedName));
        // Act
        var length=property!.GetMaxLength();
        // Assert
        Assert.Equal(100,length); }
    [Fact] public void Product_NormalizedName_HasUniqueIndex() { // Arrange
        using var db=new TestDb(); var index=db.Context.Model.FindEntityType(typeof(Product))!.GetIndexes().Single(i=>i.Properties.Any(p=>p.Name==nameof(Product.NormalizedName)));
        // Act
        var unique=index.IsUnique;
        // Assert
        Assert.True(unique); }
    [Fact] public void Product_Description_MaxLengthIs500() { // Arrange
        using var db=new TestDb(); var property=db.Context.Model.FindEntityType(typeof(Product))!.FindProperty(nameof(Product.Description));
        // Act
        var length=property!.GetMaxLength();
        // Assert
        Assert.Equal(500,length); }
    [Fact] public void Product_Price_HasDecimalColumnType() { // Arrange
        using var db=new TestDb(); var property=db.Context.Model.FindEntityType(typeof(Product))!.FindProperty(nameof(Product.Price));
        // Act
        var type=property!.GetColumnType();
        // Assert
        Assert.Equal("decimal(18,2)",type); }
    [Fact] public void CartItem_CartForeignKey_CascadesDelete() { // Arrange
        using var db=new TestDb(); var fk=db.Context.Model.FindEntityType(typeof(CartItem))!.GetForeignKeys().Single(f=>f.PrincipalEntityType.ClrType==typeof(Cart));
        // Act
        var behavior=fk.DeleteBehavior;
        // Assert
        Assert.Equal(DeleteBehavior.Cascade,behavior); }
    [Fact] public void CartItem_ProductForeignKey_RestrictsDelete() { // Arrange
        using var db=new TestDb(); var fk=db.Context.Model.FindEntityType(typeof(CartItem))!.GetForeignKeys().Single(f=>f.PrincipalEntityType.ClrType==typeof(Product));
        // Act
        var behavior=fk.DeleteBehavior;
        // Assert
        Assert.Equal(DeleteBehavior.Restrict,behavior); }
    [Fact] public void OrderItem_OrderForeignKey_CascadesDelete() { // Arrange
        using var db=new TestDb(); var fk=db.Context.Model.FindEntityType(typeof(OrderItem))!.GetForeignKeys().Single(f=>f.PrincipalEntityType.ClrType==typeof(Order));
        // Act
        var behavior=fk.DeleteBehavior;
        // Assert
        Assert.Equal(DeleteBehavior.Cascade,behavior); }
}
