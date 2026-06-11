using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigBall.Api.Migrations;

/// <inheritdoc />
public partial class ProfileCreateDateAndAuthUserTrigger : Migration
{
    private const string InstallAuthProfileTriggerSql = @"
DO $outer$
BEGIN
  IF to_regclass('auth.users') IS NOT NULL THEN
    CREATE OR REPLACE FUNCTION public.handle_new_user()
    RETURNS trigger
    LANGUAGE plpgsql
    SECURITY DEFINER
    SET search_path = public
    AS $fn$
    BEGIN
      INSERT INTO public.profiles (id, email, display_name, avatar_url, is_platform_admin, create_date)
      VALUES (
        new.id,
        COALESCE(new.email, ''),
        COALESCE(
          NULLIF(btrim(COALESCE(new.raw_user_meta_data->>'full_name', '')), ''),
          NULLIF(btrim(COALESCE(new.raw_user_meta_data->>'name', '')), ''),
          NULLIF(split_part(COALESCE(new.email, ''), '@', 1), ''),
          'Usuário BigBall'
        ),
        NULLIF(btrim(COALESCE(
          new.raw_user_meta_data->>'avatar_url',
          new.raw_user_meta_data->>'picture'
        )), ''),
        false,
        COALESCE(new.created_at, now())
      )
      ON CONFLICT (id) DO NOTHING;
      RETURN new;
    END;
    $fn$;

    DROP TRIGGER IF EXISTS on_auth_user_created ON auth.users;
    CREATE TRIGGER on_auth_user_created
      AFTER INSERT ON auth.users
      FOR EACH ROW
      EXECUTE PROCEDURE public.handle_new_user();
  END IF;
END;
$outer$;
";

    private const string RemoveAuthProfileTriggerSql = @"
DO $outer$
BEGIN
  IF to_regclass('auth.users') IS NOT NULL THEN
    DROP TRIGGER IF EXISTS on_auth_user_created ON auth.users;
  END IF;
END;
$outer$;

DROP FUNCTION IF EXISTS public.handle_new_user();
";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "create_date",
            table: "profiles",
            type: "timestamp with time zone",
            nullable: false,
            defaultValueSql: "now()");

        migrationBuilder.Sql(InstallAuthProfileTriggerSql);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(RemoveAuthProfileTriggerSql);

        migrationBuilder.DropColumn(
            name: "create_date",
            table: "profiles");
    }
}
