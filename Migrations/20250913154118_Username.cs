using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

#nullable disable

namespace PRM_BE.Migrations
{
    /// <inheritdoc />
    public partial class Username : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Users",
                newName: "UserName");

            migrationBuilder.Sql(@"
               AlTER TABLE ""Users""
                ALTER COLUMN ""Role"" TYPE integer 
                USING (CASE
                     WHEN ""Role"" = 'Admin' THEN 1
                     WHEN ""Role"" = 'User' THEN 2
                     WHEN ""Role"" = 'Staff' THEN 3
                     ELSE 2
                END);
            ");


            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Users",
                newName: "Name");

            migrationBuilder.Sql(@"
               AlTER TABLE ""Users""
                ALTER COLUMN ""Role"" TYPE integer 
                USING (CASE
                     WHEN ""Role"" = '1' THEN 'Admin'
                     WHEN ""Role"" = '2' THEN 'User'
                     WHEN ""Role"" = '3' THEN 'Staff'
                     ELSE 2
                END);
            ");
        }
    }
}
