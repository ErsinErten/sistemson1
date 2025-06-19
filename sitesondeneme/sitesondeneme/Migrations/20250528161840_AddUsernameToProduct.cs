using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sitesondeneme.Migrations
{
    /// <inheritdoc />
    public partial class AddUsernameToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Username",
                table: "Products");
        }
    }
}
