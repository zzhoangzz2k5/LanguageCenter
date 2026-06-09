using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhoneShop.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Products",
                type: "nvarchar(200)",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE Products
                SET Slug = LOWER(
                    REPLACE(
                        REPLACE(
                            REPLACE(LTRIM(RTRIM(Name)), ' ', '-'),
                            '/',
                            '-'
                        ),
                        '''',
                        ''
                    )
                )
                WHERE Slug IS NULL AND Name IS NOT NULL;

                UPDATE Products
                SET Slug = CONCAT('product-', Id)
                WHERE Slug IS NULL OR Slug = '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Products");
        }
    }
}
