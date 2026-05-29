using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAFTelemarketingConsultasDB
    {
        private readonly IConfiguration _config;

        private const string OpcionTodos = "T";
        private const string SpTelemarketingConsulta = "spAFI_Telemarketing_Consulta";
        private const string SpClientesComun = "spMKD_ClientesComun";
        private const string SpClientesComunDetalle = "spMKD_ClientesComun_Detalle";
        private const string SpTelemarketingContactos = "spAFI_Telemarketing_Contactos";

        private const string SqlCategorias = @"
                    SELECT COD_MORA AS item,
                           COD_MORA AS descripcion
                    FROM dbo.Cbr_Clasificacion_Mora;";

        private const string SqlActividades = @"
                    SELECT COD_ACTIVIDAD AS item,
                           DESCRIPCION AS descripcion
                    FROM dbo.AFI_ACTIVIDADES_ECO
                    ORDER BY DESCRIPCION;";

        private const string SqlCanales = @"
                    SELECT CANAL_TIPO AS item,
                           DESCRIPCION AS descripcion
                    FROM dbo.AFI_CANALES_TIPOS
                    ORDER BY DESCRIPCION;";

        private const string SqlDestinos = @"
                    SELECT COD_DESTINO AS item,
                           DESCRIPCION AS descripcion
                    FROM dbo.CATALOGO_DESTINOS
                    ORDER BY DESCRIPCION;";

        private const string SqlInstituciones = @"
                    SELECT COD_INSTITUCION AS item,
                           DESCRIPCION AS descripcion
                    FROM dbo.INSTITUCIONES
                    ORDER BY DESCRIPCION;";

        private const string SqlPreferencias = @"
                    SELECT COD_PREFERENCIA AS item,
                           DESCRIPCION AS descripcion
                    FROM dbo.AFI_PREFERENCIAS
                    ORDER BY DESCRIPCION;";

        private const string SqlLineasCatalogo = @"
                    SELECT CODIGO AS item,
                           DESCRIPCION AS descripcion
                    FROM dbo.CATALOGO
                    WHERE LINEA_INTERNA = 1
                      AND RETENCION = 'N'
                      AND POLIZA = 'N'
                    ORDER BY DESCRIPCION;";

        private const string SqlLineasCredito = @"
                    SELECT Codigo AS item,
                           Codigo + ' - ' + Descripcion AS descripcion
                    FROM dbo.Catalogo
                    WHERE Poliza = 'N'
                      AND Retencion = 'N';";

        private const string SqlLineasPolizaRetencion = @"
                    SELECT Codigo AS item,
                           Codigo + ' - ' + Descripcion AS descripcion
                    FROM dbo.Catalogo
                    WHERE Poliza = 'S'
                       OR Retencion = 'S';";

        private const string SqlPivotDelete = @"
                    DELETE FROM dbo.SYS_REPORT_PIVOT_01
                    WHERE usuario = @Usuario;";

        private const string SqlPivotInsert = @"
                    INSERT INTO dbo.SYS_REPORT_PIVOT_01
                    (
                        USUARIO,
                        CODIGO,
                        REGISTRO_FECHA,
                        COD_REPORTE
                    )
                    VALUES
                    (
                        @Usuario,
                        @Codigo,
                        GETDATE(),
                        @CodReporte
                    );";

        private const string SqlEstadosPersona = @"
                    SELECT COD_ESTADO AS item,
                           DESCRIPCION AS descripcion
                    FROM dbo.AFI_ESTADOS_PERSONA;";

        private static readonly IReadOnlyDictionary<string, string> CatalogosMap =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Actividad"] = SqlActividades,
                ["Canal"] = SqlCanales,
                ["Destino"] = SqlDestinos,
                ["Institucion"] = SqlInstituciones,
                ["Preferencias"] = SqlPreferencias,
                ["Linea"] = SqlLineasCatalogo
            };

        public FrmAFTelemarketingConsultasDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        #region Colocacion

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Telemarketing_Categoria_Obtener(int CodEmpresa)
        {
            var result = EjecutarListaDropDown(CodEmpresa, SqlCategorias);
            if (result.Code == 0)
            {
                InsertarOpcionTodos(result.Result);
            }

            return result;
        }

        public ErrorDto<List<AfTelemarketingColocacionData>> AF_Telemarketing_Colocacion_Obtener(
            int CodEmpresa,
            ColocacionFiltros filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros de colocación son requeridos.",
                    -2,
                    new List<AfTelemarketingColocacionData>());
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<AfTelemarketingColocacionData>(
                    SpTelemarketingConsulta,
                    CrearParametrosColocacion(filtros),
                    commandType: System.Data.CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<AfTelemarketingColocacionData>())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener colocaciones de telemarketing.",
                    result.Code.GetValueOrDefault(-1),
                    new List<AfTelemarketingColocacionData>());
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Telemarketing_Catalogos_Obtener(
            int CodEmpresa,
            string tipo)
        {
            if (!CatalogosMap.TryGetValue(NormalizarTexto(tipo), out var sql))
            {
                return DbHelper.CreateOkResponse(new List<DropDownListaGenericaModel>());
            }

            return EjecutarListaDropDown(CodEmpresa, sql);
        }

        #endregion

        #region Clientes

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Telemarketing_Lineas_Obtener(
            int CodEmpresa,
            int combo)
        {
            return EjecutarListaDropDown(
                CodEmpresa,
                combo == 1 ? SqlLineasCredito : SqlLineasPolizaRetencion);
        }

        public ErrorDto<List<AfTelemarketingClientesData>> AF_Telemarketing_Clientes_Obtener(
            int CodEmpresa,
            ClientesFiltros filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros de clientes son requeridos.",
                    -2,
                    new List<AfTelemarketingClientesData>());
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var usuario = NormalizarTexto(filtros.usuario);
                connection.Execute(SqlPivotDelete, new { Usuario = usuario });
                RegistrarPivot(connection, usuario, filtros.lineas, "MKD_Clc");
                RegistrarPivot(connection, usuario, filtros.codigos, "MKD_Cod");

                return connection.Query<AfTelemarketingClientesData>(
                    SpClientesComun,
                    new
                    {
                        Usuario = usuario,
                        ChkIntegral = filtros.chkAnalisis == true ? 1 : 0
                    },
                    commandType: System.Data.CommandType.StoredProcedure).ToList();
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<AfTelemarketingClientesData>())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener clientes de telemarketing.",
                    result.Code.GetValueOrDefault(-1),
                    new List<AfTelemarketingClientesData>());
        }

        public ErrorDto<List<AfTelemarketingClientesDetalleData>> AF_Telemarketing_ClientesDetalle_Obtener(
            int CodEmpresa,
            string vCadena,
            string usuario)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<AfTelemarketingClientesDetalleData>(
                    SpClientesComunDetalle,
                    new
                    {
                        Cadena = NormalizarTexto(vCadena),
                        Usuario = NormalizarTexto(usuario)
                    },
                    commandType: System.Data.CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<AfTelemarketingClientesDetalleData>())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener detalle de clientes.",
                    result.Code.GetValueOrDefault(-1),
                    new List<AfTelemarketingClientesDetalleData>());
        }

        #endregion

        #region Contactos

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Telemarketing_EstadosPer_Obtener(int CodEmpresa)
        {
            var result = EjecutarListaDropDown(CodEmpresa, SqlEstadosPersona);
            if (result.Code == 0)
            {
                InsertarOpcionTodos(result.Result);
            }

            return result;
        }

        public ErrorDto<List<AfTelemarketingContactoData>> AF_Telemarketing_Contacto_Obtener(
            int CodEmpresa,
            ContactosFiltros filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros de contacto son requeridos.",
                    -2,
                    new List<AfTelemarketingContactoData>());
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<AfTelemarketingContactoData>(
                    SpTelemarketingContactos,
                    CrearParametrosContactos(filtros),
                    commandType: System.Data.CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<AfTelemarketingContactoData>())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener contactos de telemarketing.",
                    result.Code.GetValueOrDefault(-1),
                    new List<AfTelemarketingContactoData>());
        }

        #endregion

        private ErrorDto<List<DropDownListaGenericaModel>> EjecutarListaDropDown(int codEmpresa, string sql)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                codEmpresa,
                sql);
        }

        private static void InsertarOpcionTodos(List<DropDownListaGenericaModel>? lista)
        {
            lista?.Insert(0, new DropDownListaGenericaModel
            {
                item = OpcionTodos,
                descripcion = "TODOS"
            });
        }

        private static object CrearParametrosColocacion(ColocacionFiltros filtros)
        {
            var validaciones = ObtenerValidaciones(filtros.validaciones);

            return new
            {
                Tipo = ObtenerTipoColocacion(filtros.fechaTipo),
                FechaInicio = filtros.chkFechas ? (DateTime?)null : filtros.fechaInicio.Date,
                FechaCorte = filtros.chkFechas
                    ? (DateTime?)null
                    : filtros.fechaCorte.Date.AddDays(1).AddSeconds(-1),
                Credito = NormalizarNull(filtros.credito),
                Destino = NormalizarNull(filtros.destino),
                Producto = NormalizarNull(filtros.producto),
                Canal = NormalizarNull(filtros.canal),
                Institucion = NormalizarNull(filtros.institucion),
                ChkSinMora = validaciones.ChkSinMora,
                ChkEmail = validaciones.ChkEmail,
                ChkMovil = validaciones.ChkMovil,
                filtros.mFecUltMovUpdate,
                Categoria = string.Equals(
                    NormalizarTexto(filtros.categoria),
                    OpcionTodos,
                    StringComparison.OrdinalIgnoreCase)
                    ? null
                    : NormalizarNull(filtros.categoria),
                Gyp = NormalizarNull(filtros.gyp)
            };
        }

        private static object CrearParametrosContactos(ContactosFiltros filtros)
        {
            var fechaTipo = NormalizarTexto(filtros.fechaTipo);
            var todos = string.Equals(fechaTipo, OpcionTodos, StringComparison.OrdinalIgnoreCase);

            return new
            {
                FechaInicio = todos || filtros.fechaInicio is null
                    ? (DateTime?)null
                    : filtros.fechaInicio.Value.Date,
                FechaCorte = todos || filtros.fechaCorte is null
                    ? (DateTime?)null
                    : filtros.fechaCorte.Value.Date.AddDays(1).AddSeconds(-1),
                FechaTipo = todos ? OpcionTodos : fechaTipo,
                Estado = string.Equals(
                    NormalizarTexto(filtros.estado),
                    OpcionTodos,
                    StringComparison.OrdinalIgnoreCase)
                    ? null
                    : NormalizarNull(filtros.estado)
            };
        }

        private static void RegistrarPivot(
            SqlConnection connection,
            string usuario,
            IEnumerable<dynamic>? items,
            string codReporte)
        {
            foreach (var item in items ?? Enumerable.Empty<dynamic>())
            {
                var codigo = NormalizarTexto(item.item?.ToString());
                if (string.IsNullOrWhiteSpace(codigo))
                {
                    continue;
                }

                connection.Execute(SqlPivotInsert, new
                {
                    Usuario = usuario,
                    Codigo = codigo,
                    CodReporte = codReporte
                });
            }
        }

        private static (int ChkSinMora, int ChkEmail, int ChkMovil) ObtenerValidaciones(
            IEnumerable<dynamic>? validaciones)
        {
            var chkSinMora = 0;
            var chkEmail = 0;
            var chkMovil = 0;

            foreach (var item in validaciones ?? Enumerable.Empty<dynamic>())
            {
                var codigo = NormalizarTexto(item.item?.ToString());

                if (string.Equals(codigo, "M", StringComparison.Ordinal))
                {
                    chkSinMora = 1;
                }
                else if (string.Equals(codigo, "E", StringComparison.Ordinal))
                {
                    chkEmail = 1;
                }
                else if (string.Equals(codigo, "T", StringComparison.Ordinal))
                {
                    chkMovil = 1;
                }
            }

            return (chkSinMora, chkEmail, chkMovil);
        }

        private static string ObtenerTipoColocacion(int fechaTipo)
        {
            return fechaTipo switch
            {
                1 => "CRD_01",
                2 => "CRD_02",
                3 => "CRD_03",
                _ => "CRD_01"
            };
        }

        private static string? NormalizarNull(string? valor)
        {
            var texto = NormalizarTexto(valor);
            return string.IsNullOrWhiteSpace(texto) ? null : texto;
        }

        private PortalDB CreatePortalDb() => new(_config);

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}