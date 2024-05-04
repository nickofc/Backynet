create table public.jobs
(
    id                 uuid                     not null
        constraint jobs_pk
            primary key,
    state              integer                  not null,
    created_at         timestamp with time zone not null,
    base_type          varchar,
    method             varchar,
    arguments          varchar,
    server_name        varchar,
    cron               varchar,
    group_name         varchar,
    next_occurrence_at timestamp with time zone
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
