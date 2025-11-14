using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RepositoryLayer.Migrations
{
    /// <inheritdoc />
    public partial class OrderSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_OrderAddresses_ShippingAddressId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ShippingAddressId",
                table: "Orders");

            migrationBuilder.AddColumn<string>(
                name: "ClientSecret",
                table: "Carts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryMethodId",
                table: "Carts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentIntentId",
                table: "Carts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "Animals",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Price",
                table: "Products",
                column: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Stock",
                table: "Products",
                column: "Stock");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShippingAddressId",
                table: "Orders",
                column: "ShippingAddressId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_OrderAddresses_ShippingAddressId",
                table: "Orders",
                column: "ShippingAddressId",
                principalTable: "OrderAddresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_OrderAddresses_ShippingAddressId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Products_Price",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Stock",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ShippingAddressId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ClientSecret",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "DeliveryMethodId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "PaymentIntentId",
                table: "Carts");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "Animals",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShippingAddressId",
                table: "Orders",
                column: "ShippingAddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_OrderAddresses_ShippingAddressId",
                table: "Orders",
                column: "ShippingAddressId",
                principalTable: "OrderAddresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
