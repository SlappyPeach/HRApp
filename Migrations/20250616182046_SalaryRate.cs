using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HRApp.Migrations
{
    /// <inheritdoc />
    public partial class SalaryRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 1,
                column: "Amount",
                value: 62090m);

            migrationBuilder.UpdateData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 2,
                column: "Amount",
                value: 47275m);

            migrationBuilder.UpdateData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 3,
                column: "Amount",
                value: 34120m);

            migrationBuilder.InsertData(
                table: "SalaryRates",
                columns: new[] { "Id", "Amount", "PositionId" },
                values: new object[,]
                {
                    { 4, 34580m, 4 },
                    { 5, 56607m, 5 },
                    { 6, 41716m, 6 },
                    { 7, 42206m, 7 },
                    { 8, 30445m, 8 },
                    { 9, 37245m, 9 },
                    { 10, 35678m, 10 },
                    { 11, 34870m, 11 },
                    { 12, 31237m, 12 },
                    { 13, 33000m, 13 },
                    { 14, 36110m, 14 },
                    { 15, 36500m, 15 },
                    { 16, 29400m, 16 },
                    { 17, 31500m, 17 },
                    { 18, 32000m, 18 },
                    { 19, 28950m, 19 },
                    { 20, 37500m, 20 },
                    { 21, 41500m, 21 },
                    { 22, 29000m, 22 },
                    { 23, 25650m, 23 },
                    { 24, 30200m, 24 },
                    { 25, 26500m, 25 },
                    { 26, 28000m, 26 },
                    { 27, 27000m, 27 },
                    { 28, 26000m, 28 },
                    { 29, 22000m, 29 },
                    { 30, 23000m, 30 },
                    { 31, 25000m, 31 },
                    { 32, 21000m, 32 },
                    { 33, 20000m, 33 },
                    { 34, 30000m, 34 },
                    { 35, 24000m, 35 },
                    { 36, 22000m, 36 },
                    { 37, 23000m, 37 },
                    { 38, 21000m, 38 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.UpdateData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 1,
                column: "Amount",
                value: 100000m);

            migrationBuilder.UpdateData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 2,
                column: "Amount",
                value: 60000m);

            migrationBuilder.UpdateData(
                table: "SalaryRates",
                keyColumn: "Id",
                keyValue: 3,
                column: "Amount",
                value: 65000m);
        }
    }
}
