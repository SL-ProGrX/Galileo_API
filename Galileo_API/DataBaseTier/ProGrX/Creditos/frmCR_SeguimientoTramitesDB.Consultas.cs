using System.Data;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrSeguimientoTramitesDb
    {
        /// <summary>
        /// Inicializa los catálogos y parámetros globales utilizados por el seguimiento de trámites.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesInicializarData> Cr_SeguimientoTramites_Inicializar(
            int codEmpresa,
            string usuario)
        {
            string usuarioNormalizado = (usuario ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(usuarioNormalizado))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el usuario.",
                    -2,
                    new CrSeguimientoTramitesInicializarData());
            }

            var globalesResp = _mainDb.sbSifParametrosInicializa(codEmpresa, usuarioNormalizado);
            if (globalesResp.Code != 0 || globalesResp.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    globalesResp.Description ?? "No fue posible obtener los parámetros globales.",
                    globalesResp.Code.GetValueOrDefault(-1),
                    new CrSeguimientoTramitesInicializarData());
            }

            var catalogosResp = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                conn => Cr_SeguimientoTramites_Catalogos_Cargar(conn, usuarioNormalizado));

            if (catalogosResp.Code != 0 || catalogosResp.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    catalogosResp.Description ?? "No fue posible inicializar los catálogos.",
                    catalogosResp.Code.GetValueOrDefault(-1),
                    new CrSeguimientoTramitesInicializarData());
            }

            CrSeguimientoTramitesInicializarData result = catalogosResp.Result;
            result.fecha_servidor = globalesResp.Result.fxFechaServidor ?? DateTime.Today;
            result.oficina = globalesResp.Result.GOficina;
            result.oficina_titular = globalesResp.Result.GOficinaTitular;
            result.oficina_apoyo = globalesResp.Result.GOficinaApoyo;
            result.fecha_credito = globalesResp.Result.GlngFechaCR;
            result.sys_plan_pagos = globalesResp.Result.SysPlanPagos;
            result.sys_doc_version = globalesResp.Result.SysDocVersion;
            result.tipos_documento = Cr_SeguimientoTramites_TiposDocumento_Crear();

            return DbHelper.CreateOkResponse(result);
        }

        /// <summary>
        /// Busca solicitudes de crédito conservando los filtros y descripciones del formulario VB6.
        /// </summary>
        public ErrorDto<List<CrSeguimientoTramitesBusquedaItem>> Cr_SeguimientoTramites_Buscar(
            int codEmpresa,
            string? cedula,
            string? nombre)
        {
            const string sql = @"
                select
                    R.id_solicitud,
                    rtrim(R.codigo) as codigo,
                    rtrim(R.cedula) as cedula,
                    rtrim(S.nombre) as nombre,
                    R.fechasol,
                    R.montosol,
                    isnull(R.estadosol, '') as estadosol,
                    case R.estadosol
                        when 'R' then 'Recibida'
                        when 'P' then 'Pendiente'
                        when 'A' then 'Aprobada'
                        when 'D' then 'Denegada'
                        when 'F' then 'Formalizada'
                        when 'N' then 'Anulada'
                        else ''
                    end as estado_descripcion,
                    isnull(R.estado, '') as estado,
                    case R.estado
                        when 'A' then 'Activa'
                        when 'C' then 'Cancelada'
                        else 'En Trámite'
                    end as activo_descripcion,
                    isnull(R.proceso, '') as proceso,
                    case R.proceso
                        when 'J' then 'Cobro Jud'
                        when 'N' then 'Normal'
                        when 'T' then 'Traspaso'
                        else '------'
                    end as proceso_descripcion
                from REG_CREDITOS R
                inner join CATALOGO C on R.CODIGO = C.CODIGO
                inner join SOCIOS S on R.cedula = S.cedula
                where C.retencion = 'N'
                  and C.poliza = 'N'
                  and R.cedula like '%' + @Cedula + '%'
                  and S.nombre like '%' + @Nombre + '%'
                order by R.id_solicitud desc;";

            var parameters = new
            {
                Cedula = Cr_SeguimientoTramites_Filtro_Normalizar(cedula, 20),
                Nombre = Cr_SeguimientoTramites_Filtro_Normalizar(nombre, 150)
            };

            return DbHelper.ExecuteListQuery<CrSeguimientoTramitesBusquedaItem>(
                _portalDb,
                codEmpresa,
                sql,
                parameters);
        }

        /// <summary>
        /// Obtiene la operación y todos sus catálogos dependientes en una sola respuesta.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesOperacionData> Cr_SeguimientoTramites_Operacion_Obtener(
            int codEmpresa,
            int operacion)
        {
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operación.",
                    -2,
                    new CrSeguimientoTramitesOperacionData());
            }

            var response = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                conn => Cr_SeguimientoTramites_Operacion_Cargar(conn, operacion));

            if (response.Code != 0
                && string.Equals(
                    response.Description,
                    "No existe esta Solicitud",
                    StringComparison.Ordinal))
            {
                return DbHelper.CreateErrorResponse(
                    "No existe esta Solicitud",
                    -2,
                    new CrSeguimientoTramitesOperacionData());
            }

            return response;
        }

        /// <summary>
        /// Buscador paginado de operaciones, equivalente a sbBusqueda(0) del formulario VB6:
        /// id_solicitud, codigo, cedula, montoapr y saldo de reg_creditos.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesOperacionBusquedaLista>
            Cr_SeguimientoTramites_Operaciones_Buscar(
                int codEmpresa,
                FiltrosLazyLoadData? filtros)
        {
            var sortMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["id_solicitud"] = 1,
                ["codigo"] = 2,
                ["cedula"] = 3,
                ["montoapr"] = 4,
                ["saldo"] = 5
            };

            LazyLoadSpec lazy = LazyLoadHelper.Build(filtros, sortMap, "id_solicitud");

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                conn => Cr_SeguimientoTramites_Operaciones_Buscar_Cargar(conn, lazy));
        }

        private static CrSeguimientoTramitesOperacionBusquedaLista
            Cr_SeguimientoTramites_Operaciones_Buscar_Cargar(
                IDbConnection conn,
                LazyLoadSpec lazy)
        {
            const string filtroSql = """
                    @hasFilter = 0
                 OR convert(varchar(20), R.id_solicitud) like @filtro
                 OR R.codigo like @filtro
                 OR R.cedula like @filtro
                """;

            string sqlCount = $"""
                select count(1)
                from reg_creditos R
                where ({filtroSql});
                """;

            string sqlData = $"""
                with base as (
                    select
                        R.id_solicitud,
                        rtrim(isnull(R.codigo, '')) as codigo,
                        rtrim(isnull(R.cedula, '')) as cedula,
                        isnull(R.montoapr, 0) as montoapr,
                        isnull(R.saldo, 0) as saldo
                    from reg_creditos R
                    where ({filtroSql})
                )
                select id_solicitud, codigo, cedula, montoapr, saldo
                from base t
                order by
                    case when @sortCode = 1 and @isAsc = 1 then t.id_solicitud end asc,
                    case when @sortCode = 1 and @isAsc = 0 then t.id_solicitud end desc,
                    case when @sortCode = 2 and @isAsc = 1 then t.codigo end asc,
                    case when @sortCode = 2 and @isAsc = 0 then t.codigo end desc,
                    case when @sortCode = 3 and @isAsc = 1 then t.cedula end asc,
                    case when @sortCode = 3 and @isAsc = 0 then t.cedula end desc,
                    case when @sortCode = 4 and @isAsc = 1 then t.montoapr end asc,
                    case when @sortCode = 4 and @isAsc = 0 then t.montoapr end desc,
                    case when @sortCode = 5 and @isAsc = 1 then t.saldo end asc,
                    case when @sortCode = 5 and @isAsc = 0 then t.saldo end desc,
                    t.id_solicitud asc
                offset @offset rows fetch next @pageSize rows only;
                """;

            return new CrSeguimientoTramitesOperacionBusquedaLista
            {
                total = conn.ExecuteScalar<int>(sqlCount, lazy.Params),
                lista = conn.Query<CrSeguimientoTramitesOperacionBusquedaItem>(
                    sqlData,
                    lazy.Params).ToList()
            };
        }

        /// <summary>
        /// Obtiene la operación anterior o siguiente respecto de la actual, equivalente a
        /// FlatScrollBar_Change del formulario VB6. Devuelve 0 cuando no hay más.
        /// </summary>
        public ErrorDto<int> Cr_SeguimientoTramites_Operacion_Navegar(
            int codEmpresa,
            int operacion,
            string direccion)
        {
            bool siguiente = !string.Equals(
                (direccion ?? string.Empty).Trim(),
                "A",
                StringComparison.OrdinalIgnoreCase);

            int referencia = ObtenerReferencia(operacion, siguiente);

            string sql = siguiente
                ? """
                    select top 1 R.id_solicitud
                    from reg_creditos R
                    inner join Catalogo C on R.codigo = C.codigo
                    where C.Retencion = 'N' and C.poliza = 'N'
                      and R.id_solicitud > @Operacion
                    order by R.id_solicitud asc;
                    """
                : """
                    select top 1 R.id_solicitud
                    from reg_creditos R
                    inner join Catalogo C on R.codigo = C.codigo
                    where C.Retencion = 'N' and C.poliza = 'N'
                      and R.id_solicitud < @Operacion
                    order by R.id_solicitud desc;
                    """;

            var response = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                0,
                new { Operacion = referencia });

            return response.Code != 0
                ? DbHelper.CreateErrorResponse(
                    response.Description ?? "No fue posible navegar entre operaciones.",
                    response.Code.GetValueOrDefault(-1),
                    0)
                : DbHelper.CreateOkResponse(response.Result);
        }

        private static CrSeguimientoTramitesInicializarData Cr_SeguimientoTramites_Catalogos_Cargar(
            IDbConnection conn,
            string usuario)
        {
            const string sql = @"
                select convert(varchar(20), id_comite) as idx, rtrim(descripcion) as itmx
                from comites
                where estado = 1
                order by descripcion;

                select rtrim(COD_OFICINA) as idx, rtrim(descripcion) as itmx
                from SIF_OFICINAS
                where estado = 1
                order by descripcion;

                exec spCRDGarantiaFND;

                select rtrim(cod_actividad) as idx, rtrim(descripcion) as itmx
                from AFI_ACTIVIDADES_ECO
                where activa = 1
                order by descripcion;

                select rtrim(Canal_Tipo) as idx, rtrim(descripcion) as itmx
                from AFI_CANALES_TIPOS
                where Activo = 1
                order by descripcion;

                exec spCrd_SGT_Bancos @Usuario;";

            using SqlMapper.GridReader grid = conn.QueryMultiple(sql, new { Usuario = usuario });

            return new CrSeguimientoTramitesInicializarData
            {
                comites = Cr_SeguimientoTramites_Opciones_Mapear(grid.Read<CrSeguimientoTramitesOpcionRaw>()),
                oficinas = Cr_SeguimientoTramites_Opciones_Mapear(grid.Read<CrSeguimientoTramitesOpcionRaw>()),
                garantias_fondo = Cr_SeguimientoTramites_Opciones_Mapear(grid.Read<CrSeguimientoTramitesOpcionRaw>()),
                actividades = Cr_SeguimientoTramites_Opciones_Mapear(grid.Read<CrSeguimientoTramitesOpcionRaw>()),
                canales = Cr_SeguimientoTramites_Opciones_Mapear(grid.Read<CrSeguimientoTramitesOpcionRaw>()),
                bancos = Cr_SeguimientoTramites_Opciones_Mapear(grid.Read<CrSeguimientoTramitesOpcionRaw>())
            };
        }

        private static CrSeguimientoTramitesOperacionData Cr_SeguimientoTramites_Operacion_Cargar(
            IDbConnection conn,
            int operacion)
        {
            CrSeguimientoTramitesOperacionData? result = conn.QueryFirstOrDefault<CrSeguimientoTramitesOperacionData>(
                "spCrd_Operacion_Consulta",
                new { Operacion = operacion },
                commandType: CommandType.StoredProcedure);

            if (result is null)
            {
                throw new InvalidOperationException("No existe esta Solicitud");
            }

            Cr_SeguimientoTramites_Operacion_Complementar(conn, result);
            return result;
        }

        private static void Cr_SeguimientoTramites_Operacion_Complementar(
            IDbConnection conn,
            CrSeguimientoTramitesOperacionData operacion)
        {
            const string sql = @"
                select rtrim(D.cod_destino) as idx, rtrim(D.descripcion) as itmx
                from catalogo_destinos D
                inner join catalogo_destinosASG C on D.cod_destino = C.cod_destino
                where C.codigo = @Codigo
                order by D.prioridad asc;

                select rtrim(T.Garantia) as idx, rtrim(T.descripcion) as itmx
                from crd_catalogo_garantias C
                inner join crd_garantia_tipos T on C.garantia = T.garantia
                where C.codigo = @Codigo;

                select rtrim(G.cod_grupo) as idx, rtrim(G.descripcion) as itmx
                from catalogo_grupos G
                inner join catalogo_asignaGrp A on G.cod_grupo = A.cod_grupo
                where G.estado = 1 and A.codigo = @Codigo;

                exec spSys_Cuentas_Bancarias @Identificacion, @BancoId, 1;

                select convert(varchar(20), COD_DEDUCTORA) as idx, rtrim(DESCRIPCION) as itmx
                from vAFI_Deductoras
                where cod_institucion = @InstitucionId
                order by DESCRIPCION;

                select isnull(max(convert(int, envio_Tesoreria)), 0)
                from catalogo_destinos
                where cod_destino = @Destino;

                select rtrim(isnull(max(FORMULARIO), ''))
                from CRD_GARANTIA_TIPOS
                where Garantia = @Garantia;

                select rtrim(isnull(max(cod_preAnalisis), ''))
                from CRD_PREA_PREANALISIS
                where id_solicitud = @Operacion;";

            var parameters = new
            {
                Codigo = operacion.codigo,
                Cedula = operacion.cedula,
                Identificacion = operacion.cedula,
                BancoId = operacion.cod_banco,
                InstitucionId = operacion.cod_institucion,
                Destino = operacion.cod_destino,
                Garantia = operacion.garantia,
                Operacion = operacion.id_solicitud
            };

            using SqlMapper.GridReader grid = conn.QueryMultiple(sql, parameters);
            operacion.destinos = Cr_SeguimientoTramites_Opciones_Mapear(grid.Read<CrSeguimientoTramitesOpcionRaw>());
            operacion.garantias = Cr_SeguimientoTramites_Opciones_Mapear(grid.Read<CrSeguimientoTramitesOpcionRaw>());
            operacion.recursos = Cr_SeguimientoTramites_Opciones_Mapear(grid.Read<CrSeguimientoTramitesOpcionRaw>());
            operacion.cuentas_bancarias = Cr_SeguimientoTramites_Opciones_Mapear(grid.Read<CrSeguimientoTramitesOpcionRaw>());
            operacion.deductoras = Cr_SeguimientoTramites_Opciones_Mapear(grid.Read<CrSeguimientoTramitesOpcionRaw>());
            // fxEnvioTesoreria del VB6.
            operacion.envio_tesoreria = grid.ReadFirstOrDefault<int>() == 1;
            // fxGarantiaForm del VB6: F02 fiadores, F03 hipotecaria, F07 prendas.
            operacion.formulario_garantia = grid.ReadFirstOrDefault<string>() ?? string.Empty;
            // Expediente de preanálisis asociado, si la operación ya tiene estudio.
            operacion.cod_preanalisis = grid.ReadFirstOrDefault<string>() ?? string.Empty;
            operacion.estados = Cr_SeguimientoTramites_Estados_Crear(operacion.estadosol);
            operacion.estado_tooltip = Cr_SeguimientoTramites_EstadoTooltip_Crear(operacion);
            operacion.tasa_tooltip = $"Pts Bonificación: {operacion.tasa_pts_bono}";
            operacion.seccion_inicial = Cr_SeguimientoTramites_SeccionInicial_Obtener(
                operacion.estadosol,
                operacion.estado);
            operacion.permite_formalizar = !string.Equals(
                operacion.estadosol,
                "N",
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Obtiene la referencia de operación para navegación.
        /// El VB6 parte de 0 hacia adelante y del tope hacia atrás cuando no hay operación.
        /// </summary>
        private static int ObtenerReferencia(int operacion, bool siguiente)
        {
            return operacion > 0
                ? operacion
                : (siguiente ? 0 : int.MaxValue);
        }
    }
}
