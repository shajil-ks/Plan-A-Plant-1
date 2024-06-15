using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plan_A_Plant.Migrations
{
    /// <inheritdoc />
    public partial class AddwalletSessionIdToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WalletPaymentIntentId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WalletSessionId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WalletPaymentIntentId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "WalletSessionId",
                table: "AspNetUsers");
        }
    }
}
