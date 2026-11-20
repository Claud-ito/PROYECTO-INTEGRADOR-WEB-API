using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRE_EMERGENCIAS_WEB_API.Data.Migrations
{
    /// <inheritdoc />
    public partial class InicialUbicaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Modulos",
                columns: table => new
                {
                    ModuloId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, collation: "Latin1_General_100_CI_AS"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modulos", x => x.ModuloId);
                    table.CheckConstraint("CK_Modulos_Codigo_NoVacio", "LEN(LTRIM(RTRIM([Codigo]))) > 0");
                    table.CheckConstraint("CK_Modulos_Nombre_NoVacio", "LEN(LTRIM(RTRIM([Nombre]))) > 0");
                });

            migrationBuilder.CreateTable(
                name: "Pisos",
                columns: table => new
                {
                    PisoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuloId = table.Column<int>(type: "int", nullable: false),
                    Denominacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, collation: "Latin1_General_100_CI_AS"),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pisos", x => x.PisoId);
                    table.CheckConstraint("CK_Pisos_Denominacion_NoVacia", "LEN(LTRIM(RTRIM([Denominacion]))) > 0");
                    table.ForeignKey(
                        name: "FK_Pisos_Modulos_ModuloId",
                        column: x => x.ModuloId,
                        principalTable: "Modulos",
                        principalColumn: "ModuloId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Modulos_Codigo",
                table: "Modulos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pisos_ModuloId_Denominacion",
                table: "Pisos",
                columns: new[] { "ModuloId", "Denominacion" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pisos");

            migrationBuilder.DropTable(
                name: "Modulos");
        }
    }
}
