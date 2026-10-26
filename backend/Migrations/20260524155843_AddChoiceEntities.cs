using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddChoiceEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TimeStamp",
                table: "Runs",
                newName: "TimeStarted");

            migrationBuilder.RenameColumn(
                name: "Score",
                table: "Runs",
                newName: "Won");

            migrationBuilder.RenameColumn(
                name: "DidWin",
                table: "Runs",
                newName: "TimeElapsed");

            migrationBuilder.AddColumn<string>(
                name: "BuildId",
                table: "Runs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GameMode",
                table: "Runs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KilledByEncounter",
                table: "Runs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SchemaVersion",
                table: "Runs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Seed",
                table: "Runs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CardChoice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RunId = table.Column<int>(type: "INTEGER", nullable: false),
                    CardId = table.Column<string>(type: "TEXT", nullable: false),
                    Picked = table.Column<bool>(type: "INTEGER", nullable: false),
                    Act = table.Column<int>(type: "INTEGER", nullable: false),
                    Floor = table.Column<int>(type: "INTEGER", nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardChoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardChoice_Runs_RunId",
                        column: x => x.RunId,
                        principalTable: "Runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RelicChoice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RunId = table.Column<int>(type: "INTEGER", nullable: false),
                    RelicId = table.Column<string>(type: "TEXT", nullable: false),
                    Picked = table.Column<bool>(type: "INTEGER", nullable: false),
                    Act = table.Column<int>(type: "INTEGER", nullable: false),
                    Floor = table.Column<int>(type: "INTEGER", nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelicChoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RelicChoice_Runs_RunId",
                        column: x => x.RunId,
                        principalTable: "Runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardChoice_RunId",
                table: "CardChoice",
                column: "RunId");

            migrationBuilder.CreateIndex(
                name: "IX_RelicChoice_RunId",
                table: "RelicChoice",
                column: "RunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardChoice");

            migrationBuilder.DropTable(
                name: "RelicChoice");

            migrationBuilder.DropColumn(
                name: "BuildId",
                table: "Runs");

            migrationBuilder.DropColumn(
                name: "GameMode",
                table: "Runs");

            migrationBuilder.DropColumn(
                name: "KilledByEncounter",
                table: "Runs");

            migrationBuilder.DropColumn(
                name: "SchemaVersion",
                table: "Runs");

            migrationBuilder.DropColumn(
                name: "Seed",
                table: "Runs");

            migrationBuilder.RenameColumn(
                name: "Won",
                table: "Runs",
                newName: "Score");

            migrationBuilder.RenameColumn(
                name: "TimeStarted",
                table: "Runs",
                newName: "TimeStamp");

            migrationBuilder.RenameColumn(
                name: "TimeElapsed",
                table: "Runs",
                newName: "DidWin");
        }
    }
}
