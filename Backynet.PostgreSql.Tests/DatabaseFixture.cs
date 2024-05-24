using Npgsql;

namespace Backynet.PostgreSql.Tests;

public class DatabaseFixture
{
    public string ConnectionString { get; }

    public DatabaseFixture()
    {
        var connectionString = Environment.GetEnvironmentVariable("BACKYNET_CONNECTION_STRING");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Unable to load connection string.");
        }

        ConnectionString = connectionString;
    }

    public void Setup()
    {
    }

    public void DropAll()
    {
        const string sql = """
                           DO
                           $$
                           DECLARE
                               rec RECORD;
                           BEGIN
                               -- Drop all tables
                               FOR rec IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public') LOOP
                                   EXECUTE 'DROP TABLE IF EXISTS ' || quote_ident(rec.tablename) || ' CASCADE';
                               END LOOP;
                           
                               -- Drop all views
                               FOR rec IN (SELECT viewname FROM pg_views WHERE schemaname = 'public') LOOP
                                   EXECUTE 'DROP VIEW IF EXISTS ' || quote_ident(rec.viewname) || ' CASCADE';
                               END LOOP;
                           
                               -- Drop all sequences
                               FOR rec IN (SELECT sequencename FROM pg_sequences WHERE schemaname = 'public') LOOP
                                   EXECUTE 'DROP SEQUENCE IF EXISTS ' || quote_ident(rec.sequencename) || ' CASCADE';
                               END LOOP;
                           
                               -- Drop all functions
                               FOR rec IN (SELECT proname FROM pg_proc INNER JOIN pg_namespace ns ON (pg_proc.pronamespace = ns.oid) WHERE ns.nspname = 'public') LOOP
                                   EXECUTE 'DROP FUNCTION IF EXISTS ' || quote_ident(rec.proname) || ' CASCADE';
                               END LOOP;
                           
                               -- Drop all types
                               FOR rec IN (SELECT typname FROM pg_type INNER JOIN pg_namespace ns ON (pg_type.typnamespace = ns.oid) WHERE ns.nspname = 'public') LOOP
                                   EXECUTE 'DROP TYPE IF EXISTS ' || quote_ident(rec.typname) || ' CASCADE';
                               END LOOP;
                           
                               -- Drop all schemas (except public)
                               FOR rec IN (SELECT nspname FROM pg_namespace WHERE nspname NOT IN ('public', 'information_schema', 'pg_catalog')) LOOP
                                   EXECUTE 'DROP SCHEMA IF EXISTS ' || quote_ident(rec.nspname) || ' CASCADE';
                               END LOOP;
                           END
                           $$;
                           """;

        using var con = new NpgsqlConnection(ConnectionString);
        using var cmd = con.CreateCommand();
        cmd.CommandText = sql;
        con.Open();
        cmd.ExecuteNonQuery();
    }
}