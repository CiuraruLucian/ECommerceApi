using ECommerceApi.Models;

namespace ECommerceApi.Tests;

public class ModelTests
{
    [Fact] public void Product_DefaultName_IsEmpty() { // Arrange
        var product=new Product();
        // Act
        var value=product.Name;
        // Assert
        Assert.Equal(string.Empty,value); }
    [Fact] public void Product_DefaultNormalizedName_IsEmpty() { // Arrange
        var product=new Product();
        // Act
        var value=product.NormalizedName;
        // Assert
        Assert.Equal(string.Empty,value); }
    [Fact] public void Product_DefaultDescription_IsEmptyString() { // Arrange
        var product=new Product();
        // Act
        var value=product.Description;
        // Assert
        Assert.Equal(string.Empty,value); }
    [Fact] public void Product_CanStorePrice() { // Arrange
        var product=new Product();
        // Act
        product.Price=19.99m;
        // Assert
        Assert.Equal(19.99m,product.Price); }
    [Fact] public void Product_CanStoreStock() { // Arrange
        var product=new Product();
        // Act
        product.Stock=42;
        // Assert
        Assert.Equal(42,product.Stock); }
    [Fact] public void Cart_DefaultUserId_IsEmpty() { // Arrange
        var cart=new Cart();
        // Act
        var value=cart.UserId;
        // Assert
        Assert.Equal(string.Empty,value); }
    [Fact] public void Cart_DefaultItems_IsEmptyCollection() { // Arrange
        var cart=new Cart();
        // Act
        var count=cart.Items.Count;
        // Assert
        Assert.Equal(0,count); }
    [Fact] public void CartItem_DefaultQuantity_IsZero() { // Arrange
        var item=new CartItem();
        // Act
        var value=item.Quantity;
        // Assert
        Assert.Equal(0,value); }
    [Fact] public void CartItem_CanStoreProductId() { // Arrange
        var item=new CartItem();
        // Act
        item.ProductId=7;
        // Assert
        Assert.Equal(7,item.ProductId); }
    [Fact] public void Order_DefaultItems_IsEmptyCollection() { // Arrange
        var order=new Order();
        // Act
        var count=order.Items.Count;
        // Assert
        Assert.Equal(0,count); }
    [Fact] public void Order_DefaultUserId_IsEmpty() { // Arrange
        var order=new Order();
        // Act
        var value=order.UserId;
        // Assert
        Assert.Equal(string.Empty,value); }
    [Fact] public void Order_CanStorePaymentIntentId() { // Arrange
        var order=new Order();
        // Act
        order.PaymentIntentId="pi_123";
        // Assert
        Assert.Equal("pi_123",order.PaymentIntentId); }
    [Fact] public void OrderItem_DefaultProductName_IsEmpty() { // Arrange
        var item=new OrderItem();
        // Act
        var value=item.ProductName;
        // Assert
        Assert.Equal(string.Empty,value); }
    [Fact] public void OrderItem_CanStoreUnitPrice() { // Arrange
        var item=new OrderItem();
        // Act
        item.UnitPrice=12.50m;
        // Assert
        Assert.Equal(12.50m,item.UnitPrice); }
    [Fact] public void User_DefaultOrders_IsEmptyCollection() { // Arrange
        var user=new User();
        // Act
        var count=user.Orders.Count;
        // Assert
        Assert.Equal(0,count); }
}
