using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mym.Migrations
{
    /// <inheritdoc />
    public partial class _PendingCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Forums",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 11, 21, 29, 26, 0, DateTimeKind.Local).AddTicks(9643));

            migrationBuilder.UpdateData(
                table: "Forums",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 11, 21, 29, 26, 0, DateTimeKind.Local).AddTicks(9876));

            migrationBuilder.UpdateData(
                table: "Forums",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 11, 21, 29, 26, 0, DateTimeKind.Local).AddTicks(9879));

            migrationBuilder.UpdateData(
                table: "Forums",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 11, 21, 29, 26, 0, DateTimeKind.Local).AddTicks(9881));

            migrationBuilder.UpdateData(
                table: "Forums",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 11, 21, 29, 26, 0, DateTimeKind.Local).AddTicks(9884));

            migrationBuilder.UpdateData(
                table: "Forums",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 11, 21, 29, 26, 0, DateTimeKind.Local).AddTicks(9886));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Forums",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 11, 21, 29, 26, 693, DateTimeKind.Local).AddTicks(9643));

            migrationBuilder.UpdateData(
                table: "Forums",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 11, 21, 29, 26, 693, DateTimeKind.Local).AddTicks(9876));

            migrationBuilder.UpdateData(
                table: "Forums",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 11, 21, 29, 26, 693, DateTimeKind.Local).AddTicks(9879));

            migrationBuilder.UpdateData(
                table: "Forums",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 11, 21, 29, 26, 693, DateTimeKind.Local).AddTicks(9881));

            migrationBuilder.UpdateData(
                table: "Forums",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 11, 21, 29, 26, 693, DateTimeKind.Local).AddTicks(9884));

            migrationBuilder.UpdateData(
                table: "Forums",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 11, 21, 29, 26, 693, DateTimeKind.Local).AddTicks(9886));
        }
    }
}
