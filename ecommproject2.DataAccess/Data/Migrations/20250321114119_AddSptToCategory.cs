using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ecommproject2.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSptToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE PROCEDURE sp_CreateCategory
	                               @name  varchar(50)
                                   AS
	                               insert Categories values(@name)");
            migrationBuilder.Sql(@"CREATE PROCEDURE sp_UpdateCategory
                                   @id int,
	                               @name  varchar(50)
                                   AS
	                               update Categories set name = @name
                                   where id = @id");
            migrationBuilder.Sql(@"CREATE PROCEDURE sp_DeleteCategory
                                   @id int
                                   AS
	                               delete from Categories
                                   where id = @id ");
            migrationBuilder.Sql(@"CREATE PROCEDURE sp_GetCategories
                                   AS
	                               select * from Categories");
            migrationBuilder.Sql(@"CREATE PROCEDURE sp_GetCategory
                                   @id int
                                   AS
	                               select * from Categories where id = @id");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
