using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriceQ.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StoreUrl",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoreUrl",
                table: "Products");
        }
    }
}
