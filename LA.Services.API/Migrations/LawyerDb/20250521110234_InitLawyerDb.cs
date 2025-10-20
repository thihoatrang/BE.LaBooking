using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LA.Services.API.Migrations.LawyerDb
{
    /// <inheritdoc />
    public partial class InitLawyerDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LawyerProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Spec = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LicenseNum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpYears = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Rating = table.Column<double>(type: "float", nullable: false),
                    PricePerHour = table.Column<double>(type: "float", nullable: false),
                    Img = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DayOfWeek = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkTime = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawyerProfiles", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "LawyerProfiles",
                columns: new[] { "Id", "Bio", "DayOfWeek", "Description", "ExpYears", "Img", "LicenseNum", "PricePerHour", "Rating", "Spec", "UserId", "WorkTime" },
                values: new object[,]
                {
                    { 1, "Luật sư chuyên dân sự, hơn 10 năm kinh nghiệm.", "Mon,Tue,Wed", "Hà Nội", 10, "lawyer1.jpg", "LS1234", 500000.0, 4.9000000000000004, "Dân sự,Hợp đồng", 1, "08:00-12:00" },
                    { 2, "Luật sư hình sự, tư vấn luật hơn 8 năm.", "Thu,Fri", "TP. HCM", 8, "lawyer2.jpg", "LS5678", 600000.0, 4.7000000000000002, "Hình sự,Tố tụng", 2, "13:00-17:00" },
                    { 3, "Luật sư chuyên về đất đai và bất động sản.", "Mon,Wed,Fri", "Đà Nẵng", 12, "lawyer3.jpg", "LS9001", 700000.0, 4.7999999999999998, "Đất đai,Bất động sản", 3, "09:00-11:00" },
                    { 4, "Luật sư trẻ đầy nhiệt huyết, chuyên luật doanh nghiệp.", "Tue,Thu", "Hải Phòng", 5, "lawyer4.jpg", "LS7004", 400000.0, 4.5999999999999996, "Doanh nghiệp,Hợp đồng", 4, "14:00-17:00" },
                    { 5, "Luật sư có chuyên môn sâu về hôn nhân gia đình.", "Sat,Sun", "Cần Thơ", 7, "lawyer5.jpg", "LS3010", 450000.0, 4.8499999999999996, "Hôn nhân,Ly hôn,Nuôi con", 5, "08:00-12:00" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LawyerProfiles");
        }
    }
}
