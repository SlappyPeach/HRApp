using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmpHis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "EmploymentHistories");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "EmploymentHistories",
                newName: "Reason");

            migrationBuilder.RenameColumn(
                name: "Speciality",
                table: "EmploymentHistories",
                newName: "Position");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "EmploymentHistories",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "EmploymentHistories",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "RecordNumber",
                table: "EmploymentHistories",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "EmploymentHistories");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "EmploymentHistories");

            migrationBuilder.DropColumn(
                name: "RecordNumber",
                table: "EmploymentHistories");

            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "EmploymentHistories",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "Position",
                table: "EmploymentHistories",
                newName: "Speciality");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "EmploymentHistories",
                type: "TEXT",
                nullable: true);
        }
    }
}
