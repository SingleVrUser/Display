using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace DataAccess.Extension;

public static class MigrationExtension
{
    public static void UpdateTableWithNewPrimaryKey<TColumns>(this MigrationBuilder migrationBuilder,
        string oldName,
        string newName,
        Func<ColumnsBuilder,TColumns> columns,
        Action<CreateTableBuilder<TColumns>>? constraints,
        string[]? oldColumns = null,
        Expression<Func<TColumns, object>>? newColumns = null)
    {
        var tmpName = "tmp_" + newName;
        
        
        // 临时表
        migrationBuilder.CreateTable(
            name: tmpName,
            columns: columns,
            constraints: constraints);

        if (oldColumns is not null && newColumns is not null)
        {
            var oldColumnStr = string.Join(",", oldColumns);
            var newColumnArray = newColumns.Body.Type.GetProperties().Select(x=>x.Name).ToArray();
            var newColumnStr = string.Join(",", newColumnArray);
            
            // 移动
            migrationBuilder.Sql($"INSERT INTO {tmpName} ({newColumnStr}) " +
                                 $"SELECT {oldColumnStr} from {oldName};");
        }
            
        // 删除旧表
        migrationBuilder.DropTable(oldName);

        // 更新临时表名字
        migrationBuilder.RenameTable(
            name: tmpName,
            newName: newName);
    }

    public static void AddUpdateTimeTrigger(this MigrationBuilder migrationBuilder, string tableName)
    {
        migrationBuilder.Sql($"CREATE TRIGGER {tableName}_Update AFTER UPDATE ON \"{tableName}\" BEGIN UPDATE {tableName} SET update_time = DATETIME('now'); END;");
    }
}