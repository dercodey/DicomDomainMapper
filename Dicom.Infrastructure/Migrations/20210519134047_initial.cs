using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Elekta.Capability.Dicom.Infrastructure.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DicomSeries",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeriesInstanceUid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PatientName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PatientID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AcquisitionDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpectedInstanceCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DicomSeries", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DicomInstances",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DicomSeriesId = table.Column<int>(type: "int", nullable: false),
                    SopInstanceUid = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DicomInstances", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DicomInstances_DicomSeries_DicomSeriesId",
                        column: x => x.DicomSeriesId,
                        principalTable: "DicomSeries",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DicomElements",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DicomInstanceId = table.Column<int>(type: "int", nullable: false),
                    DicomTag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DicomElements", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DicomElements_DicomInstances_DicomInstanceId",
                        column: x => x.DicomInstanceId,
                        principalTable: "DicomInstances",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DicomElements_DicomInstanceId",
                table: "DicomElements",
                column: "DicomInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_DicomInstances_DicomSeriesId",
                table: "DicomInstances",
                column: "DicomSeriesId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DicomElements");

            migrationBuilder.DropTable(
                name: "DicomInstances");

            migrationBuilder.DropTable(
                name: "DicomSeries");
        }
    }
}
