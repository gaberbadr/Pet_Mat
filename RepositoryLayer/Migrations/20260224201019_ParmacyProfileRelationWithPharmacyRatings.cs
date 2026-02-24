using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RepositoryLayer.Migrations
{
    /// <inheritdoc />
    public partial class ParmacyProfileRelationWithPharmacyRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyRatings_PharmacyProfiles_PharmacyProfileId",
                table: "PharmacyRatings");

            migrationBuilder.DropIndex(
                name: "IX_PharmacyRatings_PharmacyProfileId",
                table: "PharmacyRatings");

            migrationBuilder.DropColumn(
                name: "PharmacyProfileId",
                table: "PharmacyRatings");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_PharmacyProfiles_UserId",
                table: "PharmacyProfiles",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyRatings_PharmacyProfiles_PharmacyId",
                table: "PharmacyRatings",
                column: "PharmacyId",
                principalTable: "PharmacyProfiles",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyRatings_PharmacyProfiles_PharmacyId",
                table: "PharmacyRatings");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_PharmacyProfiles_UserId",
                table: "PharmacyProfiles");

            migrationBuilder.AddColumn<Guid>(
                name: "PharmacyProfileId",
                table: "PharmacyRatings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyRatings_PharmacyProfileId",
                table: "PharmacyRatings",
                column: "PharmacyProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyRatings_PharmacyProfiles_PharmacyProfileId",
                table: "PharmacyRatings",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfiles",
                principalColumn: "Id");
        }
    }
}
