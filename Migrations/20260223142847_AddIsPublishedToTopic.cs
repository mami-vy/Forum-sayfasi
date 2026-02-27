using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mym.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPublishedToTopic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Topics",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Topics");
        }
    }
}
