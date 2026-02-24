using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RepositoryLayer.Migrations
{
    /// <inheritdoc />
    public partial class DoctorProfileRelationWithDoctorRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorRatings_DoctorProfiles_DoctorProfileId",
                table: "DoctorRatings");

            migrationBuilder.DropIndex(
                name: "IX_DoctorRatings_DoctorProfileId",
                table: "DoctorRatings");

            migrationBuilder.DropColumn(
                name: "DoctorProfileId",
                table: "DoctorRatings");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_DoctorProfiles_UserId",
                table: "DoctorProfiles",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorRatings_DoctorProfiles_DoctorId",
                table: "DoctorRatings",
                column: "DoctorId",
                principalTable: "DoctorProfiles",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorRatings_DoctorProfiles_DoctorId",
                table: "DoctorRatings");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_DoctorProfiles_UserId",
                table: "DoctorProfiles");

            migrationBuilder.AddColumn<Guid>(
                name: "DoctorProfileId",
                table: "DoctorRatings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorRatings_DoctorProfileId",
                table: "DoctorRatings",
                column: "DoctorProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorRatings_DoctorProfiles_DoctorProfileId",
                table: "DoctorRatings",
                column: "DoctorProfileId",
                principalTable: "DoctorProfiles",
                principalColumn: "Id");
        }
    }
}
