using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class SeedMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Messages",
                columns: new[] { "Name", "Body", "PathToPhoto", "CreatedAt", "LastModified" },
                values: new object[,]
                {
                    { "/start", "Привет! Чтобы подобрать квартиру в Тбилиси, заполните короткую анкету.\n\nСначала укажите <b>страну проживания</b>:", null, DateTime.UtcNow, null },
                    { "profession", "Укажите вашу <b>профессию или род деятельности</b>:", null, DateTime.UtcNow, null },
                    { "hasPets", "Есть ли у вас <b>домашние животные</b>?", null, DateTime.UtcNow, null },
                    { "term", "На какой <b>срок</b> планируете аренду?", null, DateTime.UtcNow, null },
                    { "flat", "Отлично! Теперь <b>перешлите пост с квартирой</b> из канала @propertyintbilisi", null, DateTime.UtcNow, null },
                    { "app", "✅ Заявка принята! Менеджер свяжется с вами в ближайшее время.", null, DateTime.UtcNow, null },
                    { "channelError", "❌ Пожалуйста, перешлите пост <b>из канала @propertyintbilisi</b>, а не из другого источника.", null, DateTime.UtcNow, null },
                    { "memoryCacheError", "⏰ Прошло слишком много времени. Данные не сохранились.\n\nНажмите /start чтобы начать заново.", null, DateTime.UtcNow, null },
                });

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "Messages", keyColumn: "Name", keyValues: new object[]
            {
                "/start", "profession", "hasPets", "term", "flat", "app", "channelError", "memoryCacheError"
            });

        }
    }
}
