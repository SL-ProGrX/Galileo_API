IF OBJECT_ID(N'dbo.CRD_DIG_TiposArchivos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CRD_DIG_TiposArchivos
    (
        IdTipoArchivo SMALLINT NOT NULL,
        Descripcion VARCHAR(100) NOT NULL,
        Activo BIT NOT NULL,
        RegistroFecha DATETIME NULL,
        RegistroUsuario VARCHAR(35) NULL,
        CONSTRAINT PK_CRD_DIG_TiposArchivos
            PRIMARY KEY NONCLUSTERED (IdTipoArchivo ASC)
    );
END;
GO

IF OBJECT_ID(N'dbo.CRD_DIG_TiposCreditos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CRD_DIG_TiposCreditos
    (
        IdTipoCredito SMALLINT NOT NULL,
        IdCodigoCatalogo CHAR(4) NOT NULL,
        Activo BIT NOT NULL,
        Nota VARCHAR(300) NULL,
        RegistroFecha DATETIME NULL,
        RegistroUsuario VARCHAR(35) NULL,
        CONSTRAINT PK_CRD_DIG_TiposCreditos
            PRIMARY KEY NONCLUSTERED (IdTipoCredito ASC)
    );
END;
GO

IF OBJECT_ID(N'dbo.CRD_DIG_TiposEstadosSolicitud', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CRD_DIG_TiposEstadosSolicitud
    (
        IdEstadoSolicitud SMALLINT NOT NULL,
        NombreEstadoSolicitud VARCHAR(100) NOT NULL,
        Descripcion VARCHAR(300) NULL,
        Orden SMALLINT NOT NULL,
        Activo BIT NOT NULL,
        Final BIT NOT NULL,
        RegistroFecha DATETIME NULL,
        RegistroUsuario VARCHAR(35) NULL,
        CONSTRAINT PK_CRD_DIG_TiposEstadosSolicitud
            PRIMARY KEY NONCLUSTERED (IdEstadoSolicitud ASC)
    );
END;
GO
