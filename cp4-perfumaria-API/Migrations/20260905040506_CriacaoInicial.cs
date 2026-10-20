using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Perfumaria.API.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "T_PRF_NOTAS_OLFATIVAS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    FAMILIA = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_PRF_NOTAS_OLFATIVAS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "T_PRF_PERFUMES",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    MARCA = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    GENERO = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    VOLUME_ML = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PRECO = table.Column<decimal>(type: "NUMBER(10,2)", nullable: false),
                    ANO_LANCAMENTO = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_PRF_PERFUMES", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "T_PRF_PERFUME_NOTA",
                columns: table => new
                {
                    PERFUME_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NOTA_OLFATIVA_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    POSICAO = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_PRF_PERFUME_NOTA", x => new { x.PERFUME_ID, x.NOTA_OLFATIVA_ID });
                    table.ForeignKey(
                        name: "FK_T_PRF_PERFUME_NOTA_T_PRF_NOTAS_OLFATIVAS_NOTA_OLFATIVA_ID",
                        column: x => x.NOTA_OLFATIVA_ID,
                        principalTable: "T_PRF_NOTAS_OLFATIVAS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_T_PRF_PERFUME_NOTA_T_PRF_PERFUMES_PERFUME_ID",
                        column: x => x.PERFUME_ID,
                        principalTable: "T_PRF_PERFUMES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_T_PRF_PERFUME_NOTA_NOTA_OLFATIVA_ID",
                table: "T_PRF_PERFUME_NOTA",
                column: "NOTA_OLFATIVA_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "T_PRF_PERFUME_NOTA");

            migrationBuilder.DropTable(
                name: "T_PRF_NOTAS_OLFATIVAS");

            migrationBuilder.DropTable(
                name: "T_PRF_PERFUMES");
        }
    }
}
