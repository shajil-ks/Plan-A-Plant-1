using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plan_A_Plant.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWalletHistoryjasonFromApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WalletHistoryJson",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WalletHistoryJson",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
