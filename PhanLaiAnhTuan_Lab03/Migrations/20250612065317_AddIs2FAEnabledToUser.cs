using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhanLaiAnhTuan_Lab03.Migrations
{
    /// <inheritdoc />
    public partial class AddIs2FAEnabledToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Is2FAEnabled",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Is2FAEnabled",
                table: "AspNetUsers");
        }
    }
}
