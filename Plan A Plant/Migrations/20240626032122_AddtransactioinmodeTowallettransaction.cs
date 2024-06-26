using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plan_A_Plant.Migrations
{
    /// <inheritdoc />
    public partial class AddtransactioinmodeTowallettransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TransactionMode",
                table: "WalletTransactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionMode",
                table: "WalletTransactions");
        }
    }
}
