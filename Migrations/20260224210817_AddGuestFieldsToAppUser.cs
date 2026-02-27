using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mym.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestFieldsToAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "GuestExpiresAtUtc",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGuest",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuestExpiresAtUtc",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsGuest",
                table: "AspNetUsers");
        }
    }
}
