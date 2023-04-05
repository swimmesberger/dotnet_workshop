using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eShop.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Carts",
                columns: new[] { "Id", "CreatedAt", "LastAccess" },
                values: new object[] { new Guid("03fa107f-2fd8-4627-9eab-d0cc3a1537b9"), new DateTimeOffset(new DateTime(2023, 4, 5, 14, 48, 43, 636, DateTimeKind.Unspecified).AddTicks(7969), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2023, 4, 5, 14, 48, 43, 636, DateTimeKind.Unspecified).AddTicks(7973), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CreatedAt", "Name", "Price" },
                values: new object[] { new Guid("a7892885-18ca-4b84-b51b-611462ffcbb0"), new DateTimeOffset(new DateTime(2023, 4, 5, 14, 48, 43, 636, DateTimeKind.Unspecified).AddTicks(7977), new TimeSpan(0, 0, 0, 0, 0)), "Test Product", 10.50m });

            migrationBuilder.InsertData(
                table: "CartItem",
                columns: new[] { "Id", "Amount", "CartId", "CreatedAt", "ProductId" },
                values: new object[] { new Guid("03b8309c-154f-4a4c-9a65-9f851765cfc9"), 5, new Guid("03fa107f-2fd8-4627-9eab-d0cc3a1537b9"), new DateTimeOffset(new DateTime(2023, 4, 5, 14, 48, 43, 636, DateTimeKind.Unspecified).AddTicks(7984), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a7892885-18ca-4b84-b51b-611462ffcbb0") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CartItem",
                keyColumn: "Id",
                keyValue: new Guid("03b8309c-154f-4a4c-9a65-9f851765cfc9"));

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: new Guid("03fa107f-2fd8-4627-9eab-d0cc3a1537b9"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("a7892885-18ca-4b84-b51b-611462ffcbb0"));
        }
    }
}
