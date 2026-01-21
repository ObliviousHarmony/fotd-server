using FluentMigrator.Builders.Alter.Table;
using FluentMigrator.Builders.Create.Table;

namespace FOMServer.Shared.Infrastructure.Migrations
{
    public static class FluentMigratorMySQLExtensions
    {
        // ---------------- CREATE TABLE: TEXT TYPES ----------------

        public static ICreateTableColumnOptionOrWithColumnSyntax AsTinyText(
            this ICreateTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("TINYTEXT");
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax AsText(
            this ICreateTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("TEXT");
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax AsMediumText(
            this ICreateTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("MEDIUMTEXT");
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax AsLongText(
            this ICreateTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("LONGTEXT");
        }

        // ---------------- CREATE TABLE: TIMESTAMPS ----------------

        public static ICreateTableColumnOptionOrWithColumnSyntax AsTimestamp(
            this ICreateTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("TIMESTAMP");
        }

        public static ICreateTableWithColumnSyntax AsCreatedAtTimestamp(
            this ICreateTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP");
        }

        public static ICreateTableWithColumnSyntax AsUpdatedAtTimestamp(
            this ICreateTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
        }

        // ---------------- CREATE TABLE: UNSIGNED INTEGERS ----------------

        public static ICreateTableColumnOptionOrWithColumnSyntax AsUInt8(
            this ICreateTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("TINYINT UNSIGNED");
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax AsUInt16(
            this ICreateTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("SMALLINT UNSIGNED");
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax AsUInt32(
            this ICreateTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("INT UNSIGNED");
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax AsUInt64(
            this ICreateTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("BIGINT UNSIGNED");
        }

        // ---------------- ALTER TABLE: TEXT TYPES ----------------

        public static IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax AsTinyText(
            this IAlterTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("TINYTEXT");
        }

        public static IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax AsText(
            this IAlterTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("TEXT");
        }

        public static IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax AsMediumText(
            this IAlterTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("MEDIUMTEXT");
        }

        public static IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax AsLongText(
            this IAlterTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("LONGTEXT");
        }

        // ---------------- ALTER TABLE: TIMESTAMPS ----------------

        public static IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax AsTimestamp(
            this IAlterTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("TIMESTAMP");
        }

        public static IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax AsCreatedAtTimestamp(
            this IAlterTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP");
        }

        public static IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax AsUpdatedAtTimestamp(
            this IAlterTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
        }

        // ---------------- ALTER TABLE: UNSIGNED INTEGERS ----------------

        public static IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax AsUInt8(
            this IAlterTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("TINYINT UNSIGNED");
        }

        public static IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax AsUInt16(
            this IAlterTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("SMALLINT UNSIGNED");
        }

        public static IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax AsUInt32(
            this IAlterTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("INT UNSIGNED");
        }

        public static IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax AsUInt64(
            this IAlterTableColumnAsTypeSyntax column)
        {
            return column.AsCustom("BIGINT UNSIGNED");
        }
    }
}
