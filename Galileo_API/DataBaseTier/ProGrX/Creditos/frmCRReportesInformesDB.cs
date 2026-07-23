using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCRReportesInformesDB
    {
        private readonly PortalDB _portalDB;
        private const string Interesv = "Interesv";
        private const string TODOS = "TODOS";
        private const string TODAS = "TODAS";
        private const string Plazo = "Plazo";
        
        public FrmCRReportesInformesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        #region Helpers

        private static string NormalizeFiltro(string? filtro)
        {
            return (filtro ?? string.Empty).Trim();
        }

        private static object BuildFiltroParams(string? filtro)
        {
            var texto = NormalizeFiltro(filtro);
            return new { filtro = texto, like = $"%{texto}%" };
        }

        private ErrorDto<List<DropDownListaGenericaModel>> EjecutarDropdown(
            int CodEmpresa,
            string? filtro,
            string sql)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var result = conn
                    .Query<DropDownListaGenericaModel>(sql, BuildFiltroParams(filtro))
                    .ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    ex.Message,
                    -1,
                    new List<DropDownListaGenericaModel>());
            }
        }

        private static List<DropDownListaGenericaModel> CrearDropdown(params (string item, string descripcion)[] items)
        {
            return items
                .Select(x => new DropDownListaGenericaModel
                {
                    item = x.item,
                    descripcion = x.descripcion
                })
                .ToList();
        }

        #endregion

        #region Informes - Panel Base

        /// <summary>
        /// Obtiene los tipos de reporte principales.
        /// </summary>
        /// <returns></returns>
        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_TipoReporte_Dropdown_Obtener()
        {
            var result = CrearDropdown(
                ("D", "Detalle"),
                ("R", "Resumen"));

            return DbHelper.CreateOkResponse(result);
        }

        /// <summary>
        /// Obtiene las bases de fecha disponibles para filtrar reportes.
        /// </summary>
        /// <returns></returns>
        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_BaseFecha_Dropdown_Obtener()
        {
            var result = CrearDropdown(
                ("S", "Solicitud"),
                ("R", "Resolución"),
                ("F", "Formalización"),
                ("D", "Desembolso"),
                ("U", "Último Mov."));

            return DbHelper.CreateOkResponse(result);
        }

        /// <summary>
        /// Obtiene los estados de solicitud.
        /// </summary>
        /// <returns></returns>
        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_EstadoSolicitud_Dropdown_Obtener()
        {
            var result = CrearDropdown(
                ("R", "Recibida"),
                ("P", "Pendiente"),
                ("F", "Formalizada"),
                ("N", "Nula"),
                ("A", "Aprobada"),
                ("D", "Denegada"),
                ("T", "Todas"));

            return DbHelper.CreateOkResponse(result);
        }

        /// <summary>
        /// Obtiene los estados de operación.
        /// </summary>
        /// <returns></returns>
        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_EstadoOperacion_Dropdown_Obtener()
        {
            var result = CrearDropdown(
                ("A", "Activa"),
                ("C", "Cancelada"),
                ("N", "Nulas"),
                ("T", "Todas (Activas/Canceladas)"));

            return DbHelper.CreateOkResponse(result);
        }

        /// <summary>
        /// Obtiene las oficinas para filtro de reportes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Oficinas_Dropdown_Obtener(int CodEmpresa,string? filtro)
        {
            const string sql = @"
                SELECT
                    RTRIM(cod_oficina) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM SIF_Oficinas
                WHERE @filtro = ''
                   OR cod_oficina LIKE @like
                   OR descripcion LIKE @like
                ORDER BY descripcion;";

            return EjecutarDropdown(CodEmpresa, filtro, sql);
        }

        /// <summary>
        /// Obtiene los estados de persona para filtro de reportes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_EstadoPersona_Dropdown_Obtener(int CodEmpresa,string? filtro)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                const string sql = @"
                    SELECT
                        RTRIM(cod_estado) AS item,
                        RTRIM(descripcion) AS descripcion
                    FROM afi_estados_persona
                    WHERE @filtro = ''
                       OR cod_estado LIKE @like
                       OR descripcion LIKE @like
                    ORDER BY descripcion;";

                var result = conn
                    .Query<DropDownListaGenericaModel>(sql, BuildFiltroParams(filtro))
                    .ToList();

                result.Insert(0, new DropDownListaGenericaModel
                {
                    item = TODOS,
                    descripcion = TODOS
                });

                result.Add(new DropDownListaGenericaModel
                {
                    item = "X",
                    descripcion = "Ex.Socios"
                });

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    ex.Message,
                    -1,
                    new List<DropDownListaGenericaModel>());
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    ex.Message,
                    -1,
                    new List<DropDownListaGenericaModel>());
            }
        }

        #endregion

        #region Informes - General

        /// <summary>
        /// Obtiene las divisas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Divisas_Dropdown_Obtener(
            int CodEmpresa,
            string? filtro)
        {
            const string sql = @"
                SELECT
                    RTRIM(COD_DIVISA) AS item,
                    RTRIM(DESCRIPCION) AS descripcion
                FROM vSys_Divisas
                WHERE @filtro = ''
                   OR COD_DIVISA LIKE @like
                   OR DESCRIPCION LIKE @like
                ORDER BY DESCRIPCION;";

            return EjecutarDropdown(CodEmpresa, filtro, sql);
        }

        /// <summary>
        /// Obtiene los tipos de garantía.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Garantias_Dropdown_Obtener(
            int CodEmpresa,
            string? filtro)
        {
            const string sql = @"
                SELECT
                    RTRIM(Garantia) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM crd_garantia_tipos
                WHERE @filtro = ''
                   OR Garantia LIKE @like
                   OR descripcion LIKE @like
                ORDER BY descripcion;";

            return EjecutarDropdown(CodEmpresa, filtro, sql);
        }

        /// <summary>
        /// Obtiene los comités.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Comites_Dropdown_Obtener(
            int CodEmpresa,
            string? filtro)
        {
            const string sql = @"
                SELECT
                    CAST(id_comite AS varchar(20)) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM comites
                WHERE @filtro = ''
                   OR CAST(id_comite AS varchar(20)) LIKE @like
                   OR descripcion LIKE @like
                ORDER BY descripcion;";

            return EjecutarDropdown(CodEmpresa, filtro, sql);
        }

        /// <summary>
        /// Obtiene los grupos de usuario de crédito.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_GruposUsuario_Dropdown_Obtener(
            int CodEmpresa,
            string? filtro)
        {
            const string sql = @"
                SELECT
                    RTRIM(cod_grupo) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM crd_grupos
                WHERE @filtro = ''
                   OR cod_grupo LIKE @like
                   OR descripcion LIKE @like
                ORDER BY descripcion;";

            return EjecutarDropdown(CodEmpresa, filtro, sql);
        }

        /// <summary>
        /// Obtiene los recursos según línea o todas las líneas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <param name="todasLineas"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Recursos_Dropdown_Obtener(
            int CodEmpresa,
            string? codigo,
            bool todasLineas,
            string? filtro)
        {
            const string sqlTodas = @"
                SELECT
                    RTRIM(cod_grupo) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM catalogo_grupos
                WHERE @filtro = ''
                   OR cod_grupo LIKE @like
                   OR descripcion LIKE @like
                ORDER BY descripcion;";

            const string sqlLinea = @"
                SELECT
                    RTRIM(R.cod_grupo) AS item,
                    RTRIM(R.descripcion) AS descripcion
                FROM catalogo_grupos R
                INNER JOIN catalogo_AsignaGrp A
                        ON R.cod_grupo = A.cod_grupo
                       AND A.codigo = @codigo
                WHERE @filtro = ''
                   OR R.cod_grupo LIKE @like
                   OR R.descripcion LIKE @like
                ORDER BY R.descripcion;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var texto = NormalizeFiltro(filtro);
                var result = conn.Query<DropDownListaGenericaModel>(
                    todasLineas ? sqlTodas : sqlLinea,
                    new { filtro = texto, like = $"%{texto}%", codigo = (codigo ?? string.Empty).Trim() })
                    .ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    ex.Message,
                    -1,
                    new List<DropDownListaGenericaModel>());
            }
        }

        /// <summary>
        /// Obtiene los destinos según línea o todas las líneas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <param name="todasLineas"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Destinos_Dropdown_Obtener(
            int CodEmpresa,
            string? codigo,
            bool todasLineas,
            string? filtro)
        {
            const string sqlTodas = @"
                SELECT
                    RTRIM(cod_destino) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM catalogo_destinos
                WHERE @filtro = ''
                   OR cod_destino LIKE @like
                   OR descripcion LIKE @like
                ORDER BY descripcion;";

            const string sqlLinea = @"
                SELECT
                    RTRIM(R.cod_destino) AS item,
                    RTRIM(R.descripcion) AS descripcion
                FROM catalogo_destinos R
                INNER JOIN catalogo_destinosAsg A
                        ON R.cod_destino = A.cod_destino
                       AND A.codigo = @codigo
                WHERE @filtro = ''
                   OR R.cod_destino LIKE @like
                   OR R.descripcion LIKE @like
                ORDER BY R.descripcion;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var texto = NormalizeFiltro(filtro);
                var result = conn.Query<DropDownListaGenericaModel>(
                    todasLineas ? sqlTodas : sqlLinea,
                    new { filtro = texto, like = $"%{texto}%", codigo = (codigo ?? string.Empty).Trim() })
                    .ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    ex.Message,
                    -1,
                    new List<DropDownListaGenericaModel>());
            }
        }

        /// <summary>
        /// Obtiene las instituciones.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Instituciones_Dropdown_Obtener(
            int CodEmpresa,
            string? filtro)
        {
            const string sql = @"
                SELECT
                    CAST(cod_institucion AS varchar(20)) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM instituciones
                WHERE @filtro = ''
                   OR CAST(cod_institucion AS varchar(20)) LIKE @like
                   OR descripcion LIKE @like
                ORDER BY descripcion;";

            return EjecutarDropdown(CodEmpresa, filtro, sql);
        }

        /// <summary>
        /// Obtiene las deductoras según institución.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codInstitucion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Deductoras_Dropdown_Obtener(
            int CodEmpresa,
            int codInstitucion,
            string? filtro)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var texto = NormalizeFiltro(filtro);
                var like = $"%{texto}%";

                const string sqlTodas = @"
                    SELECT
                        CAST(cod_institucion AS varchar(20)) AS item,
                        RTRIM(descripcion) AS descripcion
                    FROM instituciones
                    WHERE @filtro = ''
                       OR CAST(cod_institucion AS varchar(20)) LIKE @like
                       OR descripcion LIKE @like
                    ORDER BY descripcion;";

                const string sqlVinculadas = @"
                    EXEC spAFI_Institucion_Vinculadas @codInstitucion, 3;";

                var result = codInstitucion <= 0
                    ? conn.Query<DropDownListaGenericaModel>(sqlTodas, new { filtro = texto, like }).ToList()
                    : conn.Query<DropDownListaGenericaModel>(sqlVinculadas, new { codInstitucion }).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    ex.Message,
                    -1,
                    new List<DropDownListaGenericaModel>());
            }
        }

        /// <summary>
        /// Obtiene el filtro especial de cartera.
        /// </summary>
        /// <returns></returns>
        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Especial_Dropdown_Obtener()
        {
            var result = CrearDropdown(
                (TODOS, TODOS),
                ("INTERNA", "Cartera Interna"),
                ("ADMINISTRADA", "Cartera Administrada"));

            return DbHelper.CreateOkResponse(result);
        }

        #endregion

        #region Informes - Adicionales

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Cobro_Dropdown_Obtener()
        {
            return DbHelper.CreateOkResponse(CrearDropdown(
                ("C", "Cajas"),
                ("P", "Planilla"),
                ("T", TODOS)));
        }

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Proceso_Dropdown_Obtener()
        {
            return DbHelper.CreateOkResponse(CrearDropdown(
                ("N", "Normal"),
                ("T", "Traspaso Deuda"),
                ("J", "Cobro Judicial"),
                (TODOS, TODOS)));
        }

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_TipoOperacion_Dropdown_Obtener()
        {
            return DbHelper.CreateOkResponse(CrearDropdown(
                (TODAS, TODAS),
                ("O", "Originales"),
                ("D", "Derivadas")));
        }

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_TiposTasas_Dropdown_Obtener()
        {
            return DbHelper.CreateOkResponse(CrearDropdown(
                (TODAS, TODAS),
                ("R", "Revisables"),
                ("I", "Indizadas")));
        }

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Autorizaciones_Dropdown_Obtener()
        {
            return DbHelper.CreateOkResponse(CrearDropdown(
                ("A", "Autorizadas"),
                ("N", "Normales"),
                (TODAS, TODAS)));
        }

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Signos_Dropdown_Obtener()
        {
            return DbHelper.CreateOkResponse(CrearDropdown(
                (">", ">"),
                ("<", "<"),
                ("=", "=")));
        }

        /// <summary>
        /// Obtiene gestores externos de cobro.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Gestiona_Dropdown_Obtener(int CodEmpresa,string? filtro)
        {
            const string sql = @"
                SELECT
                    RTRIM(Usuario) AS item,
                    RTRIM(Usuario) AS descripcion
                FROM CBR_USUARIOS
                WHERE OPERADOR_EXTERNO = 1
                  AND (
                        @filtro = ''
                     OR Usuario LIKE @like
                  )
                ORDER BY Usuario;";

            return EjecutarDropdown(CodEmpresa, filtro, sql);
        }

        #endregion

        #region Informes - F1

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Provincias_Dropdown_Obtener(int CodEmpresa,string? filtro)
        {
            const string sql = @"
                SELECT
                    CAST(Provincia AS varchar(20)) AS item,
                    RTRIM(Descripcion) AS descripcion
                FROM Provincias
                WHERE @filtro = ''
                   OR CAST(Provincia AS varchar(20)) LIKE @like
                   OR Descripcion LIKE @like
                ORDER BY Descripcion;";

            return EjecutarDropdown(CodEmpresa, filtro, sql);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Cantones_Dropdown_Obtener(int CodEmpresa,string provincia, string? filtro)
        {
            const string sql = @"
                SELECT
                    CAST(Canton AS varchar(20)) AS item,
                    RTRIM(Descripcion) AS descripcion
                FROM Cantones
                WHERE provincia = @provincia
                  AND (
                        @filtro = ''
                     OR CAST(Canton AS varchar(20)) LIKE @like
                     OR Descripcion LIKE @like
                  )
                ORDER BY Descripcion;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var texto = NormalizeFiltro(filtro);

                var result = conn.Query<DropDownListaGenericaModel>(
                    sql,
                    new { provincia = provincia.Trim(), filtro = texto, like = $"%{texto}%" })
                    .ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    ex.Message,
                    -1,
                    new List<DropDownListaGenericaModel>());
            }
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Distritos_Dropdown_Obtener(int CodEmpresa,string provincia,string canton,string? filtro)
        {
            const string sql = @"
                SELECT
                    CAST(Distrito AS varchar(20)) AS item,
                    RTRIM(Descripcion) AS descripcion
                FROM Distritos
                WHERE provincia = @provincia
                  AND canton = @canton
                  AND (
                        @filtro = ''
                     OR CAST(Distrito AS varchar(20)) LIKE @like
                     OR Descripcion LIKE @like
                  )
                ORDER BY Descripcion;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var texto = NormalizeFiltro(filtro);

                var result = conn.Query<DropDownListaGenericaModel>(
                    sql,
                    new
                    {
                        provincia = provincia.Trim(),
                        canton = canton.Trim(),
                        filtro = texto,
                        like = $"%{texto}%"
                    }).ToList();

                result.Add(new DropDownListaGenericaModel { item = string.Empty, descripcion = " " });

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    ex.Message,
                    -1,
                    new List<DropDownListaGenericaModel>());
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    ex.Message,
                    -1,
                    new List<DropDownListaGenericaModel>());
            }
            catch (ArgumentException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    ex.Message,
                    -1,
                    new List<DropDownListaGenericaModel>());
            }
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Zonas_Dropdown_Obtener(int CodEmpresa,string? filtro)
        {
            const string sql = @"
                SELECT
                    RTRIM(COD_ZONA) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM AFI_ZONAS
                WHERE @filtro = ''
                   OR COD_ZONA LIKE @like
                   OR descripcion LIKE @like
                ORDER BY descripcion;";

            return EjecutarDropdown(CodEmpresa, filtro, sql);
        }

        #endregion

        #region Informes - F2

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Profesiones_Dropdown_Obtener( int CodEmpresa,string? filtro)
        {
            const string sql = @"
                SELECT
                    CAST(COD_PROFESION AS varchar(20)) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM AFI_PROFESIONES
                WHERE @filtro = ''
                   OR CAST(COD_PROFESION AS varchar(20)) LIKE @like
                   OR descripcion LIKE @like
                ORDER BY descripcion;";

            return EjecutarDropdown(CodEmpresa, filtro, sql);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Sectores_Dropdown_Obtener(int CodEmpresa,string? filtro)
        {
            const string sql = @"
                SELECT
                    CAST(COD_SECTOR AS varchar(20)) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM AFI_SECTORES
                WHERE @filtro = ''
                   OR CAST(COD_SECTOR AS varchar(20)) LIKE @like
                   OR descripcion LIKE @like
                ORDER BY descripcion;";

            return EjecutarDropdown(CodEmpresa, filtro, sql);
        }

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Sexo_Dropdown_Obtener()
        {
            return DbHelper.CreateOkResponse(CrearDropdown(
                (TODOS, TODOS),
                ("F", "Femenino"),
                ("M", "Masculino")));
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_EstadoCivil_Dropdown_Obtener(int CodEmpresa,string? filtro)
        {
            const string sql = @"
                SELECT
                    RTRIM(Estado_Civil) AS item,
                    RTRIM(Descripcion) AS descripcion
                FROM SYS_ESTADO_CIVIL
                WHERE Activo = 1
                  AND (
                        @filtro = ''
                     OR Estado_Civil LIKE @like
                     OR Descripcion LIKE @like
                  )
                ORDER BY Descripcion;";

            return EjecutarDropdown(CodEmpresa, filtro, sql);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_CondicionLaboral_Dropdown_Obtener(
            int CodEmpresa,
            string? filtro)
        {
            const string sql = @"
                SELECT
                    RTRIM(Estado_Laboral) AS item,
                    RTRIM(Descripcion) AS descripcion
                FROM AFI_ESTADO_LABORAL
                WHERE Activo = 1
                  AND (
                        @filtro = ''
                     OR Estado_Laboral LIKE @like
                     OR Descripcion LIKE @like
                  )
                ORDER BY Descripcion;";

            return EjecutarDropdown(CodEmpresa, filtro, sql);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Ejecutivos_Dropdown_Obtener(
            int CodEmpresa,
            string? filtro)
        {
            const string sql = @"
                SELECT
                    CAST(id_promotor AS varchar(20)) AS item,
                    RTRIM(nombre) AS descripcion
                FROM promotores
                WHERE @filtro = ''
                   OR CAST(id_promotor AS varchar(20)) LIKE @like
                   OR nombre LIKE @like
                ORDER BY nombre;";

            return EjecutarDropdown(CodEmpresa, filtro, sql);
        }

        #endregion

        #region Informes - Adicional Dinámico

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_CausasTipos_Dropdown_Obtener()
        {
            return DbHelper.CreateOkResponse(CrearDropdown(
                ("P", "Pendientes"),
                ("D", "Denegadas")));
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Causas_Dropdown_Obtener(
            int CodEmpresa,
            string tipo,
            string? filtro)
        {
            const string sql = @"
                SELECT
                    RTRIM(cod_causas) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM operacion_causas
                WHERE tipo = @tipo
                  AND (
                        @filtro = ''
                     OR cod_causas LIKE @like
                     OR descripcion LIKE @like
                  )
                ORDER BY descripcion;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var texto = NormalizeFiltro(filtro);

                var result = conn.Query<DropDownListaGenericaModel>(
                    sql,
                    new { tipo = tipo.Trim(), filtro = texto, like = $"%{texto}%" })
                    .ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    ex.Message,
                    -1,
                    new List<DropDownListaGenericaModel>());
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    ex.Message,
                    -1,
                    new List<DropDownListaGenericaModel>());
            }
        }

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_RequisitoMarca_Dropdown_Obtener()
        {
            return DbHelper.CreateOkResponse(CrearDropdown(
                ("1", "Cumple"),
                ("2", "No Cumple"),
                ("0", "En Blanco")));
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Requisitos_Dropdown_Obtener(int CodEmpresa,string? codigo,bool todasLineas,string? filtro)
        {
            const string sqlTodas = @"
                SELECT
                    RTRIM(cod_requisito) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM requisitos_adicionales
                WHERE @filtro = ''
                   OR cod_requisito LIKE @like
                   OR descripcion LIKE @like
                ORDER BY descripcion;";

            const string sqlLinea = @"
                SELECT
                    RTRIM(R.cod_requisito) AS item,
                    RTRIM(R.descripcion) AS descripcion
                FROM requisitos_adicionales R
                INNER JOIN requisitos_asignacion A
                        ON R.cod_requisito = A.cod_requisito
                       AND A.Codigo = @codigo
                WHERE @filtro = ''
                   OR R.cod_requisito LIKE @like
                   OR R.descripcion LIKE @like
                ORDER BY R.descripcion;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var texto = NormalizeFiltro(filtro);

                var result = conn.Query<DropDownListaGenericaModel>(
                    todasLineas ? sqlTodas : sqlLinea,
                    new { filtro = texto, like = $"%{texto}%", codigo = (codigo ?? string.Empty).Trim() })
                    .ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    ex.Message,
                    -1,
                    new List<DropDownListaGenericaModel>());
            }
        }

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Cortes_Dropdown_Obtener()
        {
            return DbHelper.CreateOkResponse(CrearDropdown(
                ("Diario", "Diario"),
                ("Semanal", "Semanal"),
                ("Mensual", "Mensual"),
                ("Trimestral", "Trimestral"),
                ("Semestral", "Semestral"),
                ("Anual", "Anual")));
        }

        #endregion

        #region Informes - F4

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Lineas_Buscar(
            int CodEmpresa,
            string? filtro)
        {
            const string sql = @"
                SELECT
                    RTRIM(codigo) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM catalogo
                WHERE @filtro = ''
                   OR codigo LIKE @like
                   OR descripcion LIKE @like
                ORDER BY descripcion;";

            return EjecutarDropdown(CodEmpresa, filtro, sql);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_UnidadProgramatica_Buscar(
            int CodEmpresa,
            int codInstitucion,
            string? filtro)
        {
            const string sqlSif = @"
                SELECT
                    RTRIM(cod_departamento) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM afDepartamentos
                WHERE cod_institucion = @codInstitucion
                  AND (
                        @filtro = ''
                     OR cod_departamento LIKE @like
                     OR descripcion LIKE @like
                  )
                ORDER BY cod_departamento;";

            const string sqlUp = @"
                SELECT
                    RTRIM(codigo) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM uprogramatica
                WHERE @filtro = ''
                   OR codigo LIKE @like
                   OR descripcion LIKE @like
                ORDER BY codigo;";

            return EjecutarUnidadBusqueda(CodEmpresa, codInstitucion, filtro, sqlSif, sqlUp);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_UnidadTrabajo_Buscar(
            int CodEmpresa,
            int codInstitucion,
            string? codDepartamento,
            string? filtro)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var texto = NormalizeFiltro(filtro);
                var usarSif = EsModoSif(conn);

                const string sqlSif = @"
                    SELECT
                        RTRIM(cod_seccion) AS item,
                        RTRIM(descripcion) AS descripcion
                    FROM afSecciones
                    WHERE cod_institucion = @codInstitucion
                      AND cod_departamento = @codDepartamento
                      AND (
                            @filtro = ''
                         OR cod_seccion LIKE @like
                         OR descripcion LIKE @like
                      )
                    ORDER BY cod_seccion;";

                const string sqlUt = @"
                    SELECT
                        RTRIM(ut_codigo) AS item,
                        RTRIM(ut_descripcion) AS descripcion
                    FROM UTRABAJO
                    WHERE @filtro = ''
                       OR ut_codigo LIKE @like
                       OR ut_descripcion LIKE @like
                    ORDER BY ut_codigo;";

                var result = conn.Query<DropDownListaGenericaModel>(
                    usarSif ? sqlSif : sqlUt,
                    new
                    {
                        codInstitucion,
                        codDepartamento = NormalizeFiltro(codDepartamento),
                        filtro = texto,
                        like = $"%{texto}%"
                    }).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    ex.Message,
                    -1,
                    new List<DropDownListaGenericaModel>());
            }
        }

        private ErrorDto<List<DropDownListaGenericaModel>> EjecutarUnidadBusqueda(
            int CodEmpresa,
            int codInstitucion,
            string? filtro,
            string sqlSif,
            string sqlUp)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var texto = NormalizeFiltro(filtro);
                var usarSif = EsModoSif(conn);

                var result = conn.Query<DropDownListaGenericaModel>(
                    usarSif ? sqlSif : sqlUp,
                    new { codInstitucion, filtro = texto, like = $"%{texto}%" })
                    .ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    ex.Message,
                    -1,
                    new List<DropDownListaGenericaModel>());
            }
        }

        private static bool EsModoSif(SqlConnection conn)
        {
            const string sql = "SELECT OBJECT_ID('UPROGRAMATICA') AS Resultado;";
            var resultado = conn.QueryFirstOrDefault<int?>(sql);

            return resultado == null;
        }

        #endregion

        #region Informes - Árbol

        /// <summary>
        /// Obtiene el árbol de reportes. Los padres son fijos y los reportes vienen de CRD_REPORTES.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrReportesInformesArbolDto>> CR_Reportes_Informes_Arbol_Obtener(int CodEmpresa)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var result = BuildArbolBase();

                const string sql = @"
                    SELECT
                        id,
                        RTRIM(tipo) AS tipo,
                        RTRIM(reporte) AS reporte,
                        RTRIM(prefijo) AS prefijo,
                        ISNULL(adicional, 0) AS adicional,
                        ISNULL(seguridad, 0) AS seguridad
                    FROM CRD_REPORTES
                    ORDER BY reporte;";

                var reportes = conn.Query<CrReportesInformesArbolDto>(sql).ToList();

                foreach (var reporte in reportes)
                {
                    reporte.categoria = reporte.tipo.Trim().ToUpperInvariant();
                    result.Add(reporte);
                }

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrReportesInformesArbolDto>>(
                    ex.Message,
                    -1,
                    new List<CrReportesInformesArbolDto>());
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrReportesInformesArbolDto>>(
                    ex.Message,
                    -1,
                    new List<CrReportesInformesArbolDto>());
            }
        }

        /// <summary>
        /// Obtiene la definición del reporte seleccionado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="idReporte"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CrReportesInformesArbolDto> CR_Reportes_Informes_Reporte_Obtener(int CodEmpresa,int idReporte,string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    SELECT
                        id,
                        RTRIM(tipo) AS tipo,
                        RTRIM(reporte) AS reporte,
                        RTRIM(prefijo) AS prefijo,
                        ISNULL(adicional, 0) AS adicional,
                        ISNULL(seguridad, 0) AS seguridad,
                        RTRIM(tipo) AS categoria
                    FROM CRD_REPORTES
                    WHERE id = @idReporte;";

                var result = conn.QueryFirstOrDefault<CrReportesInformesArbolDto>(
                    sql,
                    new { idReporte });

                if (result == null)
                {
                    return DbHelper.CreateErrorResponse(
                        "No se encontró el reporte seleccionado.",
                        -2,
                        new CrReportesInformesArbolDto());
                }

                result.tiene_acceso = TieneAccesoReporte(
                    conn,
                    result.id,
                    result.seguridad,
                    usuario);

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CrReportesInformesArbolDto());
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CrReportesInformesArbolDto());
            }
        }

        private static bool TieneAccesoReporte(SqlConnection conn,int idReporte,int seguridad,string usuario)
        {
            if (seguridad != 1)
            {
                return true;
            }

            const string sql = @"
                SELECT COUNT(1)
                FROM CRD_REPORTES_GRP_AUT
                WHERE id = @idReporte
                  AND cod_grupo IN (
                      SELECT cod_grupo
                      FROM CRD_REPORTES_GRP_USR
                      WHERE usuario = @usuario
                  );";

            return conn.ExecuteScalar<int>(
                sql,
                new
                {
                    idReporte,
                    usuario = NormalizeFiltro(usuario)
                }) > 0;
        }
        private static List<CrReportesInformesArbolDto> BuildArbolBase()
        {
            return new List<CrReportesInformesArbolDto>
            {
                new() { id = 0, tipo = "ROOT", reporte = "Reportes", prefijo = string.Empty, adicional = 0, seguridad = 0, categoria = string.Empty },
                new() { id = 0, tipo = "CRD", reporte = "Créditos", prefijo = string.Empty, adicional = 0, seguridad = 0, categoria = "ROOT" },
                new() { id = 0, tipo = "SGT", reporte = "Trámites", prefijo = string.Empty, adicional = 0, seguridad = 0, categoria = "ROOT" },
                new() { id = 0, tipo = "CBR", reporte = "Cobro", prefijo = string.Empty, adicional = 0, seguridad = 0, categoria = "ROOT" },
                new() { id = 0, tipo = "RET", reporte = "Retenciones", prefijo = string.Empty, adicional = 0, seguridad = 0, categoria = "ROOT" },
                new() { id = 0, tipo = "ESP", reporte = "Especiales", prefijo = string.Empty, adicional = 0, seguridad = 0, categoria = "ROOT" }
            };
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Reportes_Informes_Usuarios_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            const string sql = @"
            SELECT
                RTRIM(cod_grupo) AS item,
                RTRIM(descripcion) AS descripcion
            FROM crd_grupos
            WHERE @filtro = ''
               OR cod_grupo LIKE @like
               OR descripcion LIKE @like
            ORDER BY descripcion;";

            return EjecutarDropdown(CodEmpresa, filtro, sql);
        }
        #endregion
        #region Informes - Generar Reporte

        private sealed record ReporteDef(
    int Id,
    string Tipo,
    string Reporte,
    string Prefijo,
    short Adicional,
    short Seguridad);

        public ErrorDto<CrReportesInformesGenerarResult> CR_Reportes_Informes_Generar(int CodEmpresa,CrReportesInformesGenerarRequest request)
        {
            try
            {
                if (request == null ||
                    request.id_reporte is null ||
                    request.id_reporte <= 0)
                {
                    return DbHelper.CreateErrorResponse(
                        "Debe seleccionar un reporte.",
                        -2,
                        new CrReportesInformesGenerarResult());
                }

                using var conn = DbHelper.OpenConnection(
                    _portalDB,
                    CodEmpresa);

                var reporte = ObtenerReporteDef(
                    conn,
                    request.id_reporte.Value);

                if (reporte == null)
                {
                    return DbHelper.CreateErrorResponse(
                        "No se encontró el reporte seleccionado.",
                        -2,
                        new CrReportesInformesGenerarResult());
                }

                if (!UsuarioTieneAccesoReporte(
                        conn,
                        reporte,
                        request.usuario_sesion))
                {
                    return DbHelper.CreateErrorResponse(
                        "El usuario actual no tiene acceso autorizado a este reporte, verifique.",
                        -2,
                        new CrReportesInformesGenerarResult());
                }

                var tipo = reporte.Tipo
                    .Trim()
                    .ToUpperInvariant();

                if (tipo == "RET")
                {
                    return GenerarReporteRET(
                        conn,
                        reporte,
                        request);
                }

                if (tipo is "CRD" or "SGT")
                {
                    return GenerarReporteCRD(
                        conn,
                        reporte,
                        request);
                }

                return DbHelper.CreateErrorResponse(
                    "El tipo de reporte seleccionado no está soportado para generación.",
                    -2,
                    new CrReportesInformesGenerarResult());
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CrReportesInformesGenerarResult());
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CrReportesInformesGenerarResult());
            }
        }

        private ErrorDto<CrReportesInformesGenerarResult> GenerarReporteCRD(SqlConnection conn, ReporteDef reporte, CrReportesInformesGenerarRequest request)
        {
            var builder = new ReporteSqlWhereBuilder(
                "vCRDCreditosReportes01");

            var titulo = BuildTitulo(reporte, request);
            var subtitulo = string.Empty;
            var filtro = string.Empty;

            AgregarFiltroFechas(
                builder,
                request.base_filtros,
                ref subtitulo);

            AgregarFiltroGestor(
                builder,
                request.adicionales,
                ref subtitulo);

            AgregarFiltroGeneralCRD(
                builder,
                request,
                reporte.Tipo,
                ref subtitulo,
                ref filtro);

            AgregarFiltroAdicionalesOperacion(
                builder,
                request.adicionales,
                ref filtro);

            AgregarFiltroEjecutivo(
                builder,
                request.f2,
                ref subtitulo);

            AgregarFiltroAdicionalDinamico(
                builder,
                reporte,
                request.f3,
                ref subtitulo,
                ref filtro);

            AgregarFiltroF2(
                builder,
                request.f2,
                ref subtitulo,
                ref filtro);

            AgregarFiltroF1(
                builder,
                request.f1,
                ref filtro);

            var texto = new ReporteTexto
            {
                Titulo = titulo,
                Subtitulo = subtitulo,
                Filtro = filtro,
                SqlWhere = builder.ToString()
            };

            var result = BuildGenerarResult(
                conn,
                reporte,
                request,
                texto);

            return DbHelper.CreateOkResponse(result);
        }

        private ErrorDto<CrReportesInformesGenerarResult> GenerarReporteRET(SqlConnection conn,ReporteDef reporte, CrReportesInformesGenerarRequest request)
        {
            var builder = new ReporteSqlWhereBuilder(
                "vCRDRetencionesReportes01");

            var titulo = BuildTitulo(reporte, request);
            var subtitulo = string.Empty;
            var filtro = string.Empty;

            AgregarFiltroFechas(
                builder,
                request.base_filtros,
                ref subtitulo);

            AgregarFiltroGeneralRET(
                builder,
                request,
                reporte.Tipo,
                ref subtitulo,
                ref filtro);

            AgregarFiltroRETAdicionales(
                builder,
                request.adicionales,
                ref filtro);

            AgregarFiltroF2(
                builder,
                request.f2,
                ref subtitulo,
                ref filtro);

            AgregarFiltroF1(
                builder,
                request.f1,
                ref filtro);

            var texto = new ReporteTexto
            {
                Titulo = titulo,
                Subtitulo = subtitulo,
                Filtro = filtro,
                SqlWhere = builder.ToString()
            };

            var result = BuildGenerarResult(
                conn,
                reporte,
                request,
                texto);

            return DbHelper.CreateOkResponse(result);
        }

        private static ReporteDef? ObtenerReporteDef(SqlConnection conn, int idReporte)
        {
            const string sql = @"
        SELECT
            id,
            RTRIM(tipo) AS tipo,
            RTRIM(reporte) AS reporte,
            RTRIM(prefijo) AS prefijo,
            ISNULL(adicional, 0) AS adicional,
            ISNULL(seguridad, 0) AS seguridad
        FROM CRD_REPORTES
        WHERE id = @idReporte;";

            return conn.QueryFirstOrDefault<ReporteDef>(sql, new { idReporte });
        }

        private static bool UsuarioTieneAccesoReporte(
            SqlConnection conn,
            ReporteDef reporte,
            string usuario)
        {
            if (reporte.Seguridad != 1)
            {
                return true;
            }

            const string sql = @"
        SELECT ISNULL(COUNT(*), 0)
        FROM CRD_REPORTES_GRP_AUT
        WHERE id = @idReporte
          AND cod_grupo IN (
                SELECT cod_grupo
                FROM crd_reportes_grp_usr
                WHERE usuario = @usuario
          );";

            var existe = conn.ExecuteScalar<int>(
                sql,
                new
                {
                    idReporte = reporte.Id,
                    usuario = NormalizeFiltro(usuario)
                });

            return existe > 0;
        }
        private sealed class ReporteTexto
        {
            public string Titulo { get; init; } = string.Empty;
            public string Subtitulo { get; init; } = string.Empty;
            public string Filtro { get; init; } = string.Empty;
            public string SqlWhere { get; init; } = string.Empty;
        }
        private CrReportesInformesGenerarResult BuildGenerarResult(SqlConnection conn, ReporteDef reporte,CrReportesInformesGenerarRequest request,ReporteTexto texto)
        {
            return new CrReportesInformesGenerarResult
            {
                tipo = reporte.Tipo,
                reporte = reporte.Reporte,
                prefijo = reporte.Prefijo,
                reporte_file_name = BuildReportFileName(reporte, request),
                selection_formula = texto.SqlWhere,
                fx_fecha = $"FECHA: {DateTime.Now:dd/MM/yyyy}",
                fx_empresa = ObtenerNombreEmpresa(conn),
                fx_usuario = $"USER: {request.usuario_sesion}",
                fx_titulo = texto.Titulo,
                fx_subtitulo = Trunc(texto.Subtitulo, 250),
                fx_filtro = Trunc(texto.Filtro, 250)
            };
        }

        private static string BuildTitulo(
            ReporteDef reporte,
            CrReportesInformesGenerarRequest request)
        {
            var tipo = request.base_filtros.tipo_reporte == "R" ? "Resumen" : "Detalle";

            if (reporte.Adicional == 3)
            {
                var corte = GetF3Value(request.f3, "corte");
                return $"{reporte.Reporte} [{corte}] {tipo}".Trim().ToUpperInvariant();
            }

            return $"{reporte.Reporte} : {tipo}".Trim().ToUpperInvariant();
        }

        private static string BuildReportFileName(
            ReporteDef reporte,
            CrReportesInformesGenerarRequest request)
        {
            var tipo = request.base_filtros.tipo_reporte == "R" ? "Resumen" : "Detalle";

            if (reporte.Adicional == 3)
            {
                var corte = GetF3Value(request.f3, "corte");
                return $"{reporte.Prefijo}_{corte}_{tipo}.rpt";
            }

            return $"{reporte.Prefijo}_{tipo}.rpt";
        }

        private static string ObtenerNombreEmpresa(SqlConnection conn)
        {
            const string sql = @"
        SELECT TOP 1
            RTRIM(ISNULL(nombre, '')) AS nombre
        FROM sif_empresa;";

            return conn.QueryFirstOrDefault<string>(sql) ?? string.Empty;
        }

        private static void AgregarFiltroFechas(ReporteSqlWhereBuilder builder,CrReportesInformesFiltroBaseDto filtro,ref string subtitulo)
        {
            if (filtro.todas_fechas == true)
            {
                subtitulo = "Historico";
                return;
            }

            if (!filtro.fecha_inicio.HasValue || !filtro.fecha_corte.HasValue)
            {
                subtitulo = "Historico";
                return;
            }

            var campo = filtro.base_reporte switch
            {
                "S" => "fechaSol",
                "R" => "fechares",
                "F" => "fechaforp",
                "D" => "fecha_inicio_Calculo",
                "U" => "Ultimo_Movimiento",
                _ => "fechaSol"
            };

            var texto = filtro.base_reporte switch
            {
                "S" => "Solicitadas",
                "R" => "Resueltas",
                "F" => "Formalizadas",
                "D" => "Desembolsos",
                "U" => "Ultimo Movimiento",
                _ => "Solicitadas"
            };

            var inicio = filtro.fecha_inicio.Value.Date;
            var fechaCorte = filtro.fecha_corte.Value.Date;
            var corteExclusivo = fechaCorte.AddDays(1);

            builder.AddRaw(
                $"{builder.Field(campo)} >= '{inicio:yyyyMMdd}' " +
                $"AND {builder.Field(campo)} < '{corteExclusivo:yyyyMMdd}'");

            subtitulo =
                $"{texto} entre {inicio:dd/MM/yyyy} y {fechaCorte:dd/MM/yyyy}";
        }

        private static void AgregarFiltroGestor(
            ReporteSqlWhereBuilder builder,
            CrReportesInformesFiltroAdicionalesDto adicionales,
            ref string subtitulo)
        {
            if (EsTodos(adicionales.gestor_cobros_ext))
            {
                return;
            }

            builder.AddEquals("GestionExternaUsuario", adicionales.gestor_cobros_ext);
            subtitulo += $" Gestionado por: {adicionales.gestor_cobros_ext} ¦";
        }

        private static void AgregarFiltroGeneralCRD(ReporteSqlWhereBuilder builder, CrReportesInformesGenerarRequest request, string tipoReporte, ref string subtitulo, ref string filtro)
        {
            var baseFiltros = request.base_filtros;
            var general = request.general;

            builder.AddEqualsIfNotTodos(
                "COD_DIVISA",
                general.divisa);

            subtitulo += $" ¦ Divisa: {general.divisa}";

            if (!EsTodos(general.especial))
            {
                if (general.especial == "INTERNA")
                {
                    builder.AddRaw(
                        $"{builder.Field("Linea_Interna")} = 1");
                }
                else if (general.especial == "ADMINISTRADA")
                {
                    builder.AddRaw(
                        $"{builder.Field("Linea_Interna")} = 0");
                }
            }

            subtitulo += $" ¦ Listado: {general.especial}";

            AgregarEstadoBase(
                builder,
                baseFiltros,
                tipoReporte,
                ref subtitulo);

            AgregarEstadoPersona(
                builder,
                baseFiltros);

            builder.AddEqualsIfNotTodos(
                "cod_oficina_R",
                baseFiltros.oficina);

            subtitulo += $", Oficina: {baseFiltros.oficina}";

            builder.AddEqualsIfNotTodos(
                "garantia",
                general.garantia);

            subtitulo += $", Garantía: {general.garantia}";

            builder.AddEqualsIfNotTodos(
                "GRUPOREC",
                request.f1.usuario);

            filtro = $", Grupo: {request.f1.usuario}";

            AgregarFiltroLineas(
                builder,
                general,
                ref filtro);

            builder.AddEqualsIfNotTodos(
                "recurso",
                general.recurso);

            filtro += $", Recurso: {general.recurso}";

            builder.AddEqualsIfNotTodos(
                "cod_destino",
                general.destino);

            filtro += $", Destino: {general.destino}";

            builder.AddNumberEqualsIfNotTodos(
                "id_comite",
                general.comite);

            builder.AddNumberEqualsIfNotTodos(
                "cod_institucion",
                general.institucion);

            builder.AddNumberEqualsIfNotTodos(
                "cod_deductora",
                general.deductora);
        }

        private static void AgregarFiltroGeneralRET(ReporteSqlWhereBuilder builder, CrReportesInformesGenerarRequest request, string tipoReporte, ref string subtitulo, ref string filtro)
        {
            var baseFiltros = request.base_filtros;
            var general = request.general;

            AgregarEstadoBase(
                builder,
                baseFiltros,
                tipoReporte,
                ref subtitulo);

            AgregarEstadoPersona(
                builder,
                baseFiltros);

            builder.AddEqualsIfNotTodos(
                "cod_oficina_R",
                baseFiltros.oficina);

            subtitulo += $", Oficina: {baseFiltros.oficina}";

            builder.AddEqualsIfNotTodos(
                "garantia",
                general.garantia);

            subtitulo += $", Garantía: {general.garantia}";

            builder.AddEqualsIfNotTodos(
                "GRUPOREC",
                request.f1.usuario);

            filtro = $", Grupo: {request.f1.usuario}";

            AgregarFiltroLineas(
                builder,
                general,
                ref filtro);

            builder.AddEqualsIfNotTodos(
                "cod_destino",
                general.destino);

            filtro += $", Destino: {general.destino}";

            builder.AddNumberEqualsIfNotTodos(
                "id_comite",
                general.comite);

            builder.AddNumberEqualsIfNotTodos(
                "cod_institucion",
                general.institucion);

            builder.AddNumberEqualsIfNotTodos(
                "cod_deductora",
                general.deductora);
        }

        private static void AgregarEstadoBase(ReporteSqlWhereBuilder builder, CrReportesInformesFiltroBaseDto baseFiltros, string tipoReporte, ref string subtitulo)
        {
            var tipo = NormalizeFiltro(tipoReporte)
                .ToUpperInvariant();

            if (tipo == "SGT")
            {
                AgregarEstadoSolicitud(
                    builder,
                    baseFiltros,
                    ref subtitulo);

                return;
            }

            if (tipo is "CRD" or "CBR" or "RET")
            {
                AgregarEstadoOperacion(
                    builder,
                    baseFiltros,
                    ref subtitulo);
            }
        }
        private static void AgregarEstadoSolicitud(ReporteSqlWhereBuilder builder, CrReportesInformesFiltroBaseDto baseFiltros, ref string subtitulo)
        {
            var estado = NormalizeFiltro(
                baseFiltros.estado_solicitud)
                .ToUpperInvariant();

            if (estado == "T")
            {
                subtitulo += ", Estado Solicitud: Todas";
                return;
            }

            if (string.IsNullOrWhiteSpace(estado))
            {
                return;
            }

            builder.AddEquals(
                "estadosol",
                estado);

            var descripcion = estado switch
            {
                "R" => "Recibida",
                "P" => "Pendiente",
                "F" => "Formalizada",
                "N" => "Nula",
                "A" => "Aprobada",
                "D" => "Denegada",
                _ => estado
            };

            subtitulo +=
                $", Estado Solicitud: {descripcion}";
        }
        private static void AgregarEstadoOperacion(ReporteSqlWhereBuilder builder, CrReportesInformesFiltroBaseDto baseFiltros, ref string subtitulo)
        {
            var estado = NormalizeFiltro(
                baseFiltros.estado_operacion)
                .ToUpperInvariant();

            if (estado == "T")
            {
                builder.AddRaw(
                    $"({builder.Field("estado")} = 'A' OR " +
                    $"{builder.Field("estado")} = 'C')");

                subtitulo +=
                    ", Estado Operación : Todas (Activas/Canceladas)";

                return;
            }

            if (string.IsNullOrWhiteSpace(estado))
            {
                return;
            }

            builder.AddEquals(
                "estado",
                estado);

            var descripcion = estado switch
            {
                "A" => "Activa",
                "C" => "Cancelada",
                "N" => "Nulas",
                _ => estado
            };

            subtitulo +=
                $", Estado Operación: {descripcion}";
        }
        private static void AgregarEstadoPersona(ReporteSqlWhereBuilder builder, CrReportesInformesFiltroBaseDto baseFiltros)
        {
            if (EsTodos(baseFiltros.estado_persona))
            {
                return;
            }

            if (baseFiltros.estado_persona == "X")
            {
                builder.AddRaw($"({builder.Field("estadoactual")} = 'A' OR {builder.Field("estadoactual")} = 'P')");
                return;
            }

            builder.AddEquals("estadoactual", baseFiltros.estado_persona);
        }

        private static void AgregarFiltroLineas(
            ReporteSqlWhereBuilder builder,
            CrReportesInformesFiltroGeneralDto general,
            ref string filtro)
        {
            if (general.todas_lineas)
            {
                filtro += ", Todas las Líneas";
                return;
            }

            builder.AddEquals("Codigo", general.linea);
            filtro += $", Línea: {general.linea.ToUpperInvariant()}";
        }

        private static void AgregarFiltroAdicionalesOperacion( ReporteSqlWhereBuilder builder,CrReportesInformesFiltroAdicionalesDto adicionales,ref string filtro)
        {
            AgregarFiltroProceso(builder, adicionales, ref filtro);
            AgregarFiltroTipoOperacion(builder, adicionales, ref filtro);
            AgregarFiltroCobro(builder, adicionales, ref filtro);
            AgregarFiltroTipoTasa(builder, adicionales, ref filtro);
            AgregarFiltroAutorizacion(builder, adicionales, ref filtro);
            AgregarFiltroPlazos(builder, adicionales, ref filtro);
            AgregarFiltroTasas(builder, adicionales, ref filtro);
            AgregarFiltroPrimerDeduccion(builder, adicionales, ref filtro);
            AgregarFiltroUltimoMovimiento(builder, adicionales, ref filtro);
        }
        private static void AgregarFiltroProceso(ReporteSqlWhereBuilder builder,CrReportesInformesFiltroAdicionalesDto adicionales,ref string filtro)
        {
            if (EsTodos(adicionales.proceso))
            {
                return;
            }

            builder.AddEquals(
                "Proceso",
                adicionales.proceso);

            filtro += $", Proceso: {adicionales.proceso}";
        }
        private static void AgregarFiltroTipoOperacion(ReporteSqlWhereBuilder builder, CrReportesInformesFiltroAdicionalesDto adicionales, ref string filtro)
        {
            if (EsTodas(adicionales.tipos_de))
            {
                return;
            }

            builder.AddRaw(
                adicionales.tipos_de == "O"
                    ? $"{builder.Field("REFERENCIA")} IS NULL"
                    : $"{builder.Field("REFERENCIA")} IS NOT NULL");

            filtro += $", Operaciones: {adicionales.tipos_de}";
        }
        private static void AgregarFiltroCobro( ReporteSqlWhereBuilder builder,CrReportesInformesFiltroAdicionalesDto adicionales,ref string filtro)
        {
            if (EsTodos(adicionales.cobro_en))
            {
                return;
            }

            builder.AddEquals(
                "Ind_deduce_Planilla",
                adicionales.cobro_en == "C" ? "N" : "S");

            filtro += $", Cobro vía: {adicionales.cobro_en}";
        }
        private static void AgregarFiltroTipoTasa(ReporteSqlWhereBuilder builder,CrReportesInformesFiltroAdicionalesDto adicionales, ref string filtro)
        {
            if (EsTodas(adicionales.tipo))
            {
                return;
            }

            builder.AddRaw(
                adicionales.tipo == "I"
                    ? $"{builder.Field("TBP_PuntosAdd")} IS NOT NULL"
                    : $"{builder.Field("TBP_PuntosAdd")} IS NULL");

            filtro += $", Tipo Tasa: {adicionales.tipo}";
        }
        private static void AgregarFiltroAutorizacion(ReporteSqlWhereBuilder builder,CrReportesInformesFiltroAdicionalesDto adicionales, ref string filtro)
        {
            if (EsTodas(adicionales.autorizaciones))
            {
                return;
            }

            builder.AddRaw(
                adicionales.autorizaciones == "N"
                    ? $"{builder.Field("Autoriza_Fecha")} IS NOT NULL"
                    : $"{builder.Field("Autoriza_Fecha")} IS NULL");

            filtro += $", Autoriza: {adicionales.autorizaciones}";
        }
        private static void AgregarFiltroPlazos( ReporteSqlWhereBuilder builder,CrReportesInformesFiltroAdicionalesDto adicionales,
    ref string filtro)
        {
            if (adicionales.todos_plazos)
            {
                return;
            }

            builder.AddRaw(
                $"{builder.Field(Plazo)} >= {adicionales.plazo_desde} " +
                $"AND {builder.Field(Plazo)} <= {adicionales.plazo_hasta}");

            filtro +=
                $", Plazos: {adicionales.plazo_desde} - {adicionales.plazo_hasta}";
        }
        private static void AgregarFiltroTasas(
    ReporteSqlWhereBuilder builder,
    CrReportesInformesFiltroAdicionalesDto adicionales,
    ref string filtro)
        {
            if (adicionales.todas_tasas)
            {
                return;
            }

            builder.AddRaw(
                $"{builder.Field(Interesv)} >= " +
                $"{ToSqlNumber(adicionales.tasa_desde)} " +
                $"AND {builder.Field(Interesv)} <= " +
                $"{ToSqlNumber(adicionales.tasa_hasta)}");

            filtro +=
                $", Tasas: {adicionales.tasa_desde} - {adicionales.tasa_hasta}";
        }
        private static void AgregarFiltroPrimerDeduccion(ReporteSqlWhereBuilder builder,CrReportesInformesFiltroAdicionalesDto adicionales, ref string filtro)
        {
            if (adicionales.todas_primer_deduccion)
            {
                return;
            }

            var operador = SafeOperator(
                adicionales.primer_deduccion_operador);

            var valor = ToSqlValue(
                adicionales.primer_deduccion);

            builder.AddRaw(
                $"{builder.Field("PriDeduc")} {operador} {valor}");

            filtro +=
                $", Pri.Deduc. {adicionales.primer_deduccion_operador} " +
                $"{adicionales.primer_deduccion}";
        }
        private static void AgregarFiltroUltimoMovimiento(ReporteSqlWhereBuilder builder,CrReportesInformesFiltroAdicionalesDto adicionales, ref string filtro)
        {
            if (adicionales.todas_ult_mov)
            {
                return;
            }

            var operador = SafeOperator(
                adicionales.ult_mov_operador);

            var valor = ToSqlValue(
                adicionales.ult_mov);

            builder.AddRaw(
                $"{builder.Field("FecUlt")} {operador} {valor}");

            filtro +=
                $", Ult.Mov. {adicionales.ult_mov_operador} " +
                $"{adicionales.ult_mov}";
        }
        private static void AgregarFiltroRETAdicionales(ReporteSqlWhereBuilder builder,CrReportesInformesFiltroAdicionalesDto adicionales,ref string filtro)
        {
            if (!EsTodos(adicionales.cobro_en))
            {
                builder.AddEquals("Ind_deduce_Planilla", adicionales.cobro_en == "C" ? "N" : "S");
                filtro += $", Cobro vía: {adicionales.cobro_en}";
            }

            if (!adicionales.todos_plazos)
            {
                builder.AddRaw($"{builder.Field(Plazo)} >= {adicionales.plazo_desde} AND {builder.Field(Plazo)} <= {adicionales.plazo_hasta}");
                filtro += $", Plazos: {adicionales.plazo_desde} - {adicionales.plazo_hasta}";
            }

            if (!adicionales.todas_tasas)
            {
                builder.AddRaw($"{builder.Field(Interesv)} >= {ToSqlNumber(adicionales.tasa_desde)} AND {builder.Field(Interesv)} <= {ToSqlNumber(adicionales.tasa_hasta)}");
                filtro += $", Tasas: {adicionales.tasa_desde} - {adicionales.tasa_hasta}";
            }

            if (!adicionales.todas_primer_deduccion)
            {
                builder.AddRaw($"{builder.Field("PriDeduc")} {SafeOperator(adicionales.primer_deduccion_operador)} {ToSqlValue(adicionales.primer_deduccion)}");
                filtro += $", Pri.Deduc. {adicionales.primer_deduccion_operador} {adicionales.primer_deduccion}";
            }

            if (!adicionales.todas_ult_mov)
            {
                builder.AddRaw($"{builder.Field("FecUlt")} {SafeOperator(adicionales.ult_mov_operador)} {ToSqlValue(adicionales.ult_mov)}");
                filtro += $", Ult.Mov. {adicionales.ult_mov_operador} {adicionales.ult_mov}";
            }
        }

        private static void AgregarFiltroEjecutivo(ReporteSqlWhereBuilder builder,CrReportesInformesFiltroF2Dto f2,ref string subtitulo)
        {
            if (!int.TryParse(f2.ejecutivo_colocador, out var ejecutivo) || ejecutivo <= 0)
            {
                return;
            }

            builder.AddRaw($"{builder.Field("Id_Promotor")} = {ejecutivo}");
            subtitulo += $", Ejecutivo: {f2.ejecutivo_colocador_desc}";
        }

        private static void AgregarFiltroAdicionalDinamico(ReporteSqlWhereBuilder builder,ReporteDef reporte,CrReportesInformesFiltroF3Dto f3, ref string subtitulo,ref string filtro)
        {
            if (reporte.Adicional == 1)
            {
                AgregarFiltroRequisitos(builder, f3, ref subtitulo, ref filtro);
                return;
            }

            if (reporte.Adicional == 2)
            {
                AgregarFiltroCausas(builder, f3, ref subtitulo, ref filtro);
            }
        }
        private static void AgregarFiltroRequisitos(ReporteSqlWhereBuilder builder, CrReportesInformesFiltroF3Dto f3,ref string subtitulo,ref string filtro)
        {
            var marca = GetF3Value(f3, "marca");
            var requisito = GetF3Value(f3, "requisito");

            if (int.TryParse(marca, out var estado))
            {
                builder.AddRaw($"OPERACION_REQUISITOS.ESTADO = {estado}");
            }

            if (!EsTodos(requisito))
            {
                builder.AddRaw(
                    $"OPERACION_REQUISITOS.COD_REQUISITO = '{EscapeSql(requisito)}'");
            }

            var marcaDescripcion = EstadoRequisitoDescripcion(marca);

            filtro +=
                $", Requisitos: {requisito} ESTADO : {marcaDescripcion}";

            subtitulo +=
                $", Requisito Estado: {marcaDescripcion}";
        }

        private static string EstadoRequisitoDescripcion(string marca)
        {
            return NormalizeFiltro(marca) switch
            {
                "1" => "Cumple",
                "2" => "No Cumple",
                "0" => "En Blanco",
                _ => marca
            };
        }

        private static string TipoCausaDescripcion(string tipo)
        {
            return NormalizeFiltro(tipo).ToUpperInvariant() switch
            {
                "P" => "Pendientes",
                "D" => "Denegadas",
                _ => tipo
            };
        }
        private static void AgregarFiltroCausas(ReporteSqlWhereBuilder builder, CrReportesInformesFiltroF3Dto f3,ref string subtitulo,ref string filtro)
        {
            var tipoCausa = GetF3Value(f3, "tipo_causa");
            var causa = GetF3Value(f3, "causa");

            if (!string.IsNullOrWhiteSpace(tipoCausa))
            {
                builder.AddRaw(
                    $"OPERACION_GESTION.TIPO = '{EscapeSql(tipoCausa)}'");
            }

            if (!EsTodos(causa))
            {
                builder.AddRaw(
                    $"OPERACION_GESTION.COD_CAUSAS = '{EscapeSql(causa)}'");
            }

            filtro +=
                $", Causa de Rechazo: {causa} TIPO : {TipoCausaDescripcion(tipoCausa)}";

            subtitulo +=
                $", Tipo de Causa: {TipoCausaDescripcion(tipoCausa)}";
        }
        private static void AgregarFiltroF2( ReporteSqlWhereBuilder builder,CrReportesInformesFiltroF2Dto f2,ref string subtitulo, ref string filtro)
        {
            if (!EsTodos(f2.sexo))
            {
                builder.AddEquals("sexo", f2.sexo);
            }

            subtitulo += $", Sexo: {f2.sexo}";

            builder.AddEqualsIfNotTodos("EstadoCivil", f2.estado_civil);
            subtitulo += $", Estado Civil: {f2.estado_civil}";

            builder.AddEqualsIfNotTodos("EstadoLaboral", f2.condicion_laboral);
            filtro += $", Laboral: {f2.condicion_laboral}";

            builder.AddNumberEqualsIfNotTodos("cod_sector", f2.sector);
            filtro += $", Sector: {f2.sector}";

            builder.AddNumberEqualsIfNotTodos("cod_profesion", f2.profesion);
            filtro += $", Profesión: {f2.profesion}";
        }

        private static void AgregarFiltroF1(ReporteSqlWhereBuilder builder,CrReportesInformesFiltroF1Dto f1,ref string filtro)
        {
            builder.AddEqualsIfNotTodos("cod_zona", f1.zona);
            filtro += $", Zona: {f1.zona}";

            if (!f1.todas_provincias && !string.IsNullOrWhiteSpace(f1.provincia))
            {
                builder.AddEquals("provincia", f1.provincia);
                filtro += $", Provincia: {f1.provincia}";
            }
            else
            {
                filtro += ", Provincia: Todas";
            }

            if (!f1.todos_cantones && !string.IsNullOrWhiteSpace(f1.canton))
            {
                builder.AddEquals("canton", f1.canton);
                filtro += $" [{f1.canton}]";
            }

            if (!f1.todos_distritos && !string.IsNullOrWhiteSpace(f1.distrito))
            {
                builder.AddEquals("distrito", f1.distrito);
                filtro += $" [{f1.distrito}]";
            }

            if (!f1.todas_unidades_programaticas && !string.IsNullOrWhiteSpace(f1.unidad_programatica))
            {
                builder.AddEquals("DeptCod", f1.unidad_programatica);
                filtro += $", Dept.: {f1.unidad_programatica}";

                if (!f1.todas_unidades_trabajo && !string.IsNullOrWhiteSpace(f1.unidad_trabajo))
                {
                    builder.AddEquals("SecCod", f1.unidad_trabajo);
                    filtro += $" [{f1.unidad_trabajo}]";
                }
            }
        }

        private static string GetF3Value(CrReportesInformesFiltroF3Dto f3, string key)
        {
            return f3.valores.TryGetValue(key, out var value)
                ? NormalizeFiltro(value)
                : string.Empty;
        }

        private static bool EsTodos(string? value)
        {
            var normalized = NormalizeFiltro(value).ToUpperInvariant();
            return normalized.Length == 0 || normalized == TODOS || normalized == "T";
        }

        private static bool EsTodas(string? value)
        {
            var normalized = NormalizeFiltro(value).ToUpperInvariant();
            return normalized.Length == 0 || normalized == TODAS;
        }

        private static string Trunc(string value, int length)
        {
            value ??= string.Empty;
            return value.Length <= length ? value : value[..length];
        }

        private static string EscapeSql(string value)
        {
            return NormalizeFiltro(value).Replace("'", "''");
        }

        private static string SafeOperator(string operador)
        {
            var op = NormalizeFiltro(operador);
            return op is ">" or "<" or "=" ? op : "=";
        }

        private static string ToSqlNumber(decimal value)
        {
            return value.ToString(
                System.Globalization.CultureInfo.InvariantCulture);
        }

        private static string ToSqlNumber(decimal? value)
        {
            return (value ?? 0m).ToString(
                System.Globalization.CultureInfo.InvariantCulture);
        }

        private static string ToSqlValue(string value)
        {
            var text = NormalizeFiltro(value);

            if (decimal.TryParse(
                text,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var number))
            {
                return ToSqlNumber(number);
            }

            if (DateTime.TryParseExact(
                text,
                new[]
                {
            "dd/MM/yyyy",
            "yyyy-MM-dd",
            "yyyyMMdd"
                },
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
            {
                return $"'{date:yyyyMMdd}'";
            }

            return $"'{EscapeSql(text)}'";
        }

        private sealed class ReporteSqlWhereBuilder
        {
            private readonly string _alias;
            private readonly List<string> _filters = new();

            public ReporteSqlWhereBuilder(string alias)
            {
                _alias = alias;
            }

            public string Field(string field)
            {
                return $"{_alias}.{field}";
            }

            public void AddRaw(string condition)
            {
                if (!string.IsNullOrWhiteSpace(condition))
                {
                    _filters.Add(condition);
                }
            }

            public void AddEquals(string field, string value)
            {
                AddRaw(
                    $"{Field(field)} = '{EscapeSql(value)}'");
            }

            public void AddEqualsIfNotTodos(string field,string value)
            {
                if (!EsTodos(value))
                {
                    AddEquals(field, value);
                }
            }

            public void AddNumberEqualsIfNotTodos( string field,string value)
            {
                if (EsTodos(value))
                {
                    return;
                }

                if (decimal.TryParse(value, out var number))
                {
                    AddRaw(
                        $"{Field(field)} = {ToSqlNumber(number)}");
                }
            }

            public override string ToString()
            {
                return _filters.Count == 0
                    ? "WHERE 1 = 1"
                    : "WHERE " + string.Join(" AND ", _filters);
            }
        }

        #endregion
    }
}