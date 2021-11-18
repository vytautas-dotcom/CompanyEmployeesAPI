using Microsoft.EntityFrameworkCore.Migrations;

namespace CompanyEmployees.Migrations
{
    public partial class AddPropertyPhoneNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9317c9cd-26b7-4439-97cc-fe72bde58f54");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f67ad8f6-60fc-40f9-9ed9-aed401e0c4e9");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "2ab3004a-a412-48aa-a48e-44ce2257aaee", "f0649079-33de-4b7a-9d87-5cb8491ebc6c", "Manager", "MANAGER" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "aaa53d97-8c99-4c0c-99b1-bef0aeae60ac", "e636b3f9-93d3-4a86-a090-5fa2a37dc42e", "Administrator", "ADMINISTRATOR" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2ab3004a-a412-48aa-a48e-44ce2257aaee");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "aaa53d97-8c99-4c0c-99b1-bef0aeae60ac");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "f67ad8f6-60fc-40f9-9ed9-aed401e0c4e9", "2828e157-299f-40f5-b7a7-7e776c318d40", "Manager", "MANAGER" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "9317c9cd-26b7-4439-97cc-fe72bde58f54", "303bc80c-788b-4b2d-964b-b31080ec2b35", "Administrator", "ADMINISTRATOR" });
        }
    }
}
