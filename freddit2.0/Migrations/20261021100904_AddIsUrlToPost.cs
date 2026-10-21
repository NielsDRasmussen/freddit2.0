using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace freddit2._0.Migrations
{
    /// <inheritdoc />
    public partial class AddIsUrlToPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUrl",
                table: "Posts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsUrl",
                table: "Comments",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUrl",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "IsUrl",
                table: "Comments");
        }
    }
}
