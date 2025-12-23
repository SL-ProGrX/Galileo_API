using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier
{
    public class MFndFuncionesDb
    {
        private readonly IConfiguration _config;

        public MFndFuncionesDb(IConfiguration config)
        {
            _config = config;
        }

        // ========= Helpers comunes =========

        private SqlConnection CreateEmpresaConnection(int codEmpresa)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            if (string.IsNullOrWhiteSpace(stringConn))
                throw new InvalidOperationException("Cadena de conexión de empresa no configurada.");

            return new SqlConnection(stringConn);
        }

        private T ExecuteScalarOrDefault<T>(int codEmpresa, string sql, object? parameters, T defaultValue)
        {
            try
            {
                using var connection = CreateEmpresaConnection(codEmpresa);
                return connection.QueryFirstOrDefault<T>(sql, parameters) ?? defaultValue;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        private ErrorDto<List<T>> QueryListWithError<T>(
            int codEmpresa,
            string sql,
            object? parameters = null)
        {
            var response = new ErrorDto<List<T>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<T>()
            };

            try
            {
                using var connection = CreateEmpresaConnection(codEmpresa);
                response.Result = connection.Query<T>(sql, parameters).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        // ========= Funciones de negocio =========

        public static string fxgFNDTipoPago(string vModo, string vTipo)
        {
            if (string.IsNullOrWhiteSpace(vTipo))
                return string.Empty;

            var tipo = vTipo.Trim().ToUpper();

            if (vModo == "D")
            {
                return tipo switch
                {
                    "TRANSFERENCIA" => "TE",
                    "CHEQUE"        => "CK",
                    _               => string.Empty
                };
            }

            if (vModo == "C")
            {
                return tipo switch
                {
                    "TE" => "Transferencia",
                    "CK" => "Cheque",
                    _    => string.Empty
                };
            }

            return string.Empty;
        }

        public decimal fxgFNDCodigoMulta(int CodEmpresa, int vOperadora, string vPlan, int vContrato, decimal vMonto)
        {
            const string query = "select dbo.fxFNDMulta(@vOperadora, @vPlan, @vContrato, @vMonto) as Multa";

            return ExecuteScalarOrDefault(
                CodEmpresa,
                query,
                new { vOperadora, vPlan, vContrato, vMonto },
                0m
            );
        }

        public static string fxTipoDocumento(string vTipo)
        {
            return vTipo switch
            {
                // códigos -> descripción
                "CK" => "Cheque",
                "TE" => "Transferencia",
                "EF" => "Efectivo",
                "RE" => "Efectivo",
                "ND" => "Nota Debito",
                "NC" => "Nota Credito",
                "OT" => "Otro...",
                "CD" => "Ctrl Desembolsos",
                "CP" => "Proveedor",
                "RC" => "Retiro en Caja",
                "FD" => "Fondo Transitorio",
                "TS" => "Transferencia SINPE",

                // descripción -> códigos
                "Cheque"                => "CK",
                "Transferencia"         => "TE",
                "Efectivo"              => "EF",
                "Nota Debito"           => "ND",
                "Nota Credito"          => "NC",
                "Otro..."               => "OT",
                "Ctrl Desembolsos"      => "CD",
                "Proveedor"             => "CP",
                "Retiro en Caja"        => "RC",
                "Fondo Transitorio"     => "FD",
                "Transferencia SINPE"   => "TS",

                _ => string.Empty
            };
        }

        public string fxFndParametro(int CodEmpresa, string pParametro)
        {
            const string query = "select valor from Fnd_parametros where cod_parametro = @pParametro";

            return ExecuteScalarOrDefault(
                CodEmpresa,
                query,
                new { pParametro },
                string.Empty
            );
        }

        /// <summary>
        /// Metodo para obtener los cupones de un contrato
        /// </summary>
        public ErrorDto<List<FndContratosCuponesData>> sbFnd_Contratos_Cupones(
            int CodEmpresa,
            int pOperadora,
            string pPlan,
            long pContrato)
        {
            const string query = @"
                select *
                from vFnd_Contratos_Cupones
                where cod_operadora = @operadora
                  and cod_plan      = @plan 
                  and cod_contrato  = @contrato
                order by Fecha_Vence";

            return QueryListWithError<FndContratosCuponesData>(
                CodEmpresa,
                query,
                new
                {
                    operadora = pOperadora,
                    plan = pPlan,
                    contrato = pContrato
                }
            );
        }

        /// <summary>
        /// Metodo para obtener la bitacora de cambios de un contrato
        /// </summary>
        public ErrorDto<List<FndContratoBitacoraData>> sbFnd_Contratos_Bitacora(
            int CodEmpresa,
            int pOperadora,
            string pPlan,
            long pContrato)
        {
            const string query = @"
                select 
                    C.*,
                    S.cedula,
                    S.nombre,
                    M.Descripcion as MovimientoDesc,
                    case when C.revisado_fecha is null then 0 else 1 end as Revisado
                from fnd_contratos_cambios C 
                inner join fnd_contratos X 
                    on C.cod_operadora = X.cod_operadora
                   and C.cod_plan      = X.cod_plan 
                   and C.cod_contrato  = X.cod_contrato
                inner join Socios S 
                    on X.cedula = S.cedula
                inner join US_MOVIMIENTOS_BE M 
                    on C.Movimiento = M.Movimiento 
                   and M.modulo     = 18
                where C.cod_operadora = @operadora
                  and C.cod_plan      = @plan 
                  and C.cod_contrato  = @contrato
                order by C.fecha desc";

            return QueryListWithError<FndContratoBitacoraData>(
                CodEmpresa,
                query,
                new
                {
                    operadora = pOperadora,
                    plan = pPlan,
                    contrato = pContrato
                }
            );
        }
    }
}