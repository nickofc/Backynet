create table public.jobs
(
    id                 uuid                     not null
        constraint jobs_pk
            primary key,
    state              integer                  not null,
    created_at         timestamp with time zone not null,
    descriptor         bytea,
    server_name        varchar,
    cron               varchar,
    group_name         varchar,
    next_occurrence_at timestamp with time zone,
    row_version        integer,
    errors             bytea,
    context            bytea
);

create table public.groups
(
    group_name             varchar not null
        constraint groups_pk
            primary key,
    max_concurrent_threads integer
);

create table public.servers
(
    server_name  varchar not null
        constraint clients_pk
            primary key,
    heartbeat_on timestamp with time zone,
    created_at   timestamp with time zone
);
