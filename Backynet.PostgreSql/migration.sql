DO
$$
    DECLARE
        version     integer = 0;
        search_path varchar = @search_path;
    BEGIN
        EXECUTE format('CREATE SCHEMA IF NOT EXISTS %I', search_path);
        EXECUTE format('SET search_path TO %I', search_path);

        IF EXISTS (SELECT 1
                   FROM information_schema.tables
                   WHERE table_schema = search_path
                     AND table_name = 'schema_version') THEN
            SELECT schema_version.version INTO version FROM schema_version;
        END IF;

        IF (version = 0) THEN
            CREATE TABLE schema_version
            (
                version    int NOT NULL
                    CONSTRAINT schema_version_pk PRIMARY KEY,
                applied_at timestamp with time zone DEFAULT now()
            );

            CREATE TABLE jobs
            (
                id                 uuid                     NOT NULL
                    CONSTRAINT jobs_pk PRIMARY KEY,
                state              integer                  NOT NULL,
                created_at         timestamp with time zone NOT NULL,
                descriptor         bytea,
                server_name        varchar(128),
                cron               varchar(128),
                group_name         varchar(128),
                next_occurrence_at timestamp with time zone,
                row_version        integer,
                errors             bytea,
                context            bytea
            );

            CREATE TABLE servers
            (
                server_name  varchar(128) NOT NULL
                    CONSTRAINT servers_pk PRIMARY KEY,
                heartbeat_on timestamp with time zone,
                created_at   timestamp with time zone
            );

            INSERT INTO schema_version VALUES (1);
            version := 1;
        END IF;

        EXECUTE format('RESET search_path');
    END;
$$;