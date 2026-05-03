using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ecommproject2.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ProductDiscontinue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDiscontinued",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDiscontinued",
                table: "Products");
        }
    }
}
