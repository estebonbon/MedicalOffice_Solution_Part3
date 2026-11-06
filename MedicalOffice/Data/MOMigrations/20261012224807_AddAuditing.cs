using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalOffice.Data.MOMigrations
{
    /// <inheritdoc />
    public partial class AddAuditing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Patients",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Patients",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "MedicalTrials",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "MedicalTrials",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "MedicalTrials",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "MedicalTrials",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Doctors",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Doctors",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Doctors",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "Doctors",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MedicalTrials");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "MedicalTrials");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "MedicalTrials");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "MedicalTrials");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "Doctors");
        }
    }
}
