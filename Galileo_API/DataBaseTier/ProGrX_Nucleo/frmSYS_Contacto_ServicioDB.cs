using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSysContactoServicioDB
    {
        private const string BaseConnStringName = "BaseConnString";
        private const string ProcConsultaPadron = "spSYS_Consulta_Padron";

        private readonly string _connStr;

        public FrmSysContactoServicioDB(IConfiguration config)
        {
            _connStr =
                config.GetConnectionString(BaseConnStringName)
                ?? config[$"ConnectionStrings:{BaseConnStringName}"]
                ?? config[BaseConnStringName]
                ?? throw new InvalidOperationException($"Connection string '{BaseConnStringName}' is not configured.");

            if (string.IsNullOrWhiteSpace(_connStr))
                throw new InvalidOperationException($"Connection string '{BaseConnStringName}' is not configured.");
        }

        private static string Normalize(string? value)
            => (value ?? string.Empty).Trim().ToUpperInvariant();

        private static DynamicParameters BuildPadronParams(string identificacion, string codPais, string tipoInfo)
        {
            var p = new DynamicParameters();
            p.Add("@Identificacion", Normalize(identificacion), DbType.String, size: 20);
            p.Add("@Pais", Normalize(string.IsNullOrWhiteSpace(codPais) ? "CRC" : codPais), DbType.String, size: 10);
            p.Add("@TInfo", tipoInfo, DbType.String, size: 10);
            return p;
        }

        /// <summary>
        /// Devuelve la información general de una persona por identificación y país.
        /// </summary>
        public ErrorDto<SysContactoServicioGeneralData?> SysContactoServicio_General_Obtener(
            int CodEmpresa,
            string identificacion,
            string codPais = "CRC")
        {
            var parameters = BuildPadronParams(identificacion, codPais, "General");

            var result = DbHelper.ExecuteStoredProcedureSingle<SysContactoServicioGeneralData>(
                _connStr,
                ProcConsultaPadron,
                default,
                parameters);

            if (result.Code != 0)
                return result;

            if (result.Result == null)
                return DbHelper.CreateErrorResponse<SysContactoServicioGeneralData?>(
                    "No existe información general para la identificación indicada.",
                    -1,
                    default);

            return result;
        }

        /// <summary>
        /// Mantiene compatibilidad con firma antigua; devuelve lista con un solo registro si existe.
        /// </summary>
        public ErrorDto<List<SysContactoServicioGeneralData>> SysContactoServicio_Obtener(
            int CodEmpresa,
            string identificacion,
            string codPais,
            FiltrosLazyLoadData filtros)
        {
            var general = SysContactoServicio_General_Obtener(CodEmpresa, identificacion, codPais);

            if (general.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    general.Description ?? "Ocurrió un error desconocido.",
                    general.Code ?? -1,
                    new List<SysContactoServicioGeneralData>());
            }

            var lista = general.Result == null
                ? new List<SysContactoServicioGeneralData>()
                : new List<SysContactoServicioGeneralData> { general.Result };

            return DbHelper.CreateOkResponse(lista);
        }

        public ErrorDto<SysContactoServicioTelefonoLista> SysContactoServicio_Telefonos_Lista_Obtener(
            int CodEmpresa,
            string identificacion,
            string codPais,
            FiltrosLazyLoadData filtros)
        {
            var result = SysContactoServicio_Telefonos_Obtener(CodEmpresa, identificacion, codPais);

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? "Ocurrió un error desconocido.",
                    result.Code ?? -1,
                    new SysContactoServicioTelefonoLista
                    {
                        total = 0,
                        lista = new List<SysContactoServicioTelefonoData>()
                    });
            }

            return DbHelper.CreateOkResponse(new SysContactoServicioTelefonoLista
            {
                total = result.Result?.Count ?? 0,
                lista = result.Result ?? new List<SysContactoServicioTelefonoData>()
            });
        }

        public ErrorDto<List<SysContactoServicioTelefonoData>> SysContactoServicio_Telefonos_Obtener(
            int CodEmpresa,
            string identificacion,
            string codPais)
        {
            var parameters = BuildPadronParams(identificacion, codPais, "Telefonos");

            return DbHelper.ExecuteStoredProcedureList<SysContactoServicioTelefonoData>(
                _connStr,
                ProcConsultaPadron,
                parameters);
        }

        public ErrorDto<SysContactoServicioDireccionLista> SysContactoServicio_Direcciones_Lista_Obtener(
            int CodEmpresa,
            string identificacion,
            string codPais,
            FiltrosLazyLoadData filtros)
        {
            var result = SysContactoServicio_Direcciones_Obtener(CodEmpresa, identificacion, codPais);

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? "Ocurrió un error desconocido.",
                    result.Code ?? -1,
                    new SysContactoServicioDireccionLista
                    {
                        total = 0,
                        lista = new List<SysContactoServicioDireccionData>()
                    });
            }

            return DbHelper.CreateOkResponse(new SysContactoServicioDireccionLista
            {
                total = result.Result?.Count ?? 0,
                lista = result.Result ?? new List<SysContactoServicioDireccionData>()
            });
        }

        public ErrorDto<List<SysContactoServicioDireccionData>> SysContactoServicio_Direcciones_Obtener(
            int CodEmpresa,
            string identificacion,
            string codPais)
        {
            var parameters = BuildPadronParams(identificacion, codPais, "Direccion");

            return DbHelper.ExecuteStoredProcedureList<SysContactoServicioDireccionData>(
                _connStr,
                ProcConsultaPadron,
                parameters);
        }

        public ErrorDto<SysContactoServicioEmpresaLista> SysContactoServicio_Empresas_Lista_Obtener(
            int CodEmpresa,
            string identificacion,
            string codPais,
            FiltrosLazyLoadData filtros)
        {
            var result = SysContactoServicio_Empresas_Obtener(CodEmpresa, identificacion, codPais);

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? "Ocurrió un error desconocido.",
                    result.Code ?? -1,
                    new SysContactoServicioEmpresaLista
                    {
                        total = 0,
                        lista = new List<SysContactoServicioEmpresaData>()
                    });
            }

            return DbHelper.CreateOkResponse(new SysContactoServicioEmpresaLista
            {
                total = result.Result?.Count ?? 0,
                lista = result.Result ?? new List<SysContactoServicioEmpresaData>()
            });
        }

        public ErrorDto<List<SysContactoServicioEmpresaData>> SysContactoServicio_Empresas_Obtener(
            int CodEmpresa,
            string identificacion,
            string codPais)
        {
            var parameters = BuildPadronParams(identificacion, codPais, "Empresas");

            return DbHelper.ExecuteStoredProcedureList<SysContactoServicioEmpresaData>(
                _connStr,
                ProcConsultaPadron,
                parameters);
        }

        /// <summary>
        /// Este método no lo cubre spSYS_Consulta_Padron.
        /// Debe mantenerse con SQL directo o moverse a otro SP específico.
        /// </summary>
        public ErrorDto<SysContactoServicioPersonaLookupLista> SysContactoServicio_Personas_Lista_Buscar(
            int CodEmpresa,
            string codPais,
            FiltrosLazyLoadData filtros)
        {
            return DbHelper.CreateErrorResponse(
                "SysContactoServicio_Personas_Lista_Buscar no está soportado por spSYS_Consulta_Padron. Cree un SP específico para búsqueda/paginación.",
                -1,
                new SysContactoServicioPersonaLookupLista
                {
                    total = 0,
                    lista = new List<SysContactoServicioPersonaLookupDto>()
                });
        }
    }
}