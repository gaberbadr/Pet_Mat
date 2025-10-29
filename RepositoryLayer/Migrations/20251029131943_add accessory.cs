using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RepositoryLayer.Migrations
{
    /// <inheritdoc />
    public partial class addaccessory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AccessoryListings_Latitude_Longitude",
                table: "AccessoryListings");

            migrationBuilder.CreateIndex(
                name: "IX_AccessoryListings_CreatedAt",
                table: "AccessoryListings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AccessoryListings_IsActive",
                table: "AccessoryListings",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AccessoryListings_CreatedAt",
                table: "AccessoryListings");

            migrationBuilder.DropIndex(
                name: "IX_AccessoryListings_IsActive",
                table: "AccessoryListings");

            migrationBuilder.CreateIndex(
                name: "IX_AccessoryListings_Latitude_Longitude",
                table: "AccessoryListings",
                columns: new[] { "Latitude", "Longitude" });
        }
    }
}
