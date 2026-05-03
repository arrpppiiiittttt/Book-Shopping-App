using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ecommproject2.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSptToCoverTypeModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE PROCEDURE sp_CreateCoverType
	                               @name  varchar(50)
                                   AS
	                               insert CoverTypes values(@name)");
            migrationBuilder.Sql(@"CREATE PROCEDURE sp_UpdateCoverType
                                   @id int,
	                               @name  varchar(50)
                                   AS
	                               update CoverTypes set name = @name
                                   where id = @id");
            migrationBuilder.Sql(@"CREATE PROCEDURE sp_DeleteCoverType
                                   @id int
                                   AS
	                               delete from CoverTypes
                                   where id = @id ");
            migrationBuilder.Sql(@"CREATE PROCEDURE sp_GetCoverTypes
                                   AS
	                               select * from CoverTypes");
            migrationBuilder.Sql(@"CREATE PROCEDURE sp_GetCoverType
                                   @id int
                                   AS
	                               select * from CoverTypes where id = @id");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
