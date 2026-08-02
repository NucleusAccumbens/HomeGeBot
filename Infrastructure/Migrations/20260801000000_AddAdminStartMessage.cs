using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class AddAdminStartMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Messages",
                columns: new[] { "Name", "Body", "PathToPhoto", "CreatedAt", "LastModified" },
                values: new object[,]
                {
                    { "adminStartMessage", "Привет, админ! Ты можешь управлять заявками через веб-панель.", null, DateTime.UtcNow, null },
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Messages",
                keyColumn: "Name",
                keyValue: "adminStartMessage");
        }
    }
}
