using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRApp.Migrations
{
    /// <inheritdoc />
    public partial class OrderMoveType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgreementId",
                table: "Orders",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MoveType",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewDepartment",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewPosition",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Probation",
                table: "Orders",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgreementId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "MoveType",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "NewDepartment",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "NewPosition",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Probation",
                table: "Orders");
        }
    }
}
