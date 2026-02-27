using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mym.Migrations
{
    /// <inheritdoc />
    public partial class AddLegalConsentTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LegalTermsAcceptedAtUtc",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LegalTermsAcceptedAtUtc",
                table: "AspNetUsers");
        }
    }
}
