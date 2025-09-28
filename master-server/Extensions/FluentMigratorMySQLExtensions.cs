using FluentMigrator.Builders.Alter.Table;
using FluentMigrator.Builders.Create.Table;

namespace FOMServer.Master.Extensions
{
    public static class FluentMigratorMySQLExtensions
    {
        // ---------------- CREATE TABLE ----------------

        public static ICreateTableColumnOptionOrWithColumnSyntax AsUnsignedByte(
            this ICreateTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("TINYINT UNSIGNED");
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax AsUnsignedInt16(
            this ICreateTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("SMALLINT UNSIGNED");
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax AsUnsignedInt(
            this ICreateTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("INT UNSIGNED");
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax AsUnsignedLong(
            this ICreateTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("BIGINT UNSIGNED");
        }

        // ---------------- ALTER TABLE ----------------

        public static IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax AsUnsignedByte(
            this IAlterTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("TINYINT UNSIGNED");
        }

        public static IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax AsUnsignedInt16(
            this IAlterTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("SMALLINT UNSIGNED");
        }

        public static IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax AsUnsignedInt(
            this IAlterTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("INT UNSIGNED");
        }

        public static IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax AsUnsignedLong(
            this IAlterTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("BIGINT UNSIGNED");
        }
    }
}
