if object_id('dbo.FND_LIQUIDACION_PROCESO', 'U') is null
begin
    create table dbo.FND_LIQUIDACION_PROCESO
    (
        PROCESO_ID uniqueidentifier not null,
        COD_OPERADORA int not null,
        COD_PLAN varchar(20) not null,
        DOCUMENTO_REFERENCIA varchar(30) not null,
        FECHA_PROCESO datetime not null,
        PROCESO_CODIGO char(1) not null,
        TIPO_DOCUMENTO char(2) not null,
        TIPO_LIQUIDACION char(1) not null,
        USUARIO varchar(30) not null,
        OFICINA_TITULAR varchar(10) not null,
        OFICINA_UNIDAD varchar(10) not null,
        OFICINA_CENTRO_COSTO varchar(10) not null,
        ENLACE int not null,
        MULTA decimal(19, 4) not null,
        NOTAS varchar(1000) not null,
        RETENCION_CODIGO varchar(20) not null,
        CUENTA_LIQUIDACION varchar(50) not null,
        FECHA_VENCE datetime null,
        COD_CONTABILIDAD int not null,
        ESTADO char(1) not null,
        TOTAL_CONTRATOS int not null,
        PROCESADOS int not null,
        SOLICITUD_HASH char(64) not null,
        ERROR_MENSAJE varchar(500) null,
        REGISTRO_FECHA datetime not null,
        ACTUALIZACION_FECHA datetime not null,
        constraint PK_FND_LIQUIDACION_PROCESO primary key (PROCESO_ID)
    );

end;

if col_length('dbo.FND_LIQUIDACION_PROCESO', 'SOLICITUD_HASH') is null
begin
    alter table dbo.FND_LIQUIDACION_PROCESO
        add SOLICITUD_HASH char(64) null;

    update dbo.FND_LIQUIDACION_PROCESO
    set SOLICITUD_HASH = replicate('0', 64)
    where SOLICITUD_HASH is null;

    alter table dbo.FND_LIQUIDACION_PROCESO
        alter column SOLICITUD_HASH char(64) not null;
end;

if not exists
(
    select 1
    from sys.indexes
    where object_id = object_id('dbo.FND_LIQUIDACION_PROCESO')
      and name = 'UX_FND_LIQUIDACION_PROCESO_ACTIVO'
)
    create unique index UX_FND_LIQUIDACION_PROCESO_ACTIVO
        on dbo.FND_LIQUIDACION_PROCESO (USUARIO, COD_OPERADORA, COD_PLAN)
        where ESTADO = 'P';

if object_id('dbo.FND_LIQUIDACION_PROCESO_DET', 'U') is null
begin
    create table dbo.FND_LIQUIDACION_PROCESO_DET
    (
        PROCESO_ID uniqueidentifier not null,
        COD_CONTRATO bigint not null,
        APORTES decimal(19, 4) not null,
        RENDIMIENTO decimal(19, 4) not null,
        BANCO_FINAL varchar(20) not null,
        CUENTA_FINAL varchar(50) not null,
        ESTADO char(1) not null,
        PROCESO_FECHA datetime null,
        constraint PK_FND_LIQUIDACION_PROCESO_DET
            primary key (PROCESO_ID, COD_CONTRATO),
        constraint FK_FND_LIQUIDACION_PROCESO_DET
            foreign key (PROCESO_ID)
            references dbo.FND_LIQUIDACION_PROCESO (PROCESO_ID)
    );

end;

if not exists
(
    select 1
    from sys.indexes
    where object_id = object_id('dbo.FND_LIQUIDACION_PROCESO_DET')
      and name = 'IX_FND_LIQUIDACION_PROCESO_DET_PENDIENTE'
)
    create index IX_FND_LIQUIDACION_PROCESO_DET_PENDIENTE
        on dbo.FND_LIQUIDACION_PROCESO_DET (PROCESO_ID, ESTADO, COD_CONTRATO);
